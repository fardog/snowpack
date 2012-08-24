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
		private FDFileEncryption encryptionStream;
		private AmazonGlacierClient glacierClient;
		private ArchiveTransferManager transferManager;
		private RegionEndpoint region;
		private UploadResult result;
		private UploadOptions options;
		public string vaultName { get; set; }
		public string archiveDescription { get; set; }
		public int progress { get; set; }

		public FDGlacier ()
		{
			encryptionStream = new FDFileEncryption ();

			//Load settings from App.config
			NameValueCollection appConfig = ConfigurationManager.AppSettings;
			this.vaultName = appConfig["GlacierVaultName"];

			switch (appConfig ["AWSRegion"]) {
			case "us-west-1":
				region = RegionEndpoint.USWest1;
				break;
			case "us-west-2":
				region = RegionEndpoint.USWest2;
				break;
			case "us-east-1":
				region = RegionEndpoint.USEast1;
				break;
			case "eu-west-1":
				region = RegionEndpoint.EUWest1;
				break;
			default:
				region = RegionEndpoint.USEast1;
				break;
			}

			//Instantiate the transfer manager with our settings
			//TODO: Switch to glacier client so we can abort this damn thing
			//glacierClient = new AmazonGlacierClient(appConfig["AWSAccessKey"], appConfig["AWSSecretKey"], region);
			transferManager = new ArchiveTransferManager(appConfig["AWSAccessKey"], appConfig["AWSSecretKey"], region);
			
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
			result = transferManager.Upload (this.vaultName, this.archiveDescription, filePath, this.options);
			System.Console.WriteLine (result.ArchiveId);
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

