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
using System.Threading;
using System.IO;

namespace snowpack
{
	public class FDOperationQueue
	{
		private List<FDQueueItem> queue;
		private bool stopQueue;
		public bool wasStopped = false;
		public string currentFile { get; set; }
		public string currentStatus { get; set; }
		public Guid currentGuid { get; set; }
		public System.IO.FileInfo currentInfo { get; set; }
		public FDQueueItem currentItem { get; set; }
		public Amazon.Glacier.Transfer.UploadResult currentResult { get; set; }
		private FDDataStore DataStore;
		public Queue<FDQueueItem> finished;
		
		public FDOperationQueue (FDDataStore store)
		{
			queue = new List<FDQueueItem>();
			finished = new Queue<FDQueueItem>();
			stopQueue = false;
			DataStore = store;
			currentStatus = "idle";
		}
		
		public int Add(FDQueueItem item)
		{
			queue.Add(item);
			
			try
			{
				int index = queue.IndexOf(item);
				return index;
			}
			catch (Exception e)
			{
				System.Console.WriteLine(e.Message);
				return -1;
			}
		}
		
		public bool Remove(Guid id)
		{
			int result;
			
			try {
				result = queue.FindIndex(
				delegate (FDQueueItem qi)
				{
					return qi.guid == id;
				});
			}
			catch (ArgumentNullException e) {
				System.Console.WriteLine ("FDOperationQueue Failed to remove: " + e.Message);
				return false;
			}
			
			if(result < 0) return false;
			
			try {
				queue.RemoveAt(result);
			}
			catch (InvalidOperationException e) {
				//was already processed
			}
			catch (Exception e) {
				System.Console.WriteLine ("FDOperationQueue Failed to remove: " + e.Message);
			}
			
			System.Console.WriteLine("removed from operation queue " + result.ToString());
			return true;
		}
		
		public FDQueueItem Get(int id)
		{
			return queue[id];
		}
		
		public void ProcessQueueWorker()
		{
			int numCompleted = 0;
			
			while(!stopQueue)
			{
				if(queue.Count == 0) //if there's nothing to do at the moment
				{
					currentStatus = "idle";
					Thread.Sleep (1000);
					continue;
				}
				
				//There's something to upload. Get it.
				currentItem = queue[0];
				queue.RemoveAt (0);
				currentFile = currentItem.path;
				currentInfo = new System.IO.FileInfo(currentItem.path);
				currentGuid = currentItem.guid;
				FDQueueItem finishItem = currentItem; //create a copy for the finish queue
				
				//pass to our (potentially) recursive function for upload
				this.ProcessFile(currentFile);
				
				//Add the parent item to the "finished" queue
				finished.Enqueue(finishItem);
				
				currentStatus = "idle";
				
				numCompleted++;
			}
			//TODO Save Queue if processor quits
		}
		
		public void ProcessFile(string fileName)
		{
			FileAttributes attr = File.GetAttributes(fileName);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory) { //we're on a directory, process it recursively
				foreach ( string f in Directory.GetFiles(fileName) ) //handle files first
					this.ProcessFile (f);
				foreach ( string d in Directory.GetDirectories(fileName) ) //then directories
					this.ProcessFile (d);
				return; //if we're done processing all contents, return
			}
			
			//Build our temporary item, and update all "current*" EXCEPT guid, since we use to communicate with frontend
			FDQueueItem item = new FDQueueItem(fileName, FileAttributes.Normal);
			currentItem = item;
			currentFile = item.path;
			currentInfo = new System.IO.FileInfo(item.path);
			
			if (currentInfo.Length == 0) { //our file is zero bytes, Glacier won't accept it but we still want to remember
				DataStore.InsertFile(currentItem.path,
				                     "0",
				                     currentInfo.Length,
				                     currentInfo.LastWriteTimeUtc,
				                     "0");
				return;
			}
			
			//Calculate the file's checksum
			currentStatus = "checksum";
			FDChecksum fileChecksum = new FDChecksum(currentItem.path);
			try {
				fileChecksum.CalculateChecksum();
			}
			catch (Exception e) {
				System.Console.WriteLine("Couldn't checksum " + fileName);
				return;
			}
			currentItem.checksum = fileChecksum.checksum;
			
			//Verfiy we haven't uploaded before by comparing checksum + filesize to db
			string archiveId = DataStore.CheckExists(currentItem.checksum, currentInfo.Length);
			if (!String.IsNullOrEmpty(archiveId)) //if we got a response, re-insert the file to db and advance queue
			{
				DataStore.InsertFile(currentItem.path,
				                     currentItem.checksum,
				                     currentInfo.Length,
				                     currentInfo.LastWriteTimeUtc,
				                     archiveId);
				return;
			}
			
			//Upload the file
			currentStatus = "upload";
			FDGlacier glacier = new FDGlacier();
			glacier.archiveDescription = System.IO.Path.GetFileName (currentItem.path);
			glacier.setCallback(currentItem._updateProgress);
			try {
				glacier.uploadFile(currentItem.path);
				currentResult = glacier.getResult();
			}
			catch (Exception e) {
				//TODO log failure!
				System.Console.WriteLine("Couldn't upload " + fileName);
				currentResult = null;
				return;
			}
			
			//Store the result
			currentStatus = "store";
			DataStore.InsertFile(currentItem.path, 
			                     currentItem.checksum, 
			                     currentInfo.Length, 
			                     currentInfo.LastWriteTimeUtc, 
			                     currentResult.ArchiveId);
		}
		
		private void SaveQueue()
		{
			foreach (FDQueueItem item in queue)
			{
				//
			}
		}
		
		public void StopQueue()
		{
			stopQueue = true;
			wasStopped = true;
		}
	}
}

