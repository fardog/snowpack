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
		public FileInfo info { get; set; }
		public string checksum { get; set; }
		public Guid guid { get; set; }
		public FDItemStatus status { get; set; }
		public FDGlacier glacier { get; set; }
		public string archiveID { get; set; }
		public string downloadPath { get; set; }
		
		public FDQueueItem (string filePath, FileAttributes fileType, FDItemStatus st)
		{
			path = filePath;
			progress = 0;
			whenQueued = DateTime.UtcNow;
			kind = fileType;
			checksum = null;
			guid = Guid.NewGuid();
			status = st;
		}
		
		public void _updateProgress(object sender, Amazon.Runtime.StreamTransferProgressArgs e)
		{
			progress = e.PercentDone;
		}
	}
	
	public class FDArchiveItem
	{
		public Int64 id { get; set; }
		public string filename { get; set; }
		public string checksum { get; set; }
		public string archiveID { get; set; }
		public DateTime modified { get; set; }
		public DateTime stored { get; set; }
		public Int64 parent { get; set; }
		public int versions { get; set; }
		
		public FDArchiveItem(Int64 i, string fn, string cs, string aid, DateTime mt, DateTime st, Int64 p, int v = 0)
		{
			id = i;
			filename = fn;
			checksum = cs;
			archiveID = aid;
			modified = mt;
			stored = st;
			parent = p;
			versions = v;
		}
	}
	
	public class FDArchiveDirectory
	{
		public Int64 id { get; set; }
		public string dirname { get; set; }
		public Int64 parent { get; set; }
		
		public FDArchiveDirectory (Int64 i, string dn, Int64 p)
		{
			id = i;
			dirname = dn;
			parent = p;
		}
	}
	
	public enum FDItemStatus
	{
		QueuedUpload,
		QueuedDownload,
		QueuedMisc,
		Uploading,
		Downloading,
		FinishedUploading,
		FinishedDownloading,
		FinishedMisc,
		Error
	}
	
	public enum FDLogVerbosity
	{
		Silent,
		Error,
		Warning,
		Information,
		Debug
	}
}

