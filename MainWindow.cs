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
using snowpack;

public partial class MainWindow: Gtk.Window
{	
	private string currentFile;
	private string currentDescription;
	private string currentChecksum;
	private string currentArchiveId;
	private FileAttributes currentAttributes;
	private FileInfo currentInfo;
	private System.ComponentModel.BackgroundWorker uploadWorker;
	private System.ComponentModel.BackgroundWorker checksumWorker;
	private System.ComponentModel.BackgroundWorker waitWorker;
	private FDUserSettings UserSettings;
	private FDDataStore DataStore;
	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		UserSettings = new FDUserSettings();
		DataStore = new FDDataStore(UserSettings.CurrentDataStore);
		
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void onSelectionChanged (object sender, EventArgs e)
	{	
		if (String.IsNullOrWhiteSpace(fileChooser.Filename)) { //nothing is selected in the picker
			buttonUpload.Sensitive = false;
			return;
		} 
		
		//see if we have a file or a directory selected, and set interface accordingly
		currentAttributes = File.GetAttributes(fileChooser.Filename);
		if ((currentAttributes & FileAttributes.Directory) == FileAttributes.Directory) {
			buttonUpload.Label = "Open";
			buttonUpload.Sensitive = true;
		} else {
			currentInfo = new FileInfo(fileChooser.Filename);
			buttonUpload.Label = "Upload";
			buttonUpload.Sensitive = true;
		}
	}	

	protected void onFileActivated (object sender, EventArgs e)
	{
		//just pass to the upload clicked function, the operation is the same
		this.onUploadClicked(sender, e);
	}	

	protected void onUploadClicked (object sender, EventArgs e)
	{
		if (fileChooser.Filename == null) {
			buttonUpload.Sensitive = false;
			return;
		}
		
		//determine if we're on a file or a directory, and act accordingly
		FileAttributes attr = File.GetAttributes(fileChooser.Filename);
		if ((attr & FileAttributes.Directory) == FileAttributes.Directory) { //we're on a directory, update the picker
			fileChooser.SetCurrentFolder(fileChooser.Filename);
		} else { //we're on a file, start the upload
			//set our UI
			statusBar.Push(statusBar.GetContextId("Upload"), "Uploading: " + System.IO.Path.GetFileName(fileChooser.Filename) + "…");
			buttonUpload.Sensitive = false;
			buttonCancel.Sensitive = true;
			fileChooser.Sensitive = false;
			
			//perform the upload
			this.uploadFile(fileChooser.Filename, System.IO.Path.GetFileName (fileChooser.Filename));
		}
	}

	private void uploadFile (string fileName, string fileDescription)
	{
		this.currentFile = fileName;
		this.currentDescription = fileDescription;
		
		checksumWorker = new System.ComponentModel.BackgroundWorker();
		checksumWorker.DoWork += new DoWorkEventHandler(_checksumWork);
		checksumWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_checksumComplete);
		checksumWorker.WorkerSupportsCancellation = false;

