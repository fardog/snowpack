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
		
		public FDQueueItem (string filePath, FileAttributes fileType)
		{
			path = filePath;
			progress = 0;
			whenQueued = DateTime.Now;
			kind = fileType;
			checksum = null;
			guid = Guid.NewGuid();
		}
		
		public void _updateProgress(object sender, Amazon.Runtime.StreamTransferProgressArgs e)
		{
			progress = e.PercentDone;
		}
	}
}

