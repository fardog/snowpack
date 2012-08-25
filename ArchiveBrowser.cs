using System;
using Gtk;

namespace snowpack
{
	public partial class ArchiveBrowser : Gtk.Window
	{
		FDDataStore DataStore;
		
		public ArchiveBrowser (string dbase) : base(Gtk.WindowType.Toplevel)
		{
			DataStore = new FDDataStore(dbase);
			this.Build ();
		}
	}
}

