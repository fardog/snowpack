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
		public string checksum { get; set; }
		public Guid guid { get; set; }
		public int status { get; set; }
		public FDGlacier glacier { get; set; }
		
		public FDQueueItem (string filePath, FileAttributes fileType)
		{
			path = filePath;
			progress = 0;
			whenQueued = DateTime.UtcNow;
			kind = fileType;
			checksum = null;
			guid = Guid.NewGuid();
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
	
	public struct FDItemStatus
	{
		public const int Queued = 0;
		public const int Uploading = 1;
		public const int FinishedUploading = 2;
		public const int Downloading = 3;
		public const int FinishedDownloading = 4;
		public const int Error = -1;
	}
	
	public struct FDLogVerbosity
	{
		public const int Silent = 0;
		public const int Error = 1;
		public const int Warning = 2;
		public const int Information = 3;
		public const int Debug = 4;
	}
}

