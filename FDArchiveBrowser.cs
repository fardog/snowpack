using System;
using System.Collections.Generic;
using Gtk;

namespace snowpack
{
	public partial class FDArchiveBrowser : Gtk.Window
	{
		private FDDataStore DataStore;
		private Gtk.TreeStore TreeModel;
		
		public FDArchiveBrowser (FDDataStore ds) : base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
			DataStore = ds;
			TreeModel = new Gtk.TreeStore(
				typeof(string), //file or dirname, 0
				typeof(string), //time modified, 1
				typeof(string), //time uploaded, 2
				typeof(int), //versions, 3
				typeof(string), //checksum, 4
				typeof(string), //archiveid, 5
				typeof(Int64), //id 6
				typeof(Int64), //parent 7
				typeof(bool) //if expanded 8
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
			//this.LoadDirectories(rootIter, new FDArchiveDirectory(0, "", 0));
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
			
			if(selectionPath.Length < 1) return;
			
			foreach (Gtk.TreePath path in selectionPath)
			{
				TreeIter iter;
				if(!selection.TreeView.Model.GetIter(out iter, path)) continue;
				string archiveID = (string)selection.TreeView.Model.GetValue(iter, 5);
				if(!String.IsNullOrWhiteSpace(archiveID)) System.Console.WriteLine(archiveID);
			}
		}

		protected void OnCloseActionActivated (object sender, System.EventArgs e)
		{
			this.Destroy ();
		}
	}
}