		uploadWorker = new System.ComponentModel.BackgroundWorker();
		uploadWorker.DoWork += new DoWorkEventHandler (_uploadFileWork);
		uploadWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_uploadFileCompleted);
		uploadWorker.WorkerSupportsCancellation = false;
		
		waitWorker = new System.ComponentModel.BackgroundWorker();
		waitWorker.DoWork += new DoWorkEventHandler (_waitWork);
		waitWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_waitComplete);
		waitWorker.WorkerSupportsCancellation = false;
		
		if(!checksumWorker.IsBusy) {
			checksumWorker.RunWorkerAsync(fileName); //calculate the file's checksum
		}
		
		if(!uploadWorker.IsBusy) {
			uploadWorker.RunWorkerAsync(fileName); //launch the upload
		}
		
		if(!waitWorker.IsBusy) {
			waitWorker.RunWorkerAsync(true); //wait on threads
		}
	}

	private void _uploadFileWork(object sender, DoWorkEventArgs e)
	{
		BackgroundWorker worker = sender as BackgroundWorker;

		e.Result = _initiateUpload((string)e.Argument, worker, e);
	}

	private void _uploadFileCompleted (object sender, RunWorkerCompletedEventArgs e)
	{
		if (e.Error != null) //If there was an error
		{
			statusBar.Push (statusBar.GetContextId("Error'd"), "Error uploading file!");
			Console.WriteLine(e.Error.Message);
		}
		else if (e.Cancelled) //If we cancelled the operation
		{
			statusBar.Push (statusBar.GetContextId("Cancelled"), "Canceled upload: " + System.IO.Path.GetFileName(currentFile));
		}
		else //We had some success right here!
		{
			statusBar.Push (statusBar.GetContextId("Success"), "Successfully uploaded: " + System.IO.Path.GetFileName(currentFile));
		}
		Amazon.Glacier.Transfer.UploadResult result = (Amazon.Glacier.Transfer.UploadResult)e.Result;
		currentArchiveId = result.ArchiveId;
	}
	
	private void _uploadFileProgress(object sender, Amazon.Runtime.StreamTransferProgressArgs e) 
	{
		progressBar.Fraction = (float)e.PercentDone / 100;
	}
	
	private void _checksumWork(object sender, DoWorkEventArgs e)
	{
		BackgroundWorker worker = sender as BackgroundWorker;
		
		e.Result = _initiateChecksum((string)e.Argument, worker, e);
	}
	
	private void _checksumComplete (object sender, RunWorkerCompletedEventArgs e)
	{
		currentChecksum = (string)e.Result;
	}
	
	private void _waitWork (object sender, DoWorkEventArgs e)
	{
		//BackgroundWorker worker = sender as BackgroundWorker;
		Thread.Sleep (250); //wait in case other threads haven't started
		
		while(uploadWorker.IsBusy || checksumWorker.IsBusy)
		{
			System.Console.WriteLine("Waiting");
			Thread.Sleep(1000);
		}
	}
	
	private void _waitComplete (object sender, RunWorkerCompletedEventArgs e)
	{
		buttonUpload.Sensitive = true;
		buttonCancel.Sensitive = false;
		fileChooser.Sensitive = true;
		progressBar.Fraction = 0;
		System.Console.WriteLine("File: "+currentFile);
		System.Console.WriteLine("Checksum: "+currentChecksum);
		System.Console.WriteLine("ArchiveId: "+currentArchiveId);
		
		
		DataStore.InsertFile(currentFile, currentChecksum, currentInfo.Length, currentInfo.LastWriteTimeUtc, currentArchiveId);
	}

	private Amazon.Glacier.Transfer.UploadResult _initiateUpload (string file, BackgroundWorker worker, DoWorkEventArgs e)
	{
		if (String.IsNullOrEmpty (file)) {
			throw new ArgumentException ("A file wasn't selected for upload.");
		}
		FDGlacier glacier = new FDGlacier ();
		glacier.archiveDescription = currentDescription;
		glacier.setCallback(this._uploadFileProgress);
		glacier.uploadFile (currentFile);

		return glacier.getResult ();
	}
	
	private string _initiateChecksum (string file, BackgroundWorker worker, DoWorkEventArgs e)
	{
		if(String.IsNullOrEmpty (file)) {
			throw new ArgumentException ("A file wasn't selected for checksum.");
		}
		
		FDChecksum fileChecksum = new FDChecksum(file);
		fileChecksum.CalculateChecksum();
		
		return fileChecksum.checksum;
	}

	protected void onCancelClicked (object sender, EventArgs e)
	{
		System.Console.WriteLine ("I don't work! hahahaha whoops");
	}

	public void onUploadFinish (object obj, EventArgs args)
	{
		fileChooser.Sensitive = true;
		buttonCancel.Sensitive = false;
		statusBar.Pop (statusBar.GetContextId("Upload"));
	}
}
