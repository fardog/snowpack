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

