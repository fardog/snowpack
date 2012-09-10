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
	private FDOperationLog log;
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
		DataStore = new FDDataStore(UserSettings.CurrentDataStore, FDLogVerbosity.Warning);
		log = new FDOperationLog(DataStore, FDLogVerbosity.Warning, true);
		operationQueue = new FDOperationQueue(DataStore, UserSettings, log);
		treeview1.Selection.Changed += RemoveSensitive;
		
		//Create the item list
		items = new List<ListItem>();
		this.uploadQueue = new ListStore(typeof(Gdk.Pixbuf), typeof(string), typeof(string), typeof(Guid), typeof(int));
		
		//Create the tree view columns and misc
		TreeViewColumn icon = new TreeViewColumn();
		TreeViewColumn filename = new TreeViewColumn();
		TreeViewColumn progress = new TreeViewColumn();
		icon.Title = "";
		filename.Title = "Filename";
		progress.Title = "Progress";
		Gtk.CellRendererPixbuf iconCell = new Gtk.CellRendererPixbuf();
		Gtk.CellRendererText filenameCell = new Gtk.CellRendererText();
		Gtk.CellRendererText progressCell = new Gtk.CellRendererText();
		icon.PackStart(iconCell, true);
		filename.PackStart(filenameCell, true);
		progress.PackStart(progressCell, false);
		icon.AddAttribute(iconCell, "pixbuf", 0);
		filename.AddAttribute(filenameCell, "text", 1);
		progress.AddAttribute(progressCell, "text", 2);
		treeview1.AppendColumn(icon);
		treeview1.AppendColumn(filename);
		treeview1.AppendColumn(progress);
		treeview1.Model = this.uploadQueue;
		Gtk.TreeSelection selection = treeview1.Selection;
		selection.Mode = Gtk.SelectionMode.Multiple;
		
		//Verify our settings. If they aren't present, we need to show the settings dialog
		if(String.IsNullOrWhiteSpace(UserSettings.AWSAccessKey) ||
		   String.IsNullOrWhiteSpace(UserSettings.AWSSecretKey) ||
		   String.IsNullOrWhiteSpace(UserSettings.AWSGlacierVaultName))
		{
			FDPreferences preferences = new FDPreferences(this, DialogFlags.Modal, UserSettings, true);
			preferences.Run();
			preferences.Destroy();
		}
		
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
		
		
		//System.Console.WriteLine (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
		//System.Console.WriteLine (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		//If our queue is empty, just quit
		if(operationQueue.QueueSize() == 0) {
			log.Store(this.ToString(), "Shutdown: Queue is empty", null, FDLogVerbosity.Information);
			Application.Quit ();
			a.RetVal = false;
			return;
		}
		
		//Otherwise, we need to prompt the user
		Gtk.MessageDialog reallyQuit = new Gtk.MessageDialog(this,
		                                                     DialogFlags.Modal,
		                                                     MessageType.Warning,
		                                                     ButtonsType.YesNo,
		                                                     "Do you want to quit? This will stop all transfers and clear your queue!");
		
		if((ResponseType)reallyQuit.Run () == ResponseType.Yes) {
			reallyQuit.Destroy();
			operationQueue.StopQueue();
			log.Store (this.ToString(), "Shutdown: Queue has items", null, FDLogVerbosity.Information);
			Thread.Sleep (1000); //wait for queue shutdown
			a.RetVal = false;
			Application.Quit ();
		}
		else {
			a.RetVal = true;
			reallyQuit.Destroy();
		}
	}
	
	protected void enqueueUpload(string filePath)
	{
		if(String.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException("You must provide a valid file path to enqueue.");
		FileAttributes attr = File.GetAttributes(filePath);
		
		
		//Add to the OperationQueue
		FDQueueItem addItem = new FDQueueItem(filePath, attr, FDItemStatus.QueuedUpload);
		operationQueue.Add (addItem);
		
		//Add to the queue view
		TreeIter iter = uploadQueue.AppendValues(new Gdk.Pixbuf(null, "snowpack.assets.upload.png"), filePath, "", addItem.guid);
		ListItem item = new ListItem(filePath, addItem.progress, addItem.guid, iter);
		this.items.Add (item);
	}
	
	protected void enqueueDownload(string archiveID)
	{
		if(String.IsNullOrWhiteSpace(archiveID)) throw new ArgumentNullException("You must provide a valid Archive ID to enqueue.");
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
			Guid guid = (Guid)uploadQueue.GetValue (iter, 3);
			if(operationQueue.Remove (guid)) //only remove from the tree if we can remove from the opqueue
				uploadQueue.Remove (ref iter);
			else if ((int)uploadQueue.GetValue(iter, 4) == FDItemStatus.FinishedUploading) //it's finished, so we can remove
				uploadQueue.Remove (ref iter);
			else { //we couldn't find it, set an error
				uploadQueue.SetValue (iter, 2, "Not in Queue"); 
				uploadQueue.SetValue (iter, 4, FDItemStatus.Error);
				log.Store (this.ToString(), "Item not in queue", "An item that was dequeued was missing from the GUI. " +
					"You should check your archive to ensure everything was uploaded as expected.", FDLogVerbosity.Error);
			}
		}
	}
	
	//changes the "progress" secion on the tree view for the provided guid
	private void updateTree(Guid guid, string message, int status)
	{
		ListItem toUpdate = items.Find (
			delegate(ListItem it) {
				return it.guid == guid;
			}
		);
		
		if(!String.IsNullOrEmpty(toUpdate.file)) {
			treeview1.Model.SetValue (toUpdate.iter, 2, message);
			treeview1.Model.SetValue (toUpdate.iter, 4, status);
		}
	}
	
	private void _queueWork(object sender, DoWorkEventArgs e)
	{
		operationQueue.ProcessUploadQueueWorker();
	}
	
	private void _queueDone(object sender, RunWorkerCompletedEventArgs e)
	{
		if(operationQueue.wasStopped) //we were stopped intentionally
			return;
		else throw new Exception("Upload queue thread stopped unexpectedly.");
	}
	
	private void _updateUIWork(object sender, DoWorkEventArgs e)
	{
		uint ContextID = 1; //contextid for our statusbar updates
		string LastUploadFile = null;
		bool LastPopped = true;
		
		while (true) {
			//TODO Implement individual file status and progressbar
			
			//see if we have some currently running items to deal with
			FDQueueItem currentItem;
			while(operationQueue.current.TryPop(out currentItem))
			{
				string itemStatus;
				switch (currentItem.status) //set a status message
				{
					case FDItemStatus.Uploading:
						itemStatus = "Uploading";
						break;
					case FDItemStatus.Downloading:
						itemStatus = "Downloading";
						break;
					default:
						itemStatus = "Error: Check Log!";
						break;
				}
				
				//update the treeview
				updateTree(currentItem.guid, itemStatus, currentItem.status);
			}
			
			//see if we have some finished items to deal with
			FDQueueItem finishItem;
			while(operationQueue.finished.TryPop(out finishItem))
			{
				string itemStatus;
				switch (finishItem.status) //set a status message
				{
					case FDItemStatus.FinishedUploading:
						itemStatus = "Uploaded";
						break;
					case FDItemStatus.FinishedDownloading:
						itemStatus = "Downloaded";
						break;
					default:
						itemStatus = "Error: Check Log!";
						break;
				}
				
				//update the treeview
				updateTree(finishItem.guid, itemStatus, finishItem.status);
			}
			
			//Now update the statusbar with the current individual item
			string UploadFile = operationQueue.UploadFile;
			int UploadProgress = operationQueue.UploadProgress;
			if(operationQueue.UploadRunning)
			{
				if(!String.IsNullOrWhiteSpace(UploadFile) && UploadFile != LastUploadFile)
				{
					statusbar1.Pop(ContextID);
					statusbar1.Push (ContextID, "Uploading: " + UploadFile);
					LastUploadFile = UploadFile;
					LastPopped = false;
				}
				
				if(UploadProgress > 0) progressbar1.Fraction = (float)UploadProgress/100;
				else progressbar1.Fraction = 0.0;
			}
			else if (!LastPopped)
			{
				statusbar1.Pop(ContextID);
				progressbar1.Fraction = 0.0;
				LastPopped = true;
			}
			
			Thread.Sleep (500);
		}
	}
	
	private void _updateUIDone(object sender, RunWorkerCompletedEventArgs e)
	{
		log.Store (this.ToString(), "UI Worker Thread has quit", null, FDLogVerbosity.Error);
		
		updateUIWorker.Dispose();
		updateUIWorker = new BackgroundWorker();
		updateUIWorker.DoWork += new DoWorkEventHandler(_updateUIWork);
		updateUIWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_updateUIDone);
		if(!updateUIWorker.IsBusy)
		{
			updateUIWorker.RunWorkerAsync();
		}
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
				this.enqueueUpload (f);
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
			this.enqueueUpload (addDir.Filename);
		}
		
		addDir.Destroy ();
	}
	
	protected void ArchiveDialog (object sender, System.EventArgs e)
	{
		FDArchiveBrowser browser = new FDArchiveBrowser(this.DataStore);
		browser.Show ();
	}
	
	protected void RemoveItem (object sender, System.EventArgs e)
	{
		this.dequeueSelected();
	}
	
	//sets the sensitivity of the remove button, depending on the selection state of the tree
	protected void RemoveSensitive (object sender, System.EventArgs e)
	{
		Gtk.TreeSelection selection = sender as Gtk.TreeSelection;
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
		OnDeleteEvent(this, new DeleteEventArgs());
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

	protected void OnPreferencesActionActivated (object sender, System.EventArgs e)
	{
		//The preference window itself handles saving settings, so we just have to show it here.
		FDPreferences preferences = new FDPreferences(this, DialogFlags.Modal, UserSettings);
		preferences.Run ();
		preferences.Destroy();
	}

	protected void OnAboutActionActivated (object sender, System.EventArgs e)
	{
		Gtk.AboutDialog dialog = new AboutDialog();
		
		dialog.ProgramName = "snowpack";
		dialog.Version = "0.1.0";
		dialog.Comments = "A cross-platform Amazon Glacier Client";
		dialog.Authors = new string [] {"Nathan Wittstock"};
		dialog.Website = "http://fardogllc.com/";
		
		dialog.Run ();
		dialog.Destroy ();
	}

	protected void OnSnowpackOnGithubActionActivated (object sender, System.EventArgs e)
	{
		string target = "https://github.com/fardog/snowpack";
		
		try
		{
			System.Diagnostics.Process.Start(target);
		}
		catch (Exception f)
		{
			Gtk.MessageDialog dialog = new Gtk.MessageDialog(this, 
			                                                 DialogFlags.Modal, 
			                                                 MessageType.Error, 
			                                                 ButtonsType.Ok, 
			                                                 "Couldn't launch default web browser. Error was: " + f.Message);
			dialog.Run();
			dialog.Destroy();
		}
	}

	protected void OnArchiveBrowserActionActivated (object sender, System.EventArgs e)
	{
		ArchiveDialog(sender, e);
	}

	protected void OnRetrieveJobListActionActivated (object sender, System.EventArgs e)
	{
		FDGlacier glacier = new FDGlacier(UserSettings, log, "listjobs");
		Amazon.Glacier.Model.ListJobsResult jobs = glacier.ListJobs();
		
		foreach (Amazon.Glacier.Model.GlacierJobDescription description in jobs.JobList)
		{
			Console.WriteLine("ID: " + description.JobId + ", " + description.StatusMessage);
		}
	}
}

