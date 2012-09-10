using System;

namespace snowpack
{
	public class FDOperationLog
	{
		FDDataStore DataStore;
		int verbosity;
		bool console;
		
		public FDOperationLog (FDDataStore ds, int logVerbosity, bool toConsole)
		{
			DataStore = ds;
			verbosity = logVerbosity;
			console = toConsole;
		}
		
		public void Store (string component, string shortMessage, string fullMessage, int importance)
		{
			if(importance >= verbosity) return;
			
			DataStore.StoreLogMessage(importance, component, shortMessage, fullMessage, DateTime.Now);
			
			if(console) //we write the message to the console also
			{
				Console.WriteLine ("--- " + DateTime.Now.ToString() + ": " + component + "(" + importance + ") ---");
				Console.WriteLine (shortMessage);
			}
		}
	}
}

