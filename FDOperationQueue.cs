using System;
using System.Collections.Generic;
using System.Threading;

namespace snowpack
{
	public class FDOperationQueue
	{
		private List<FDQueueItem> queue;
		private bool stopQueue;
		public string currentFile { get; set; }
		public string currentStatus { get; set; }
		public System.IO.FileInfo currentInfo { get; set; }
		public FDQueueItem currentItem { get; set; }
		public Amazon.Glacier.Transfer.UploadResult currentResult { get; set; }
		private FDDataStore DataStore;
			
		public FDOperationQueue (FDDataStore store)
		{
			queue = new List<FDQueueItem>();
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
		
		public bool Remove(int id)
		{
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
				continue; //REMOVE once testing finished
				if(queue.Count == 0) //if there's nothing to do at the moment
				{
					//System.Console.WriteLine("No upload queue items to process");
					Thread.Sleep (1000);
					continue;
				}
				
				//There's something to upload. Get it.
				currentItem = queue[0];
				queue.RemoveAt (0);
				currentFile = currentItem.path;
				currentInfo = new System.IO.FileInfo(currentItem.path);
				System.Console.WriteLine("Uploading File " + currentItem.path);
				
				//Calculate the file's checksum
				currentStatus = "checksum";
				FDChecksum fileChecksum = new FDChecksum(currentItem.path);
				fileChecksum.CalculateChecksum();
				currentItem.checksum = fileChecksum.checksum;
				System.Console.WriteLine ("Checksum Complete " + currentItem.checksum);
				
				//Verfiy we haven't uploaded before
				string archiveId = DataStore.CheckExists(currentItem.checksum, currentInfo.Length);
				if (!String.IsNullOrEmpty(archiveId))
				{
					DataStore.InsertFile(currentItem.path,
					                     currentItem.checksum,
					                     currentInfo.Length,
					                     currentInfo.LastWriteTimeUtc,
					                     archiveId);
					continue;
				}
				
				//Upload the file
				currentStatus = "upload";
				FDGlacier glacier = new FDGlacier();
				glacier.archiveDescription = System.IO.Path.GetFileName (currentItem.path);
				glacier.setCallback(currentItem._updateProgress);
				glacier.uploadFile(currentItem.path);
				currentResult = glacier.getResult();
				System.Console.WriteLine("Upload Complate " + currentResult.ArchiveId);
				
				//Store the result
				currentStatus = "store";
				DataStore.InsertFile(currentItem.path, 
				                     currentItem.checksum, 
				                     currentInfo.Length, 
				                     currentInfo.LastWriteTimeUtc, 
				                     currentResult.ArchiveId);
				System.Console.WriteLine("Data Stored.");
				
				currentStatus = "idle";
				
				numCompleted++;
			}
		}
		
		public void StopQueue()
		{
			stopQueue = true;
		}
	}
}

