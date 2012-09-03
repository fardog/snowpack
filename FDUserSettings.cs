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
using System.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace snowpack
{
	public class FDUserSettings
	{
		public struct AWSRegionIndex {
			public const int USEast1 = 0;
			public const int USWest1 = 1;
			public const int USWest2 = 2;
			public const int EUWest1 = 3;
			public const int APNortheast1 = 4;
		}
		
		private Configuration localConfig;
		private KeyValueConfigurationCollection settings;
		
		//all the settings we might store
		public string DataStoreFile { get; set; }
		public string DataStorePath { get; set; }
		public string CurrentDataStore { get; set; }
		public string AWSAccessKey { get; set; }
		public string AWSSecretKey { get; set; }
		public int AWSRegion { get; set; }
		public string AWSGlacierVaultName { get; set; }
		
		public FDUserSettings ()
		{
			localConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
			settings = localConfig.AppSettings.Settings;
			
			if(settings["DataStoreFile"] != null)
				DataStoreFile = settings["DataStoreFile"].Value;
			else {
				settings.Add ("DataStoreFile", "index.sqlite");
				DataStoreFile = settings["DataStoreFile"].Value;
			}
			
			if (localConfig.AppSettings.Settings["DataStorePath"] != null) 
				DataStorePath = settings["DataStorePath"].Value;
			else {
				settings.Add ("DataStorePath", System.IO.Path.GetDirectoryName(localConfig.FilePath));
				DataStorePath = settings["DataStorePath"].Value;
			}
			
			if(settings["AWSAccessKey"] != null)
				AWSAccessKey = settings["AWSAccessKey"].Value;
			else {
				settings.Add ("AWSAccessKey", "");
				AWSAccessKey = settings["AWSAccessKey"].Value;
			}
			
			if(settings["AWSSecretKey"] != null)
				AWSSecretKey = settings["AWSSecretKey"].Value;
			else {
				settings.Add ("AWSSecretKey", "");
				AWSSecretKey = settings["AWSSecretKey"].Value;
			}
			
			string AWSRegionTemp;
			if(settings["AWSRegion"] != null)
					AWSRegionTemp = settings["AWSRegion"].Value;
			else {
				settings.Add ("AWSRegion", "");
				AWSRegionTemp = settings["AWSRegion"].Value;
			}
			
			if(!String.IsNullOrWhiteSpace(AWSRegionTemp)) AWSRegion = int.Parse(AWSRegionTemp);
			else AWSRegion = 0;
			
			if(settings["AWSGlacierVaultName"] != null)
				AWSGlacierVaultName = settings["AWSGlacierVaultName"].Value;
			else {
				settings.Add ("AWSGlacierVaultName", "");
				AWSGlacierVaultName = settings["AWSGlacierVaultName"].Value;
			}
			
			CurrentDataStore = DataStorePath + System.IO.Path.DirectorySeparatorChar + DataStoreFile;
			
			localConfig.Save (ConfigurationSaveMode.Modified);
		}
		
		public void SaveSettings()
		{
			settings.Add("AWSAccessKey", AWSAccessKey);
			settings.Add("AWSSecretKey", AWSSecretKey);
			settings.Add("AWSRegion", AWSRegion.ToString());
			settings.Add("AWSGlacierVaultName", AWSGlacierVaultName);
			
			localConfig.Save (ConfigurationSaveMode.Modified);
		}
	}
}

