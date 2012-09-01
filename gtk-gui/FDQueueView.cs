
// This file has been generated by the GUI designer. Do not modify.

public partial class FDQueueView
{
	private global::Gtk.UIManager UIManager;
	private global::Gtk.Action QueueAction;
	private global::Gtk.Action AddItemAction;
	private global::Gtk.Action RemoveSelectedAction;
	private global::Gtk.Action MoveSelectedAction;
	private global::Gtk.Action EditAction;
	private global::Gtk.Action WindowAction;
	private global::Gtk.Action HelpAction;
	private global::Gtk.Action QuitAction;
	private global::Gtk.Action AboutAction;
	private global::Gtk.Action SelectAllAction;
	private global::Gtk.VBox vbox1;
	private global::Gtk.MenuBar menubar1;
	private global::Gtk.ScrolledWindow GtkScrolledWindow;
	private global::Gtk.TreeView treeview1;
	private global::Gtk.HBox hbox1;
	private global::Gtk.Button buttonAdd;
	private global::Gtk.Button buttonRemove;
	private global::Gtk.Fixed fixed2;
	private global::Gtk.Button buttonArchive;
	private global::Gtk.Fixed fixed1;
	private global::Gtk.Statusbar statusbar1;
	private global::Gtk.ProgressBar progressbar1;
	
