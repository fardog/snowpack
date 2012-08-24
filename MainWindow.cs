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
	private FDGlacier glacier;
	private System.ComponentModel.BackgroundWorker uploadWorker;
	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
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
		FileAttributes attr = File.GetAttributes(fileChooser.Filename);
		if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
			buttonUpload.Label = "Open";
			buttonUpload.Sensitive = true;
		} else {
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

		uploadWorker = new System.ComponentModel.BackgroundWorker();
		uploadWorker.DoWork += new DoWorkEventHandler (_uploadFileWork);
		uploadWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_uploadFileCompleted);
		uploadWorker.WorkerSupportsCancellation = true;
		
		if(!uploadWorker.IsBusy) {
			uploadWorker.RunWorkerAsync(fileName); //launch the upload
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

		buttonUpload.Sensitive = true;
		buttonCancel.Sensitive = false;
		fileChooser.Sensitive = true;
		progressBar.Fraction = 0;
	}
	
	private void _uploadFileProgress(object sender, Amazon.Runtime.StreamTransferProgressArgs e) 
	{
		progressBar.Fraction = (float)e.PercentDone / 100;
	}

	private Amazon.Glacier.Transfer.UploadResult _initiateUpload (string file, BackgroundWorker worker, DoWorkEventArgs e)
	{
		if (String.IsNullOrEmpty (file)) {
			throw new ArgumentException ("A file wasn't selected for upload.");
		}
		glacier = new FDGlacier ();
		glacier.archiveDescription = currentDescription;
		glacier.setCallback(this._uploadFileProgress);
		glacier.uploadFile (currentFile);

		return glacier.getResult ();
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
