using System;
using System.IO;
using Gtk;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using snowpack;

public partial class FDQueueView : Gtk.Window
{
	private FDOperationQueue operationQueue;
	private BackgroundWorker queueWorker;
	private FDUserSettings UserSettings;
	private FDDataStore DataStore;
	private Gtk.ListStore uploadQueue;
	private BackgroundWorker updateUIWorker;
	private ArchiveBrowser archiveBrowser;
	
	public struct ListItem
	{
		public string file;
		public int progress;
		public FDQueueItem item;
		
		public ListItem(string fileName, int prog, FDQueueItem fdq)
		{
			this.file = fileName;
			this.progress = prog;
			this.item = fdq;
		}
	}
	
	private List<ListItem> items;
		
	
	public FDQueueView () : base(Gtk.WindowType.Toplevel)
	{
		Build ();
		UserSettings = new FDUserSettings();
		DataStore = new FDDataStore(UserSettings.CurrentDataStore);
		operationQueue = new FDOperationQueue(DataStore);
		treeview1.Selection.Changed += RemoveSensitive;
		
		//Create the item list
		items = new List<ListItem>();
		this.uploadQueue = new ListStore(typeof(string), typeof(string));
		
		//Create the tree view columns
		TreeViewColumn filename = new TreeViewColumn();
		TreeViewColumn progress = new TreeViewColumn();
		filename.Title = "Filename";
		progress.Title = "Progress";
		Gtk.CellRendererText filenameCell = new Gtk.CellRendererText();
		Gtk.CellRendererText progressCell = new Gtk.CellRendererText();
		filename.PackStart(filenameCell, true);
		progress.PackStart(progressCell, false);
		filename.AddAttribute(filenameCell, "text", 0);
		progress.AddAttribute(progressCell, "text", 1);
		treeview1.AppendColumn(filename);
		treeview1.AppendColumn(progress);
		treeview1.Model = this.uploadQueue;
		
		//Create the uploader thread
		queueWorker = new BackgroundWorker();
		queueWorker.DoWork += new DoWorkEventHandler(_queueWork);
		queueWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_queueDone);
		if(!queueWorker.IsBusy)
		{
			statusbar1.Push (statusbar1.GetContextId("workerStart"), "Upload worker thread started…");
			queueWorker.RunWorkerAsync();
		}
		
		//Now create the UI update thread that watches the uploader
		updateUIWorker = new BackgroundWorker();
		updateUIWorker.DoWork += new DoWorkEventHandler(_updateUIWork);
		updateUIWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_updateUIDone);
		if(!updateUIWorker.IsBusy)
		{
			updateUIWorker.RunWorkerAsync();
		}
		
		//Housekeeping functions for window
		this.DeleteEvent += OnDeleteEvent;
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		//TODO Save File Queue
		Application.Quit ();
		a.RetVal = true;
	}
	
	protected void enqueue(string filePath)
	{
		if(String.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException("You must provide a valid file path to enqueue.");
		FileAttributes attr = File.GetAttributes(filePath);
		
		
		//Add to the OperationQueue
		FDQueueItem addItem = new FDQueueItem(filePath, attr);
		int success = operationQueue.Add (addItem);
		FDQueueItem insertedItem = operationQueue.Get(success);
		
		//Add to the queue view
		ListItem item = new ListItem(filePath, insertedItem.progress, insertedItem);
		this.items.Add (item);
		uploadQueue.AppendValues(item.file, "");
	}
	
	private void _queueWork(object sender, DoWorkEventArgs e)
	{
		operationQueue.ProcessQueueWorker();
	}
	
	private void _queueDone(object sender, RunWorkerCompletedEventArgs e)
	{
		throw new Exception("Queue worker thread exited! That isn't supposed to happen!");
	}
	
	private void _updateUIWork(object sender, DoWorkEventArgs e)
	{
		while (true) {
			switch(operationQueue.currentStatus)
			{
			case "checksum":
				statusbar1.Push (statusbar1.GetContextId("checksum"), "Checksumming \"" +
				                 System.IO.Path.GetFileName(operationQueue.currentFile) + "\"");
				break;
			case "upload":
				statusbar1.Push (statusbar1.GetContextId("upload"), "Uploading \"" +
				                 System.IO.Path.GetFileName(operationQueue.currentFile) + "\"");
				progressbar1.Fraction = (float)operationQueue.currentItem.progress / 100;
				break;
			case "store":
				statusbar1.Push (statusbar1.GetContextId("store"), "Indexing \"" + 
				                 System.IO.Path.GetFileName(operationQueue.currentFile) + "\"");
				break;
			default:
				statusbar1.Push (statusbar1.GetContextId("idle"), "");
				break;
			}
			
			Thread.Sleep (500);
		}
	}
	
	private void _updateUIDone(object sender, RunWorkerCompletedEventArgs e)
	{
		throw new Exception("UI update thread exited! That isn't supposed to happen!");
	}
	
	protected void AddDialog (object sender, System.EventArgs e)
	{
		Gtk.FileChooserDialog addFile = new Gtk.FileChooserDialog("Add file to Upload Queue", 
		                                                          this, 
		                                                          FileChooserAction.Open,
		                                                          "Cancel",ResponseType.Cancel,
		                                                          "Open",ResponseType.Accept);
		if (addFile.Run() == (int)ResponseType.Accept)
		{
			this.enqueue (addFile.Filename);
		}
		
		addFile.Destroy();
	}
	
	protected void ArchiveDialog (object sender, System.EventArgs e)
	{
		archiveBrowser = new ArchiveBrowser(UserSettings.CurrentDataStore);
		archiveBrowser.Show ();
	}
	
	protected void RemoveItem (object sender, System.EventArgs e)
	{
		TreeSelection selection;
		selection = treeview1.Selection;
		TreePath[] paths = selection.GetSelectedRows();
		TreeModel model = treeview1.Model;
		
		foreach (TreePath path in paths)
		{
			TreeIter iter;
			model.GetIter(out iter, path);
			uploadQueue.Remove (ref iter);
		}
	}
	
	protected void RemoveSensitive (object sender, System.EventArgs e)
	{
		Gtk.TreeSelection selection = treeview1.Selection;
		Gtk.TreePath[] selectionPath = selection.GetSelectedRows();
		
		if(selectionPath.Length < 1) //nothing is selected
		{
			buttonRemove.Sensitive = false;
			RemoveSelectedAction.Sensitive = false;
			return;
		}
		
		buttonRemove.Sensitive = true;
		RemoveSelectedAction.Sensitive = true;
	}
	
	/* UI Interaction Bits
	 * go below here
	 * */
	
	protected void OnButtonAddClicked (object sender, System.EventArgs e)
	{
		AddDialog(sender, e);
	}

	protected void OnButtonRemoveClicked (object sender, System.EventArgs e)
	{
		RemoveItem(sender, e);
	}

	protected void OnButtonArchiveClicked (object sender, System.EventArgs e)
	{
		ArchiveDialog(sender, e);
	}

	protected void OnAddItemActionActivated (object sender, System.EventArgs e)
	{
		AddDialog(sender, e);
	}

	protected void OnRemoveSelectedActionActivated (object sender, System.EventArgs e)
	{
		RemoveItem (sender, e);
	}

	protected void OnQuitActionActivated (object sender, System.EventArgs e)
	{
		OnDeleteEvent(sender, (DeleteEventArgs)e);
	}
}

