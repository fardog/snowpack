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
	private Gtk.ListStore uploadQueue; //the datastore for the treeview
	private BackgroundWorker updateUIWorker;
	
	
	//local definition of the operation queue item
	public struct ListItem
	{
		public string file;
		public int progress;
		public Guid guid;
		public TreeIter iter;
		
		public ListItem(string fileName, int prog, Guid id, TreeIter it)
		{
			this.file = fileName;
			this.progress = prog;
			this.guid = id;
			this.iter = it;
		}
	}
	
	//this list maintains the items in the treeview separately so we can easily find them later
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
		this.uploadQueue = new ListStore(typeof(string), typeof(string), typeof(Guid));
		
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
		Gtk.TreeSelection selection = treeview1.Selection;
		selection.Mode = Gtk.SelectionMode.Multiple;
		
		//Create the uploader thread
		queueWorker = new BackgroundWorker();
		queueWorker.DoWork += new DoWorkEventHandler(_queueWork);
		queueWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_queueDone);
		if(!queueWorker.IsBusy)
		{
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
		Gtk.MessageDialog reallyQuit = new Gtk.MessageDialog(this,
		                                                     DialogFlags.Modal,
		                                                     MessageType.Question,
		                                                     ButtonsType.YesNo,
		                                                     "Are you sure you want to quit? Your file queue won't be saved!");
		
		if((ResponseType)reallyQuit.Run () == ResponseType.Yes) {
			reallyQuit.Destroy();
			a.RetVal = false;
			Application.Quit ();
		}
		else {
			a.RetVal = true;
			reallyQuit.Destroy();
		}
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
		TreeIter iter = uploadQueue.AppendValues(filePath, "", insertedItem.guid);
		ListItem item = new ListItem(filePath, insertedItem.progress, insertedItem.guid, iter);
		this.items.Add (item);
	}
	
	protected void dequeueSelected()
	{
		TreeSelection selection;
		selection = treeview1.Selection;
		TreePath[] paths = selection.GetSelectedRows();
		TreeModel model = treeview1.Model;
		
		foreach (TreePath path in paths) //remove each item from the tree view and operations queue
		{
			TreeIter iter;
			model.GetIter(out iter, path);
			Guid guid = (Guid)uploadQueue.GetValue (iter, 2);
			if(operationQueue.Remove (guid)) //only remove from the tree if we can remove from the opqueue
				uploadQueue.Remove (ref iter);
			else uploadQueue.SetValue(iter, 1, "Not in Queue"); //we couldn't find it
		}
	}
	
	//changes the "progress" secion on the tree view for the provided guid
	private void updateTree(Guid guid, string message)
	{
		ListItem toUpdate = items.Find (
			delegate(ListItem it) {
				return it.guid == guid;
			}
		);
		
		if(!String.IsNullOrEmpty(toUpdate.file)) 
			treeview1.Model.SetValue(toUpdate.iter, 1, message);
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
		bool processing = false;
		uint ContextID = 1;
		
		while (true) {
			switch(operationQueue.currentStatus)
			{
				case "checksum":
					statusbar1.Pop (ContextID);
					statusbar1.Push (ContextID, "Checksumming \"" +
					                 System.IO.Path.GetFileName(operationQueue.currentFile) + "\"");
					processing = true;
					break;
				case "upload":
					statusbar1.Pop (ContextID);
					statusbar1.Push (ContextID, "Uploading \"" +
					                 System.IO.Path.GetFileName(operationQueue.currentFile) + "\"");
					progressbar1.Fraction = (float)operationQueue.currentItem.progress / 100;
					processing = true;
					break;
				case "store":
					statusbar1.Pop (ContextID);
					statusbar1.Push (ContextID, "Indexing \"" + 
					                 System.IO.Path.GetFileName(operationQueue.currentFile) + "\"");
					processing = true;
					break;
				default:
					if(processing) {
						statusbar1.Pop(ContextID);
						progressbar1.Fraction = 0;
						processing = false;
					}
					break;
			}
			
			if(operationQueue.finished.Count > 0) { //if we have finished queue items to deal with
				FDQueueItem finishItem = operationQueue.finished.Dequeue();
				updateTree (finishItem.guid, "Finished");
			}
			
			if(processing) {
				//update the treeview with the currently uploading file
				try {
					updateTree(operationQueue.currentGuid, "Uploading");
				}
				catch (Exception t) {
					//do nothing, despite how bad of a practice that is
					//TODO do better than nothing
				}
			}
			
			Thread.Sleep (500);
		}
	}
	
	private void _updateUIDone(object sender, RunWorkerCompletedEventArgs e)
	{
		throw new Exception("UI update thread exited! That isn't supposed to happen!");
		//TODO start up UI thread again
	}
	
	protected void AddFileDialog (object sender, System.EventArgs e)
	{
		Gtk.FileChooserDialog addFile = new Gtk.FileChooserDialog("Add file to Upload Queue", 
		                                                          this, 
		                                                          FileChooserAction.Open,
		                                                          "Cancel",ResponseType.Cancel,
		                                                          "Add File",ResponseType.Accept);
		addFile.SelectMultiple = true;
		
		if (addFile.Run() == (int)ResponseType.Accept)
		{
			foreach (string f in addFile.Filenames)
				this.enqueue (f);
		}
		
		addFile.Destroy();
	}
	
	protected void AddDirDialog (object sender, System.EventArgs e)
	{
		Gtk.FileChooserDialog addDir = new Gtk.FileChooserDialog("Add directory to Upload Queue",
		                                                         this,
		                                                         FileChooserAction.SelectFolder,
		                                                         "Cancel",ResponseType.Cancel,
		                                                         "Add Directory",ResponseType.Accept);
		if (addDir.Run () == (int)ResponseType.Accept)
		{
			this.enqueue (addDir.Filename);
		}
		
		addDir.Destroy ();
	}
	
	protected void ArchiveDialog (object sender, System.EventArgs e)
	{
		return;
	}
	
	protected void RemoveItem (object sender, System.EventArgs e)
	{
		this.dequeueSelected();
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
	
	protected void OnButtonAddFileClicked (object sender, System.EventArgs e)
	{
		AddFileDialog(sender, e);
	}

	protected void OnButtonRemoveClicked (object sender, System.EventArgs e)
	{
		RemoveItem(sender, e);
	}

	protected void OnButtonArchiveClicked (object sender, System.EventArgs e)
	{
		ArchiveDialog(sender, e);
	}

	protected void OnAddFileActionActivated (object sender, System.EventArgs e)
	{
		AddFileDialog(sender, e);
	}

	protected void OnRemoveSelectedActionActivated (object sender, System.EventArgs e)
	{
		RemoveItem (sender, e);
	}

	protected void OnQuitActionActivated (object sender, System.EventArgs e)
	{
		MessageDialog md = new MessageDialog(this, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo,
		                                     "Do you want to quit? Your queue won't be saved!");
		ResponseType result = (ResponseType)md.Run ();

		if (result == ResponseType.Yes)
			OnDeleteEvent(sender, (DeleteEventArgs)e);
		else
			md.Destroy ();
	}

	protected void OnAddDirectoryActionActivated (object sender, System.EventArgs e)
	{
		AddDirDialog(sender, e);
	}

	protected void OnButtonAddDirClicked (object sender, System.EventArgs e)
	{
		AddDirDialog(sender, e);
	}

	protected void OnSelectAllActionActivated (object sender, System.EventArgs e)
	{
		Gtk.TreeSelection selection = treeview1.Selection;
		selection.SelectAll();
	}
}

