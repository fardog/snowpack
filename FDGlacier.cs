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
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;

using Amazon;
using Amazon.Glacier;
using Amazon.Glacier.Model;
using Amazon.Glacier.Transfer;
using snowpack;

namespace snowpack
{
	public class FDGlacier
	{
		private ArchiveTransferManager transferManager;
		private RegionEndpoint region;
		private UploadResult result;
		private UploadOptions options;
		public string vaultName { get; set; }
		public string archiveDescription { get; set; }
		public int progress { get; set; }

		public FDGlacier (FDUserSettings settings)
		{
			this.vaultName = settings.AWSGlacierVaultName;

			switch (settings.AWSRegion) {
			case FDUserSettings.AWSRegionIndex.USWest1:
				region = RegionEndpoint.USWest1;
				break;
			case FDUserSettings.AWSRegionIndex.USWest2:
				region = RegionEndpoint.USWest2;
				break;
			case FDUserSettings.AWSRegionIndex.USEast1:
				region = RegionEndpoint.USEast1;
				break;
			case FDUserSettings.AWSRegionIndex.EUWest1:
				region = RegionEndpoint.EUWest1;
				break;
			case FDUserSettings.AWSRegionIndex.APNortheast1:
				region = RegionEndpoint.APNortheast1;
				break;
			default:
				region = RegionEndpoint.USEast1;
				break;
			}

			//Instantiate the transfer manager with our settings
			//TODO: Switch to glacier client so we can abort this damn thing
			//glacierClient = new AmazonGlacierClient(appConfig["AWSAccessKey"], appConfig["AWSSecretKey"], region);
			transferManager = new ArchiveTransferManager(settings.AWSAccessKey, settings.AWSSecretKey, region);
			
			options = new UploadOptions();
			progress = 0;
			options.StreamTransferProgress = this.onProgress;
		}
		
		public void setCallback(System.EventHandler<Amazon.Runtime.StreamTransferProgressArgs> handler)
		{
			options.StreamTransferProgress = handler;
		}
		
		public void uploadFile(string filePath)
		{
			try
			{
				result = transferManager.Upload (this.vaultName, this.archiveDescription, filePath, this.options);
			}
			catch(Exception e)
			{
				System.Console.WriteLine("Error uploading file: " + e.Message);
				System.Console.WriteLine("…on file: " + filePath);
				throw e;
			}
		}
		
		public void cancelUpload()
		{
			transferManager.Dispose();
		}
		
		public UploadResult getResult()
		{
			return result;
		}
		
		public void onUploadFinish (object obj, EventArgs args)
		{
			Console.WriteLine("finished");
		}
		
		public void onProgress(Object sender, Amazon.Runtime.StreamTransferProgressArgs e)
		{
			this.progress = e.PercentDone;
		}
	}
}

