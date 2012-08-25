
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.UIManager UIManager;
	private global::Gtk.Action FileAction;
	private global::Gtk.Action OpenAction;
	private global::Gtk.VBox vbox1;
	private global::Gtk.FileChooserWidget fileChooser;
	private global::Gtk.Table table3;
	private global::Gtk.Alignment alignment4;
	private global::Gtk.Alignment alignment5;
	private global::Gtk.Button buttonArchive;
	private global::Gtk.Button buttonCancel;
	private global::Gtk.Button buttonUpload;
	private global::Gtk.Statusbar statusBar;
	private global::Gtk.ProgressBar progressBar;
	
	protected virtual void Build ()
	{
		global::Stetic.Gui.Initialize (this);
		// Widget MainWindow
		this.UIManager = new global::Gtk.UIManager ();
		global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
		this.FileAction = new global::Gtk.Action ("FileAction", global::Mono.Unix.Catalog.GetString ("_File"), null, null);
		this.FileAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("_File");
		w1.Add (this.FileAction, null);
		this.OpenAction = new global::Gtk.Action ("OpenAction", global::Mono.Unix.Catalog.GetString ("_Open"), null, null);
		this.OpenAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("_Open");
		w1.Add (this.OpenAction, "<Control>o");
		this.UIManager.InsertActionGroup (w1, 0);
		this.AddAccelGroup (this.UIManager.AccelGroup);
		this.Name = "MainWindow";
		this.Title = global::Mono.Unix.Catalog.GetString ("snowpack");
		this.WindowPosition = ((global::Gtk.WindowPosition)(4));
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.vbox1 = new global::Gtk.VBox ();
		this.vbox1.Name = "vbox1";
		this.vbox1.Spacing = 6;
		// Container child vbox1.Gtk.Box+BoxChild
		this.fileChooser = new global::Gtk.FileChooserWidget (((global::Gtk.FileChooserAction)(0)));
		this.fileChooser.Name = "fileChooser";
		this.vbox1.Add (this.fileChooser);
		global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.fileChooser]));
		w2.Position = 0;
		// Container child vbox1.Gtk.Box+BoxChild
		this.table3 = new global::Gtk.Table (((uint)(1)), ((uint)(4)), false);
		this.table3.Name = "table3";
		this.table3.RowSpacing = ((uint)(6));
		this.table3.ColumnSpacing = ((uint)(6));
		// Container child table3.Gtk.Table+TableChild
		this.alignment4 = new global::Gtk.Alignment (0.5F, 0.5F, 1F, 1F);
		this.alignment4.Name = "alignment4";
		this.table3.Add (this.alignment4);
		global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table3 [this.alignment4]));
		w3.LeftAttach = ((uint)(1));
		w3.RightAttach = ((uint)(2));
		w3.YOptions = ((global::Gtk.AttachOptions)(4));
		// Container child table3.Gtk.Table+TableChild
		this.alignment5 = new global::Gtk.Alignment (0.5F, 0.5F, 1F, 1F);
		this.alignment5.Name = "alignment5";
		// Container child alignment5.Gtk.Container+ContainerChild
		this.buttonArchive = new global::Gtk.Button ();
		this.buttonArchive.CanFocus = true;
		this.buttonArchive.Name = "buttonArchive";
		this.buttonArchive.UseUnderline = true;
		this.buttonArchive.Label = global::Mono.Unix.Catalog.GetString ("Archive Browser");
		this.alignment5.Add (this.buttonArchive);
		this.table3.Add (this.alignment5);
		global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table3 [this.alignment5]));
		w5.XOptions = ((global::Gtk.AttachOptions)(4));
		w5.YOptions = ((global::Gtk.AttachOptions)(4));
		// Container child table3.Gtk.Table+TableChild
		this.buttonCancel = new global::Gtk.Button ();
		this.buttonCancel.Sensitive = false;
		this.buttonCancel.CanFocus = true;
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.UseUnderline = true;
		this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString ("Cancel");
		this.table3.Add (this.buttonCancel);
		global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table3 [this.buttonCancel]));
		w6.LeftAttach = ((uint)(2));
		w6.RightAttach = ((uint)(3));
		w6.XOptions = ((global::Gtk.AttachOptions)(4));
		w6.YOptions = ((global::Gtk.AttachOptions)(4));
		// Container child table3.Gtk.Table+TableChild
		this.buttonUpload = new global::Gtk.Button ();
		this.buttonUpload.Sensitive = false;
		this.buttonUpload.CanFocus = true;
		this.buttonUpload.Name = "buttonUpload";
		this.buttonUpload.UseUnderline = true;
		this.buttonUpload.Label = global::Mono.Unix.Catalog.GetString ("Upload");
		this.table3.Add (this.buttonUpload);
		global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table3 [this.buttonUpload]));
		w7.LeftAttach = ((uint)(3));
		w7.RightAttach = ((uint)(4));
		w7.XOptions = ((global::Gtk.AttachOptions)(4));
		w7.YOptions = ((global::Gtk.AttachOptions)(4));
		this.vbox1.Add (this.table3);
		global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.table3]));
		w8.Position = 1;
		w8.Expand = false;
		w8.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.statusBar = new global::Gtk.Statusbar ();
		this.statusBar.Name = "statusBar";
		this.statusBar.Spacing = 6;
		this.statusBar.BorderWidth = ((uint)(2));
		// Container child statusBar.Gtk.Box+BoxChild
		this.progressBar = new global::Gtk.ProgressBar ();
		this.progressBar.WidthRequest = 100;
		this.progressBar.Name = "progressBar";
		this.statusBar.Add (this.progressBar);
		global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.statusBar [this.progressBar]));
		w9.Position = 1;
		w9.Expand = false;
		this.vbox1.Add (this.statusBar);
		global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.statusBar]));
		w10.PackType = ((global::Gtk.PackType)(1));
		w10.Position = 2;
		w10.Expand = false;
		w10.Fill = false;
		this.Add (this.vbox1);
		if ((this.Child != null)) {
			this.Child.ShowAll ();
		}
		this.DefaultWidth = 737;
		this.DefaultHeight = 533;
		this.Show ();
		this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
		this.fileChooser.SelectionChanged += new global::System.EventHandler (this.onSelectionChanged);
		this.fileChooser.FileActivated += new global::System.EventHandler (this.onFileActivated);
		this.buttonUpload.Clicked += new global::System.EventHandler (this.onUploadClicked);
		this.buttonCancel.Clicked += new global::System.EventHandler (this.onCancelClicked);
		this.buttonArchive.Activated += new global::System.EventHandler (this.onArchiveActivated);
		this.buttonArchive.Clicked += new global::System.EventHandler (this.onArchiveClicked);
	}
}
