using System;

namespace snowpack
{
	public partial class FDPreferences : Gtk.Dialog
	{
		public FDPreferences (Gtk.Window win, Gtk.DialogFlags f, FDUserSettings settings) : base ("Preferences", win, f)
		{
			this.Build ();
		}
	}
}

