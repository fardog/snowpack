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
		private Configuration localConfig;
		private KeyValueConfigurationCollection settings;
		
		//all the settings we might store
		public string DataStoreFile { get; set; }
		public string DataStorePath { get; set; }
		public string CurrentDataStore { get; set; }
		
		public FDUserSettings ()
		{
			localConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
			settings = localConfig.AppSettings.Settings;
			
			//localConfig.AppSettings["DataStoreFile"] = "index.sqlite";
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
			
			CurrentDataStore = DataStorePath + System.IO.Path.DirectorySeparatorChar + DataStoreFile;
			
			Console.WriteLine(CurrentDataStore);	
			localConfig.Save (ConfigurationSaveMode.Modified);
		}
	}
}

