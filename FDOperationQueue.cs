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
		
		//the queue items
		private ConcurrentQueue<FDQueueItem> uploadQueue;
		private ConcurrentQueue<FDQueueItem> downloadQueue;
		public ConcurrentStack<FDQueueItem> current;
		public ConcurrentStack<FDQueueItem> finished;
		
		//the ignore list, which maintains objects that have been "dequeued"
		//since we can't remove specific items from a ConcurrentQueue, we maintain a list of GUIDs which should be "skipped"
		private HashSet<Guid> ignoreGuid;
		
		//progress items meant for reporting back to the frontend. Not to be trusted implicitly!
		public int UploadProgress;
		public int DownloadProgress;
		public string UploadFile;
		public string DownloadFile;
		public bool UploadRunning;
		public bool DownloadRunning;
		
		//thread pool for download threads
		private int MaxDownloadThreads = 5;
		
		public FDOperationQueue (FDDataStore store, FDUserSettings userset, FDOperationLog oplog)
		{
			stopQueue = false;
			DataStore = store;
			settings = userset;
			log = oplog;
			UploadRunning = DownloadRunning = false;
			
			uploadQueue = new ConcurrentQueue<FDQueueItem>();
			downloadQueue = new ConcurrentQueue<FDQueueItem>();
			current = new ConcurrentStack<FDQueueItem>();
			finished = new ConcurrentStack<FDQueueItem>();
			
			ignoreGuid = new HashSet<Guid>();
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
		
		//adds a guid to the "ignore" list, so we skip it when it comes across in the queue
		public bool Remove(Guid guid)
		{
			ignoreGuid.Add(guid);
			return true;
		}
		
		private void SaveQueue()
		{
			
		}
		
		//stops after the current queue item, and saves the queue
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
			int count = 0;
			if(UploadRunning) count++;
			if(DownloadRunning) count++;
			return this.uploadQueue.Count + this.downloadQueue.Count + this.current.Count + count;
		}
		
		private void _uploadProgress(object sender, Amazon.Runtime.StreamTransferProgressArgs e)
		{
			if (e.PercentDone == UploadProgress) return;
			UploadProgress = e.PercentDone;
		}
		
		private void _downloadProgress(object sender, Amazon.Runtime.StreamTransferProgressArgs e)
		{
			if (e.PercentDone == UploadProgress) return;
			DownloadProgress = e.PercentDone;
		}
		#endregion
		
		#region Upload Functions that are used for sending data to Glacier
		public void ProcessUploadQueueWorker()
		{	
			while(!stopQueue)
			{
				if(uploadQueue.Count == 0 || pauseQueue) //if there's nothing to do at the moment, or if we're paused
				{
					if(UploadRunning) UploadRunning = false;
					Thread.Sleep (1000);
					continue;
				}
				
				if(!UploadRunning) UploadRunning = true;
				
				//There's something to upload. Get it.
				FDQueueItem currentUpload;
				if(!uploadQueue.TryDequeue(out currentUpload)) continue;
				
				//if it's in our ignore list, don't upload
				if(ignoreGuid.Contains(currentUpload.guid)) continue;
				
				//set status, and push to the frontend communication queue
				currentUpload.status = FDItemStatus.Uploading;
				current.Push(currentUpload);
				
				//pass to our (potentially) recursive function for upload
				try 
				{
					this.ProcessFileUpload(currentUpload.path);
					
					//when we reach here, we've finished uploading
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
			
			//Build our temporary item for upload
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
			
			//our file is zero bytes, Glacier won't accept it but we still want to remember that it existed
			if (item.info.Length == 0) 
			{
				DataStore.InsertFile(item.path,
				                     "0",
				                     item.info.Length,
				                     item.info.LastWriteTimeUtc,
				                     "0");
				return;
			}
			
			//set the current file name for communication with the frontend
			UploadFile = Path.GetFileName(item.path);
			
			//Calculate the file's checksum before upload, so we can check if it's uploaded before
			FDChecksum fileChecksum = new FDChecksum(item.path);
			try 
			{
				fileChecksum.CalculateChecksum();
			}
			catch (Exception e) 
			{
				log.Store (this.ToString(),
				           "Failed to checksum on " + item.path,
				           "Exception info was: " + e.Message,
				           FDLogVerbosity.Error);
				return;
			}
			item.checksum = fileChecksum.checksum;
			
			//Verfiy we haven't uploaded previously by comparing checksum + filesize to db
			string archiveId = DataStore.CheckExists(item.checksum, item.info.Length);
			
			//if we got a response, re-insert the file to db to be remembered and advance queue
			if (!String.IsNullOrEmpty(archiveId))
			{
				DataStore.InsertFile(item.path,
				                     item.checksum,
				                     item.info.Length,
				                     item.info.LastWriteTimeUtc,
				                     archiveId);
				return;
			}
			
			//If we've made it this far, we're ready to upload the file
			item.glacier = new FDGlacier(settings, log, "upload");
			item.glacier.archiveDescription = System.IO.Path.GetFileName (item.path);
			item.glacier.setCallback(this._uploadProgress);
			try 
			{
				item.glacier.uploadFile(item.path);
			}
			catch (Exception e) 
			{
				log.Store(this.ToString(),
				          "Failed to upload on " + item.path,
				          "Exception info was: " + e.Message,
				          FDLogVerbosity.Error);
				return;
			}
			
			//Store the result to db
			DataStore.InsertFile(item.path, 
			                     item.checksum, 
			                     item.info.Length, 
			                     item.info.LastWriteTimeUtc, 
			                     item.glacier.result.ArchiveId);
		}
		#endregion
		
		#region Download Functions that process the download queue
		public void ProcessDownloadQueueWorker()
		{	
			while(!stopQueue)
			{
				if(downloadQueue.Count == 0 || pauseQueue) //if there's nothing to do at the moment, or if we're paused
				{
					if(DownloadRunning) DownloadRunning = false;
					Thread.Sleep (1000);
					continue;
				}
				
				if(!DownloadRunning) DownloadRunning = true;
				
				//There's something to download. Get it.
				FDQueueItem currentDownload;
				if(!downloadQueue.TryDequeue(out currentDownload)) continue;
				
				//if it's in our ignore list, don't download
				if(ignoreGuid.Contains(currentDownload.guid)) continue;
				
				//set status, and push to the frontend communication queue
				currentDownload.status = FDItemStatus.Downloading;
				current.Push(currentDownload);
				
				//pass to our (potentially) recursive function for download
				try 
				{
					this.ProcessFileDownload(currentDownload);
					
					//when we reach here, we've finished downloading
					currentDownload.status = FDItemStatus.FinishedDownloading;
				}
				catch (Exception e)
				{
					log.Store(this.ToString(),
					          "ProcessFileDownload crashed on file: " + currentDownload.path, 
					          "Exception was: " + e.Message, 
					          FDLogVerbosity.Error);
					currentDownload.status = FDItemStatus.Error;
				}
				
				//Add the item to the finished queue
				finished.Push(currentDownload);
			}
		}
		
		public void ProcessFileDownload(FDQueueItem item) 
		{
			if(item.kind != System.IO.FileAttributes.Normal)
			{
				throw new Exception("Recursive downloads aren't supported yet.");
				return;
			}
			Console.WriteLine("Archive ID was: " + item.archiveID);
			item.glacier = new FDGlacier(settings, log, "download");
			item.glacier.archiveID = item.archiveID;
			item.glacier.setCallback(this._downloadProgress);
			item.glacier.RequestArchive(item.downloadPath);
		}
		#endregion
	}
}

