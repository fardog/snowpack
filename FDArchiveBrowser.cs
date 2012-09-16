using System;
using System.Collections.Generic;
using Gtk;

namespace snowpack
{
	public class ArchiveSelectedEventArgs : EventArgs
	{
		public List<string> archiveIDs;
		public List<Int64> paths;
		public string restoreTo;
	}
	
	public partial class FDArchiveBrowser : Gtk.Window
	{
		private FDDataStore DataStore;
		private Gtk.TreeStore TreeModel;
		public event ArchiveRestoreHandler ArchivesSelectedForRestore;
		public EventArgs e = null;
		public delegate void ArchiveRestoreHandler(FDArchiveBrowser b, ArchiveSelectedEventArgs e);
		
		public FDArchiveBrowser (FDDataStore ds) : base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
			DataStore = ds;
			TreeModel = new Gtk.TreeStore(
				typeof(string),	//file or dirname, 0
				typeof(string),	//time modified, 1
				typeof(string),	//time uploaded, 2
				typeof(int),	//versions, 3
				typeof(string),	//checksum, 4
				typeof(string),	//archiveid, 5
				typeof(Int64),	//id 6
				typeof(Int64),	//parent 7
				typeof(bool)	//if expanded 8
			);
			
			
			//Create the tree view columns and misc
			TreeViewColumn filename = new TreeViewColumn();
			TreeViewColumn modified = new TreeViewColumn();
			TreeViewColumn uploaded = new TreeViewColumn();
			filename.Title = "Filename";
			modified.Title = "Modified";
			uploaded.Title = "Uploaded";
			Gtk.CellRendererText filenameCell = new Gtk.CellRendererText();
			Gtk.CellRendererText modifiedCell = new Gtk.CellRendererText();
			Gtk.CellRendererText uploadedCell = new Gtk.CellRendererText();
			filename.PackStart(filenameCell, true);
			modified.PackStart(modifiedCell, false);
			uploaded.PackStart(uploadedCell, false);
			filename.AddAttribute(filenameCell, "text", 0);
			modified.AddAttribute(modifiedCell, "text", 1);
			uploaded.AddAttribute(uploadedCell, "text", 2);
			treeview1.AppendColumn(filename);
			treeview1.AppendColumn(modified);
			treeview1.AppendColumn(uploaded);
			treeview1.Model = this.TreeModel;
			Gtk.TreeSelection selection = treeview1.Selection;
			selection.Mode = Gtk.SelectionMode.Multiple;
			treeview1.Selection.Changed += OnTreeview1RowSelected;
			
			//insert root node, and populate the first set of directories/files
			TreeIter rootIter = TreeModel.AppendValues ("(root)");
			TreeModel.AppendValues (rootIter, "(loading…)");
		}
		
		private void LoadDirectories (Gtk.TreeIter parentIter, FDArchiveDirectory parent)
		{
			List<FDArchiveDirectory> directories = DataStore.GetDirectories(parent);
			
			foreach (FDArchiveDirectory directory in directories)
			{
				Gtk.TreeIter iter = TreeModel.AppendValues(parentIter, directory.dirname, null, null, 0, null, null, directory.id, directory.parent);
				TreeModel.AppendValues(iter, "(loading…)"); //append a "loading" message to force expansion caret
			}
		}
		
		private void LoadFiles (Gtk.TreeIter parentIter, FDArchiveDirectory parent)
		{
			List<FDArchiveItem> items = DataStore.GetItems(parent);
			
			foreach(FDArchiveItem item in items)
			{
				TreeModel.AppendValues (parentIter,
				                        item.filename,
				                        item.modified.ToLocalTime().ToShortDateString() + " " + item.modified.ToLocalTime().ToShortTimeString(),
				                        item.stored.ToLocalTime().ToShortDateString() + " " + item.stored.ToLocalTime().ToShortTimeString(),
				                        0,
				                        item.checksum,
				                        item.archiveID,
				                        item.id,
				                        item.parent);
			}
			
			RemoveLoadingItem(parentIter);
		}
		
		//clear the "loading" item we use to force expansion caret
		private void RemoveLoadingItem(Gtk.TreeIter parentIter)
		{
			TreeIter loadingNode;
			
			TreeModel.IterChildren(out loadingNode, parentIter);
			if((string)TreeModel.GetValue (loadingNode, 0) == "(loading…)")
				TreeModel.Remove (ref loadingNode);
			
			return;
		}
		
