using System;

namespace snowpack
{
	public class FDOperationLog
	{
		FDDataStore DataStore;
		FDLogVerbosity verbosity;
		bool console;
		
		public FDOperationLog (FDDataStore ds, FDLogVerbosity logVerbosity, bool toConsole)
		{
			DataStore = ds;
			verbosity = logVerbosity;
			console = toConsole;
		}
		
		public void Store (string component, string shortMessage, string fullMessage, FDLogVerbosity importance)
		{
			if((int)importance >= (int)verbosity) return;
			
			DataStore.StoreLogMessage(importance, component, shortMessage, fullMessage, DateTime.Now);
			
			if(console) //we write the message to the console also
			{
				Console.WriteLine ("--- " + DateTime.Now.ToString() + ": " + component + "(" + importance + ") ---");
				Console.WriteLine (shortMessage);
			}
		}
	}
}

