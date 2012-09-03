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
using Gtk;

namespace snowpack
{
	public partial class FDPreferences : Gtk.Dialog
	{
		FDUserSettings settings;
		
		public FDPreferences (Gtk.Window win, Gtk.DialogFlags f, FDUserSettings userset, bool firstRun = false) 
			: base ("Preferences", win, f)
		{
			this.Build ();
			settings = userset;
			
			//fill the settings fields
			entryAWSAccessKey.Text = settings.AWSAccessKey;
			entryAWSSecretKey.Text = settings.AWSSecretKey;
			entryAWSGlacierVaultName.Text = settings.AWSGlacierVaultName;
			comboboxAWSRegion.Active = settings.AWSRegion;
			
			//you can't cancel if this is firstrun
			if(firstRun) buttonCancel.Sensitive = false;
		}
		
		//we need to verify that something has been entered at least
		protected void VerifyOkResponse (object sender, System.EventArgs e)
		{
			Gdk.Color errorColor = new Gdk.Color(255,200,200);
			bool allowSave = true;
			
			if(String.IsNullOrWhiteSpace(entryAWSAccessKey.Text)) {
				entryAWSAccessKey.ModifyBase(StateType.Normal, errorColor);
				allowSave = false;
			}
			
			if(String.IsNullOrWhiteSpace(entryAWSSecretKey.Text)) {
				entryAWSSecretKey.ModifyBase(StateType.Normal, errorColor);
				allowSave = false;
			}
			
			if(String.IsNullOrWhiteSpace(entryAWSGlacierVaultName.Text)) {
				entryAWSGlacierVaultName.ModifyBase(StateType.Normal, errorColor);
				allowSave = false;
			}
			
			if(String.IsNullOrWhiteSpace(comboboxAWSRegion.ActiveText)) {
				comboboxAWSRegion.ModifyBase(StateType.Normal, errorColor);
				allowSave = false;
			}
			
			if (allowSave) {
				settings.AWSAccessKey = entryAWSAccessKey.Text;
				settings.AWSSecretKey = entryAWSSecretKey.Text;
				settings.AWSGlacierVaultName = entryAWSGlacierVaultName.Text;
				settings.AWSRegion = comboboxAWSRegion.Active;
				settings.SaveSettings();
				this.Respond(ResponseType.Ok);
			}
			else {
				labelError.Text = "You must fill all required fields.";
			}
		}

		protected void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			VerifyOkResponse(sender, e);
		}

		protected void OnButtonOkActivated (object sender, System.EventArgs e)
		{
			VerifyOkResponse(sender, e);
		}
	}
}

