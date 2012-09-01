using System;
using System.IO;

namespace snowpack
{
	public class FDQueueItem
	{
		public string path { get; set; }
		public int progress { get; set; }
		public DateTime whenQueued { get; set; }
		public DateTime whenCompleted { get; set; }
		public FileAttributes kind { get; set; }
		public string checksum { get; set; }
		
		public FDQueueItem (string filePath, FileAttributes fileType)
		{
			path = filePath;
			progress = 0;
			whenQueued = DateTime.Now;
			kind = fileType;
			checksum = null;
		}
		
		public void _updateProgress(object sender, Amazon.Runtime.StreamTransferProgressArgs e)
		{
			progress = e.PercentDone;
		}
	}
}

