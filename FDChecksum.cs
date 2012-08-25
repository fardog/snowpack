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
using System.Security.Cryptography;
using System.Text;

namespace snowpack
{
	public class FDChecksum
	{
		public string checksum { get; set; }
		public bool running { get; set; }
		private string FilePath;
		private MD5 md5sum;
		
		public FDChecksum (string path)
		{
			running = false;
			checksum = null;
			
			if(File.Exists(path)) FilePath = path;
			else FilePath = null;
		}
		
		public void CalculateChecksum()
		{
			md5sum = MD5.Create();
			
			FileStream file = new FileStream(this.FilePath, FileMode.Open);
			
			running = true;
			byte[] data = md5sum.ComputeHash((Stream)file);
			
			StringBuilder sb = new StringBuilder();
			
			for (int i = 0; i < data.Length; i++)
			{
				sb.Append(data[i].ToString("x2"));
			}
			
			checksum = sb.ToString();
			running = false;
		}
	}
}

