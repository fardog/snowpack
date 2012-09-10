/*
 * Copyright 2012 Far Dog LLC or its affiliates. All Rights Reserved.
 * 
 * Licensed under the GNU General Public License, Version 3.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 * 
 *  http://www.gnu.org/licenses/gpl-3.0.txt
 * 
 * or in the "gpl-3.0" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;

namespace snowpack
{
	public class FDOperationQueue
	{
		private bool stopQueue;
		private bool pauseQueue;
		public bool wasStopped = false;
		public bool paused = false;
		private FDDataStore DataStore;
		private FDUserSettings settings;
		private FDOperationLog log;
		
		private ConcurrentQueue<FDQueueItem> uploadQueue;
		private ConcurrentQueue<FDQueueItem> downloadQueue;
		public ConcurrentStack<FDQueueItem> current;
		public ConcurrentStack<FDQueueItem> finished;
		
		public FDOperationQueue (FDDataStore store, FDUserSettings userset, FDOperationLog oplog)
		{
			stopQueue = false;
			DataStore = store;
			settings = userset;
			log = oplog;
			
			uploadQueue = new ConcurrentQueue<FDQueueItem>();
			downloadQueue = new ConcurrentQueue<FDQueueItem>();
			current = new ConcurrentStack<FDQueueItem>();
			finished = new ConcurrentStack<FDQueueItem>();
		}
		
		
		#region Universal Functions that are using by upload, download, and misc queues.
		public void Add(FDQueueItem item)
		{
			if(item.status == FDItemStatus.QueuedUpload)
				uploadQueue.Enqueue(item);
			else if(item.status == FDItemStatus.QueuedDownload)
				downloadQueue.Enqueue(item);
			else
				throw new System.NotImplementedException("Queue doesn't yet support this item status");
		}
		
		private void SaveQueue()
		{
			
		}
		
		public void StopQueue()
		{
			stopQueue = true;
			wasStopped = true;
			this.SaveQueue();
		}
		
		public bool PauseQueue() 
		{
			this.pauseQueue = !this.pauseQueue;
			this.paused = this.pauseQueue;
			
			return this.paused;
		}
		
		public int QueueSize()
		{
			return this.uploadQueue.Count + this.downloadQueue.Count + this.current.Count;
		}
		#endregion
		
		#region Upload Functions that are used for sending data to Glacier
		public void ProcessUploadQueueWorker()
		{	
			while(!stopQueue)
			{
				if(uploadQueue.Count == 0 || pauseQueue) //if there's nothing to do at the moment, or if we're paused
				{
					Thread.Sleep (1000);
					continue;
				}
				
				//There's something to upload. Get it.
				FDQueueItem currentUpload;
				if(!uploadQueue.TryDequeue(out currentUpload)) continue;
				currentUpload.status = FDItemStatus.Uploading;
				current.Push(currentUpload);
				
				//pass to our (potentially) recursive function for upload
				try {
					this.ProcessFileUpload(currentUpload.path);
					currentUpload.status = FDItemStatus.FinishedUploading;
				}
				catch (Exception e)
				{
					log.Store(this.ToString(),
					          "ProcessFileUpload crashed on file: " + currentUpload.path, 
					          "Exception was: " + e.Message, 
					          FDLogVerbosity.Error);
					currentUpload.status = FDItemStatus.Error;
				}
				
				//Add the item to the finished queue
				finished.Push(currentUpload);
			}
			//TODO Save Queue if processor quits
		}
		
		public void ProcessFileUpload(string fileName)
		{
			FileAttributes attr = File.GetAttributes(fileName);
			//TODO handle non-file items (like devices)
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory) { //we're on a directory, process it recursively
				foreach ( string f in Directory.GetFiles(fileName) ) //handle files first
					this.ProcessFileUpload (f);
				foreach ( string d in Directory.GetDirectories(fileName) ) //then directories
					this.ProcessFileUpload (d);
				return; //if we're done processing all contents, return
			}
			
			//Build our temporary item, and update all "current*" EXCEPT guid, since we use to communicate with frontend
			FDQueueItem item = new FDQueueItem(fileName, FileAttributes.Normal, FDItemStatus.Uploading);
			try
			{
				item.info = new System.IO.FileInfo(item.path);
			}
			catch (Exception e)
			{
				log.Store (this.ToString(), 
				           "Failed to get file info on " + item.path,
				           "Exception info was: " + e.Message,
				           FDLogVerbosity.Error);
			}
			
			if (item.info.Length == 0) { //our file is zero bytes, Glacier won't accept it but we still want to remember
				DataStore.InsertFile(item.path,
				                     "0",
				                     item.info.Length,
				                     item.info.LastWriteTimeUtc,
				                     "0");
				return;
			}
			
			//Calculate the file's checksum
			FDChecksum fileChecksum = new FDChecksum(item.path);
			try {
				fileChecksum.CalculateChecksum();
			}
			catch (Exception e) {
				log.Store (this.ToString(),
				           "Failed to checksum on " + item.path,
				           "Exception info was: " + e.Message,
				           FDLogVerbosity.Error);
				return;
			}
			item.checksum = fileChecksum.checksum;
			
			//Verfiy we haven't uploaded before by comparing checksum + filesize to db
			string archiveId = DataStore.CheckExists(item.checksum, item.info.Length);
			if (!String.IsNullOrEmpty(archiveId)) //if we got a response, re-insert the file to db and advance queue
			{
				DataStore.InsertFile(item.path,
				                     item.checksum,
				                     item.info.Length,
				                     item.info.LastWriteTimeUtc,
				                     archiveId);
				return;
			}
			
			//Upload the file
			item.glacier = new FDGlacier(settings, log, "upload");
			item.glacier.archiveDescription = System.IO.Path.GetFileName (item.path);
			item.glacier.setCallback(item._updateProgress);
			try {
				item.glacier.uploadFile(item.path);
			}
			catch (Exception e) {
				log.Store(this.ToString(),
				          "Failed to upload on " + item.path,
				          "Exception info was: " + e.Message,
				          FDLogVerbosity.Error);
				return;
			}
			
			//Store the result
			DataStore.InsertFile(item.path, 
			                     item.checksum, 
			                     item.info.Length, 
			                     item.info.LastWriteTimeUtc, 
			                     item.glacier.result.ArchiveId);
		}
		#endregion
	}
}

