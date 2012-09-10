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
using Amazon.Runtime;
using Amazon.Glacier;
using Amazon.Glacier.Model;
using Amazon.Glacier.Transfer;
using snowpack;

namespace snowpack
{
	public class FDGlacier
	{
		private AmazonGlacierConfig glacierConfig;
		private AWSCredentials awsCredentials;
		private ArchiveTransferManager transferManager;
		private RegionEndpoint region;
		public UploadResult result { get; set; }
		private UploadOptions upOptions;
		private DownloadOptions downOptions;
		private string OperationType;
		public string archiveID { get; set; }
		public string vaultName { get; set; }
		public string archiveDescription { get; set; }
		public int progress { get; set; }
		private FDOperationLog log;

		public FDGlacier (FDUserSettings settings, FDOperationLog oplog, string optype)
		{
			this.vaultName = settings.AWSGlacierVaultName;
			log = oplog;

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
			
			//Instantiate the glacier config with our settins (for future move to AmazonGlacierClient)
			glacierConfig = new AmazonGlacierConfig();
			glacierConfig.RegionEndpoint = region;
			
			//Instantiate AWS Credentials
			awsCredentials = new BasicAWSCredentials(settings.AWSAccessKey, settings.AWSSecretKey);

			//Instantiate the transfer manager with our settings
			//TODO: Switch to glacier client so we can abort this damn thing
			//glacierClient = new AmazonGlacierClient(appConfig["AWSAccessKey"], appConfig["AWSSecretKey"], region);
			transferManager = new ArchiveTransferManager(awsCredentials, region);
			
			upOptions = new UploadOptions();
			downOptions = new DownloadOptions();
			progress = 0;
			upOptions.StreamTransferProgress = downOptions.StreamTransferProgress = this.onProgress;
			
			OperationType = optype;
		}
		
		public void setCallback(System.EventHandler<Amazon.Runtime.StreamTransferProgressArgs> handler)
		{
			upOptions.StreamTransferProgress = handler;
			downOptions.StreamTransferProgress = handler;
		}
		
		public string getOperationType()
		{
			return OperationType;
		}
		
		public ListJobsResult ListJobs()
		{
			AmazonGlacierClient client = new AmazonGlacierClient(awsCredentials, glacierConfig);
			ListJobsRequest request = new ListJobsRequest().WithVaultName(vaultName);
			ListJobsResponse response = client.ListJobs(request);
			System.Console.WriteLine(response.ListJobsResult.JobList.Count);
			
			return response.ListJobsResult;
		}
		
		public void uploadFile(string filePath)
		{
			try
			{
				result = transferManager.Upload (this.vaultName, this.archiveDescription, filePath, this.upOptions);
			}
			catch(Exception e)
			{
				log.Store(this.ToString(), "Error uploading file: " + filePath, e.Message, FDLogVerbosity.Error);
				throw e;
			}
		}
		
		public void cancelOperation()
		{
			//nothing yet
		}
		
		public void RequestArchive(string savePath)
		{
			try {
				transferManager.Download(vaultName, archiveID, savePath, downOptions);
			}
			catch (Exception e) {
				log.Store(this.ToString(), "Error downloading file: " + archiveID, e.Message, FDLogVerbosity.Error);
				throw e;
			}
		}
		
		public void DeleteArchive(string archiveID)
		{
			transferManager.DeleteArchive(vaultName, archiveID);
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