		//the endpoint for clicking the "restore" button in the interface
		private void RestoreItems(object o, System.EventArgs e)
		{
			//ask where we're restoring to
			Gtk.FileChooserDialog restoreTo = new Gtk.FileChooserDialog("Where should files be saved?",
		                                                         this,
		                                                         FileChooserAction.SelectFolder,
		                                                         "Cancel",ResponseType.Cancel,
		                                                         "Restore",ResponseType.Accept);
			string restorePath;
			
			if (restoreTo.Run () == (int)ResponseType.Accept)
			{
				restorePath = restoreTo.Filename;
				restoreTo.Destroy();
			}
			else
			{
				restoreTo.Destroy();
				return;
			}
			
			//get the selected items
			Gtk.TreeSelection selection = treeview1.Selection;
			Gtk.TreePath[] selectionPath = selection.GetSelectedRows();
			
			if(selectionPath.Length < 1) return;
			
			//build our arguments to send to the listener
			ArchiveSelectedEventArgs args = new ArchiveSelectedEventArgs();
			args.archiveIDs = new List<string>();
			args.paths = new List<Int64>();
			args.restoreTo = restorePath;
			
			//iterate over the selections
			foreach (Gtk.TreePath path in selectionPath)
			{
				TreeIter iter;
				
				if(!selection.TreeView.Model.GetIter(out iter, path)) continue;
				string archiveID = (string)selection.TreeView.Model.GetValue(iter, 5); //if we have an archive id, add it to the list
				if(!String.IsNullOrWhiteSpace(archiveID)) args.archiveIDs.Add (archiveID);
				else { //otherwise, we're on a directory
					Int64 directoryID = (Int64)selection.TreeView.Model.GetValue(iter, 6);
					args.paths.Add(directoryID);
				}
			}
			
			//fire the event
			ArchivesSelectedForRestore(this, args);
		}

		protected void OnTreeview1RowExpanded (object o, Gtk.RowExpandedArgs args)
		{
			FDArchiveDirectory parent = new FDArchiveDirectory(
				(Int64)treeview1.Model.GetValue (args.Iter, 7),
				(string)treeview1.Model.GetValue (args.Iter, 0),
				(Int64)treeview1.Model.GetValue (args.Iter, 6)
			);
			
			if((bool)TreeModel.GetValue(args.Iter, 8)) return;
			TreeModel.SetValue(args.Iter, 8, true);
			
			this.LoadDirectories(args.Iter, parent);
			this.LoadFiles(args.Iter, parent);
		}
		
		//when we select a vault item, retrieve its id (if it has one)
		private void OnTreeview1RowSelected (object sender, System.EventArgs e) 
		{
			Gtk.TreeSelection selection = sender as Gtk.TreeSelection;
			Gtk.TreePath[] selectionPath = selection.GetSelectedRows();
			
			if(selectionPath.Length < 1) {
				buttonRestore.Sensitive = false;
				return;
			}
			
			buttonRestore.Sensitive = true;
		}

		protected void OnCloseActionActivated (object sender, System.EventArgs e)
		{
			this.Destroy ();
		}

		protected void OnButtonRestoreActivated (object sender, System.EventArgs e)
		{
			RestoreItems (sender, e);
		}

		protected void OnButtonRestoreClicked (object sender, System.EventArgs e)
		{
			RestoreItems (sender, e);
		}
		
		//captures the right and left key presses to expand/collapse the tree node
		//this exists to make nautilus-style treeview browsing a crazy reality
		[GLib.ConnectBefore] //need to do this so i get called first
		protected void OnTreeview1KeyPressEvent (object tree, Gtk.KeyPressEventArgs args)
		{
			TreeSelection selection = treeview1.Selection;
			TreePath[] selectionPath = selection.GetSelectedRows();
			
			if(selectionPath.Length > 1) return; //do nothing if there's more than one item selected
			
			foreach (TreePath path in selectionPath)
			{
				if(args.Event.Key == Gdk.Key.Right)
				{
					treeview1.ExpandRow(path, false);
				}
				else if (args.Event.Key == Gdk.Key.Left)
				{
					if(treeview1.GetRowExpanded(path))
						treeview1.CollapseRow(path);
					else {
						path.Up();
						treeview1.SetCursor(path, null, false);
						selection.UnselectAll();
						selection.SelectPath(path);
					}
				}
			}
			
			//selects the middle column so we don't have to hear a 'bonk' while navigating
			//since the treeview selects columns with left/right keys (although invisible and useless for us)
			//it will still complain loudly when it 'hits' the end or start, visible or not
			TreeSelection innerSelection = treeview1.Selection;
			TreePath[] innerTreePath = innerSelection.GetSelectedRows();
			if(innerTreePath.Length < 1) return;
			innerSelection.UnselectAll();
			foreach (TreePath path in innerTreePath)
			{
				treeview1.SetCursor(innerTreePath[0], treeview1.Columns[1], false);
			}
			
		}
	}
}

