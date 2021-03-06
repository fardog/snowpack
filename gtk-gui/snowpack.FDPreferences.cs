
// This file has been generated by the GUI designer. Do not modify.
namespace snowpack
{
	public partial class FDPreferences
	{
		private global::Gtk.Notebook notebook1;
		private global::Gtk.Table table1;
		private global::Gtk.ComboBox comboboxAWSRegion;
		private global::Gtk.Entry entryAWSAccessKey;
		private global::Gtk.Entry entryAWSGlacierVaultName;
		private global::Gtk.Entry entryAWSSecretKey;
		private global::Gtk.Label labelAWSAccessKey;
		private global::Gtk.Label labelAWSRegion;
		private global::Gtk.Label labelAWSSecretKey;
		private global::Gtk.Label labelVault;
		private global::Gtk.Label labelAWS;
		private global::Gtk.Table table2;
		private global::Gtk.Label labelGeneral;
		private global::Gtk.Label labelError;
		private global::Gtk.Button buttonCancel;
		private global::Gtk.Button buttonOk;
		
		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget snowpack.FDPreferences
			this.Name = "snowpack.FDPreferences";
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Internal child snowpack.FDPreferences.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(8));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.notebook1 = new global::Gtk.Notebook ();
			this.notebook1.CanFocus = true;
			this.notebook1.Name = "notebook1";
			this.notebook1.CurrentPage = 1;
			// Container child notebook1.Gtk.Notebook+NotebookChild
			this.table1 = new global::Gtk.Table (((uint)(4)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			this.table1.BorderWidth = ((uint)(8));
			// Container child table1.Gtk.Table+TableChild
			this.comboboxAWSRegion = global::Gtk.ComboBox.NewText ();
			this.comboboxAWSRegion.AppendText (global::Mono.Unix.Catalog.GetString ("US East (N. Virginia)"));
			this.comboboxAWSRegion.AppendText (global::Mono.Unix.Catalog.GetString ("US West (Northern California)"));
			this.comboboxAWSRegion.AppendText (global::Mono.Unix.Catalog.GetString ("US West (Oregon)"));
			this.comboboxAWSRegion.AppendText (global::Mono.Unix.Catalog.GetString ("EU (Ireland)"));
			this.comboboxAWSRegion.AppendText (global::Mono.Unix.Catalog.GetString ("Asia Pacific (Tokyo)"));
			this.comboboxAWSRegion.Name = "comboboxAWSRegion";
			this.comboboxAWSRegion.Active = 0;
			this.table1.Add (this.comboboxAWSRegion);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1 [this.comboboxAWSRegion]));
			w2.TopAttach = ((uint)(2));
			w2.BottomAttach = ((uint)(3));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryAWSAccessKey = new global::Gtk.Entry ();
			this.entryAWSAccessKey.CanFocus = true;
			this.entryAWSAccessKey.Name = "entryAWSAccessKey";
			this.entryAWSAccessKey.IsEditable = true;
			this.entryAWSAccessKey.InvisibleChar = '•';
			this.table1.Add (this.entryAWSAccessKey);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1 [this.entryAWSAccessKey]));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryAWSGlacierVaultName = new global::Gtk.Entry ();
			this.entryAWSGlacierVaultName.CanFocus = true;
			this.entryAWSGlacierVaultName.Name = "entryAWSGlacierVaultName";
			this.entryAWSGlacierVaultName.IsEditable = true;
			this.entryAWSGlacierVaultName.InvisibleChar = '•';
			this.table1.Add (this.entryAWSGlacierVaultName);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1 [this.entryAWSGlacierVaultName]));
			w4.TopAttach = ((uint)(3));
			w4.BottomAttach = ((uint)(4));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryAWSSecretKey = new global::Gtk.Entry ();
			this.entryAWSSecretKey.CanFocus = true;
			this.entryAWSSecretKey.Name = "entryAWSSecretKey";
			this.entryAWSSecretKey.IsEditable = true;
			this.entryAWSSecretKey.Visibility = false;
			this.entryAWSSecretKey.InvisibleChar = '•';
			this.table1.Add (this.entryAWSSecretKey);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1 [this.entryAWSSecretKey]));
			w5.TopAttach = ((uint)(1));
			w5.BottomAttach = ((uint)(2));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelAWSAccessKey = new global::Gtk.Label ();
			this.labelAWSAccessKey.Name = "labelAWSAccessKey";
			this.labelAWSAccessKey.Xalign = 0F;
			this.labelAWSAccessKey.LabelProp = global::Mono.Unix.Catalog.GetString ("Access Key");
			this.table1.Add (this.labelAWSAccessKey);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1 [this.labelAWSAccessKey]));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelAWSRegion = new global::Gtk.Label ();
			this.labelAWSRegion.Name = "labelAWSRegion";
			this.labelAWSRegion.Xalign = 0F;
			this.labelAWSRegion.LabelProp = global::Mono.Unix.Catalog.GetString ("Region");
			this.table1.Add (this.labelAWSRegion);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1 [this.labelAWSRegion]));
			w7.TopAttach = ((uint)(2));
			w7.BottomAttach = ((uint)(3));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelAWSSecretKey = new global::Gtk.Label ();
			this.labelAWSSecretKey.Name = "labelAWSSecretKey";
			this.labelAWSSecretKey.Xalign = 0F;
			this.labelAWSSecretKey.LabelProp = global::Mono.Unix.Catalog.GetString ("Secret Key");
			this.table1.Add (this.labelAWSSecretKey);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1 [this.labelAWSSecretKey]));
			w8.TopAttach = ((uint)(1));
			w8.BottomAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelVault = new global::Gtk.Label ();
			this.labelVault.Name = "labelVault";
			this.labelVault.Xalign = 0F;
			this.labelVault.LabelProp = global::Mono.Unix.Catalog.GetString ("Glacier Vault Name");
			this.table1.Add (this.labelVault);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1 [this.labelVault]));
			w9.TopAttach = ((uint)(3));
			w9.BottomAttach = ((uint)(4));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			this.notebook1.Add (this.table1);
			// Notebook tab
			this.labelAWS = new global::Gtk.Label ();
			this.labelAWS.Name = "labelAWS";
			this.labelAWS.LabelProp = global::Mono.Unix.Catalog.GetString ("AWS Settings");
			this.notebook1.SetTabLabel (this.table1, this.labelAWS);
			this.labelAWS.ShowAll ();
			// Container child notebook1.Gtk.Notebook+NotebookChild
			this.table2 = new global::Gtk.Table (((uint)(3)), ((uint)(3)), false);
			this.table2.Name = "table2";
			this.table2.RowSpacing = ((uint)(6));
			this.table2.ColumnSpacing = ((uint)(6));
			this.notebook1.Add (this.table2);
			global::Gtk.Notebook.NotebookChild w11 = ((global::Gtk.Notebook.NotebookChild)(this.notebook1 [this.table2]));
			w11.Position = 1;
			// Notebook tab
			this.labelGeneral = new global::Gtk.Label ();
			this.labelGeneral.Name = "labelGeneral";
			this.labelGeneral.LabelProp = global::Mono.Unix.Catalog.GetString ("General");
			this.notebook1.SetTabLabel (this.table2, this.labelGeneral);
			this.labelGeneral.ShowAll ();
			w1.Add (this.notebook1);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(w1 [this.notebook1]));
			w12.Position = 0;
			w12.Padding = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.labelError = new global::Gtk.Label ();
			this.labelError.Name = "labelError";
			this.labelError.Xalign = 1F;
			w1.Add (this.labelError);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(w1 [this.labelError]));
			w13.Position = 1;
			w13.Expand = false;
			w13.Fill = false;
			// Internal child snowpack.FDPreferences.ActionArea
			global::Gtk.HButtonBox w14 = this.ActionArea;
			w14.Name = "dialog1_ActionArea";
			w14.Spacing = 10;
			w14.BorderWidth = ((uint)(5));
			w14.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w15 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w14 [this.buttonCancel]));
			w15.Expand = false;
			w15.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-ok";
			w14.Add (this.buttonOk);
			global::Gtk.ButtonBox.ButtonBoxChild w16 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w14 [this.buttonOk]));
			w16.Position = 1;
			w16.Expand = false;
			w16.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 636;
			this.DefaultHeight = 259;
			this.Show ();
			this.buttonOk.Clicked += new global::System.EventHandler (this.OnButtonOkClicked);
			this.buttonOk.Activated += new global::System.EventHandler (this.OnButtonOkActivated);
		}
	}
}