	protected virtual void Build ()
	{
		global::Stetic.Gui.Initialize (this);
		// Widget FDQueueView
		this.UIManager = new global::Gtk.UIManager ();
		global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
		this.QueueAction = new global::Gtk.Action ("QueueAction", global::Mono.Unix.Catalog.GetString ("_Queue"), null, null);
		this.QueueAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Queue");
		w1.Add (this.QueueAction, null);
		this.AddItemAction = new global::Gtk.Action ("AddItemAction", global::Mono.Unix.Catalog.GetString ("_Add item…"), null, null);
		this.AddItemAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Add item…");
		w1.Add (this.AddItemAction, "<Primary>o");
		this.RemoveSelectedAction = new global::Gtk.Action ("RemoveSelectedAction", global::Mono.Unix.Catalog.GetString ("Remove Selected"), null, null);
		this.RemoveSelectedAction.Sensitive = false;
		this.RemoveSelectedAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Remove Selected");
		w1.Add (this.RemoveSelectedAction, null);
		this.MoveSelectedAction = new global::Gtk.Action ("MoveSelectedAction", global::Mono.Unix.Catalog.GetString ("Move Selected"), null, null);
		this.MoveSelectedAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Move Selected");
		w1.Add (this.MoveSelectedAction, null);
		this.EditAction = new global::Gtk.Action ("EditAction", global::Mono.Unix.Catalog.GetString ("Edit"), null, null);
		this.EditAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Edit");
		w1.Add (this.EditAction, null);
		this.WindowAction = new global::Gtk.Action ("WindowAction", global::Mono.Unix.Catalog.GetString ("Window"), null, null);
		this.WindowAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Window");
		w1.Add (this.WindowAction, null);
		this.HelpAction = new global::Gtk.Action ("HelpAction", global::Mono.Unix.Catalog.GetString ("_Help"), null, null);
		this.HelpAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Help");
		w1.Add (this.HelpAction, null);
		this.QuitAction = new global::Gtk.Action ("QuitAction", global::Mono.Unix.Catalog.GetString ("_Quit…"), null, null);
		this.QuitAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Quit…");
		w1.Add (this.QuitAction, "<Primary>q");
		this.AboutAction = new global::Gtk.Action ("AboutAction", global::Mono.Unix.Catalog.GetString ("_About…"), null, null);
		this.AboutAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("About");
		w1.Add (this.AboutAction, null);
		this.SelectAllAction = new global::Gtk.Action ("SelectAllAction", global::Mono.Unix.Catalog.GetString ("Select All"), null, null);
		this.SelectAllAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Select All");
		w1.Add (this.SelectAllAction, null);
		this.UIManager.InsertActionGroup (w1, 0);
		this.AddAccelGroup (this.UIManager.AccelGroup);
		this.Name = "FDQueueView";
		this.Title = global::Mono.Unix.Catalog.GetString ("Queue - snowpack");
		this.WindowPosition = ((global::Gtk.WindowPosition)(4));
		// Container child FDQueueView.Gtk.Container+ContainerChild
		this.vbox1 = new global::Gtk.VBox ();
		this.vbox1.Name = "vbox1";
		this.vbox1.Spacing = 6;
		// Container child vbox1.Gtk.Box+BoxChild
		this.UIManager.AddUiFromString ("<ui><menubar name='menubar1'><menu name='QueueAction' action='QueueAction'><menuitem name='AddItemAction' action='AddItemAction'/><separator/><menuitem name='MoveSelectedAction' action='MoveSelectedAction'/><menuitem name='RemoveSelectedAction' action='RemoveSelectedAction'/><separator/><menuitem name='QuitAction' action='QuitAction'/></menu><menu name='EditAction' action='EditAction'><menuitem name='SelectAllAction' action='SelectAllAction'/></menu><menu name='WindowAction' action='WindowAction'/><menu name='HelpAction' action='HelpAction'><menuitem name='AboutAction' action='AboutAction'/></menu></menubar></ui>");
		this.menubar1 = ((global::Gtk.MenuBar)(this.UIManager.GetWidget ("/menubar1")));
		this.menubar1.Name = "menubar1";
		this.vbox1.Add (this.menubar1);
		global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.menubar1]));
		w2.Position = 0;
		w2.Expand = false;
		w2.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow.Name = "GtkScrolledWindow";
		this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
		this.treeview1 = new global::Gtk.TreeView ();
		this.treeview1.CanFocus = true;
		this.treeview1.Name = "treeview1";
		this.GtkScrolledWindow.Add (this.treeview1);
		this.vbox1.Add (this.GtkScrolledWindow);
		global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.GtkScrolledWindow]));
		w4.Position = 1;
		// Container child vbox1.Gtk.Box+BoxChild
		this.hbox1 = new global::Gtk.HBox ();
		this.hbox1.Name = "hbox1";
		this.hbox1.Spacing = 6;
		// Container child hbox1.Gtk.Box+BoxChild
		this.buttonAdd = new global::Gtk.Button ();
		this.buttonAdd.CanFocus = true;
		this.buttonAdd.Name = "buttonAdd";
		this.buttonAdd.UseUnderline = true;
		this.buttonAdd.Label = global::Mono.Unix.Catalog.GetString ("_Add…");
		this.hbox1.Add (this.buttonAdd);
		global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.buttonAdd]));
		w5.Position = 0;
		w5.Expand = false;
		w5.Fill = false;
		// Container child hbox1.Gtk.Box+BoxChild
		this.buttonRemove = new global::Gtk.Button ();
		this.buttonRemove.Sensitive = false;
		this.buttonRemove.CanFocus = true;
		this.buttonRemove.Name = "buttonRemove";
		this.buttonRemove.UseUnderline = true;
		this.buttonRemove.Label = global::Mono.Unix.Catalog.GetString ("_Remove");
		this.hbox1.Add (this.buttonRemove);
		global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.buttonRemove]));
		w6.Position = 1;
		w6.Expand = false;
		w6.Fill = false;
		// Container child hbox1.Gtk.Box+BoxChild
		this.fixed2 = new global::Gtk.Fixed ();
		this.fixed2.Name = "fixed2";
		this.fixed2.HasWindow = false;
		this.hbox1.Add (this.fixed2);
		global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.fixed2]));
		w7.Position = 2;
		// Container child hbox1.Gtk.Box+BoxChild
		this.buttonArchive = new global::Gtk.Button ();
		this.buttonArchive.CanFocus = true;
		this.buttonArchive.Name = "buttonArchive";
		this.buttonArchive.UseUnderline = true;
		this.buttonArchive.Label = global::Mono.Unix.Catalog.GetString ("_Archives…");
		this.hbox1.Add (this.buttonArchive);
		global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.buttonArchive]));
		w8.Position = 3;
		w8.Expand = false;
		w8.Fill = false;
		// Container child hbox1.Gtk.Box+BoxChild
		this.fixed1 = new global::Gtk.Fixed ();
		this.fixed1.WidthRequest = 10;
		this.fixed1.Name = "fixed1";
		this.fixed1.HasWindow = false;
		this.hbox1.Add (this.fixed1);
		global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.fixed1]));
		w9.Position = 4;
		w9.Expand = false;
		this.vbox1.Add (this.hbox1);
		global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox1]));
		w10.Position = 2;
		w10.Expand = false;
		w10.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.statusbar1 = new global::Gtk.Statusbar ();
		this.statusbar1.Name = "statusbar1";
		this.statusbar1.Spacing = 6;
		this.statusbar1.BorderWidth = ((uint)(2));
		// Container child statusbar1.Gtk.Box+BoxChild
		this.progressbar1 = new global::Gtk.ProgressBar ();
		this.progressbar1.WidthRequest = 100;
		this.progressbar1.Name = "progressbar1";
		this.statusbar1.Add (this.progressbar1);
		global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.statusbar1 [this.progressbar1]));
		w11.Position = 1;
		w11.Expand = false;
		this.vbox1.Add (this.statusbar1);
		global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.statusbar1]));
		w12.PackType = ((global::Gtk.PackType)(1));
		w12.Position = 3;
		w12.Expand = false;
		w12.Fill = false;
		this.Add (this.vbox1);
		if ((this.Child != null)) {
			this.Child.ShowAll ();
		}
		this.DefaultWidth = 645;
		this.DefaultHeight = 320;
		this.Show ();
		this.AddItemAction.Activated += new global::System.EventHandler (this.OnAddItemActionActivated);
		this.RemoveSelectedAction.Activated += new global::System.EventHandler (this.OnRemoveSelectedActionActivated);
		this.QuitAction.Activated += new global::System.EventHandler (this.OnQuitActionActivated);
		this.buttonAdd.Clicked += new global::System.EventHandler (this.OnButtonAddClicked);
		this.buttonRemove.Clicked += new global::System.EventHandler (this.OnButtonRemoveClicked);
		this.buttonArchive.Clicked += new global::System.EventHandler (this.OnButtonArchiveClicked);
	}
}
