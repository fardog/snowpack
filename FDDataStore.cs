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
using System.Data;
using System.IO;
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace snowpack
{
	public class FDDataStore
	{
		private string DataStoreURI;
		private IDbConnection dbcon;
		public bool ready = false;
		private FDLogVerbosity verbosity;
		
		public FDDataStore (string DataStore, FDLogVerbosity logVerbosity)
		{
			DataStoreURI = DataStore;
			verbosity = logVerbosity;
			if(!File.Exists (DataStoreURI)) //if the file doesn't exist yet 
			{
				if(!File.Exists(Path.GetDirectoryName(DataStoreURI))) //if the directory doesn't exist
				{
					try
					{
						Directory.CreateDirectory(Path.GetDirectoryName (DataStoreURI));
					}
					catch (Exception e)
					{
						throw new Exception("Couldn't create data store directory. " + e.Message);
					}
				}
				
				try //create the file
				{
					File.Create (DataStoreURI);
				}
				catch (Exception e)
				{
					throw new Exception("Couldn't create the data store file. " + e.Message);
				}
			
			}
			
			//Now we'll connect
			string connectionString = "URI=file:" + DataStoreURI;
			dbcon = (IDbConnection) new SqliteConnection(connectionString);
			dbcon.Open();
			
			if(this.CheckTables() > 0) this.ready = true; //create our tables if they don't exist
		}
		
		private int CheckTables() 
		{
			IDbCommand createTables = dbcon.CreateCommand();
			
			createTables.CommandText = 
				"CREATE TABLE IF NOT EXISTS directory ( " +
				"directory_id INTEGER PRIMARY KEY, " +
				"directory_name VARCHAR(255)," +
				"directory_parent INTEGER );" +
				"CREATE TABLE IF NOT EXISTS file (" +
				"file_id INTEGER PRIMARY KEY," +
				"file_name VARCHAR(255)," +
				"file_checksum VARCHAR(32)," +
				"file_size INTEGER," +
				"file_modified INTEGER," +
				"file_archiveid VARCHAR(138)," +
				"file_directory INTEGER," +
				"file_uploaded INTEGER );" +
				"CREATE TABLE IF NOT EXISTS queue ( " +
				"queue_id INTEGER PRIMARY KEY," +
				"queue_path VARCHAR(1024)," +
				"queue_guid CHAR(36)," +
				"queue_type VARCHAR(10)," +
				"queue_enqueued INTEGER );" +
				"CREATE TABLE IF NOT EXISTS log ( " +
				"log_id INTEGER PRIMARY KEY," +
				"log_importance INTEGER," +
				"log_component VARCHAR(32)," +
				"log_short TEXT," +
				"log_long TEXT," +
				"log_timestamp INTEGER );";
			
			int retvalue = 0;
			
			try 
			{
				retvalue = createTables.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				throw new Exception("Couldn't create the sqlite tables. " + e.Message);
			}
			
			return retvalue;
		}
		
		public string CheckExists(string checksum, long size)
		{
			IDbCommand checkFileExists = dbcon.CreateCommand();
			
			checkFileExists.CommandText = 
				"SELECT `file_archiveid` from `file` WHERE " +
				"`file_checksum` = \"" + checksum + "\" " +
				"AND `file_size` = " + size.ToString() + " LIMIT 1;";
			
			IDataReader reader = checkFileExists.ExecuteReader();
			
			if(reader.Read ()) return reader.GetString (0);
			return null;
		}
		
		public int InsertFile(string path, string checksum, long size, DateTime modified, string archiveId)
		{
			//first we need to create the directory tree in SQL if it doesn't exist yet
			string pathDirectories = Path.GetDirectoryName(path);
			string[] directories = pathDirectories.Split(Path.DirectorySeparatorChar);
			
			Int64 parent = insertDirectoryTree(directories); //creates directory tree and gets parent id
			
			//we need to verify that the exact file isn't already inserted
			IDbCommand checkFile = dbcon.CreateCommand();
			checkFile.CommandText = 
				"SELECT `file_id` FROM `file` WHERE " +
				"`file_name` = \"" + Path.GetFileName(path) + "\" " +
				"AND `file_checksum` = \"" + checksum + "\" " +
				"AND `file_directory` = " + parent.ToString() + ";";
			IDataReader reader = checkFile.ExecuteReader();
			if(reader.Read ()) return 0; //the file already exists in that directory
			
			//now we have the parent, so insert the file into db
			IDbCommand insertFile = dbcon.CreateCommand();
			insertFile.CommandText = 
				"INSERT INTO `file` VALUES (NULL, " +
				"\"" + Path.GetFileName(path) + "\", " +
				"\"" + checksum + "\", " +
				size.ToString() + ", " +
				modified.Ticks.ToString() + ", " +
				"\"" + archiveId + "\", " + 
				parent.ToString() + ", " +
				DateTime.UtcNow.Ticks.ToString() + ");";
			
			return insertFile.ExecuteNonQuery();
		}
		
		public List<FDArchiveDirectory> GetDirectories ( FDArchiveDirectory parentDir )
		{
			Int64 parent = parentDir.parent;
			IDbCommand getDirectories = dbcon.CreateCommand();
			List<FDArchiveDirectory> directories = new List<FDArchiveDirectory>();
			
			getDirectories.CommandText = 
				"SELECT * from `directory` WHERE `directory_parent` = " + parent.ToString() + ";";
			
			IDataReader reader = getDirectories.ExecuteReader();
			
			while (reader.Read ())
			{
				FDArchiveDirectory directory = new FDArchiveDirectory(
					reader.GetInt64 (0), //id
					reader.GetString (1), //directoryname
					reader.GetInt64 (2) //parent
				);
				
				directories.Add(directory);
			}
			
			return directories;
		}
		
		//gets all items given a parent directory
		public List<FDArchiveItem> GetItems ( FDArchiveDirectory parentDir )
		{
			Int64 parent = parentDir.parent;
			IDbCommand getItems = dbcon.CreateCommand();
			List<FDArchiveItem> items = new List<FDArchiveItem>();
			
			getItems.CommandText = 
				"SELECT * from `file` WHERE `file_directory` = " + parent.ToString() + ";";
			
			IDataReader reader = getItems.ExecuteReader();
			
			while(reader.Read ())
			{
				FDArchiveItem item = new FDArchiveItem(
					reader.GetInt64 (0), //id
					reader.GetString (1), //filename
					reader.GetString (2), //checksum
					reader.GetString (5), //archive id
					DateTime.FromBinary(reader.GetInt64 (4)), //modified time
					DateTime.FromBinary(reader.GetInt64 (7)), //upload time
					reader.GetInt64 (6) //parent
				);
				
				items.Add(item);
			}
			
			return items;		
		}
		
		public string GetFileName(string archiveID)
		{
			IDbCommand getFileName = dbcon.CreateCommand();
			getFileName.CommandText = "SELECT `file_name` FROM `file` where `file_archiveid` = \"" + archiveID + "\"";
			
			return (string)getFileName.ExecuteScalar();
		}
		
		public Int64 GetFileParent(string archiveID)
		{
			IDbCommand getFileParent = dbcon.CreateCommand();
			getFileParent.CommandText = "SELECT `file_directory` FROM `file` where `file_archiveid` = \"" + archiveID + "\"";
			
			return (Int64)getFileParent.ExecuteScalar();
		}
		
		//gets the full path of a directory, based on the directory's id
		public string GetFullPath(Int64 directoryID)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			List<string> directories = new List<string>();
			
			while(directoryID != 0)
			{
				IDbCommand getDirectoryName = dbcon.CreateCommand();
				getDirectoryName.CommandText = "SELECT `directory_name`,`directory_parent` FROM `directory` where `directory_id` = " + directoryID.ToString();
				
				IDataReader reader = getDirectoryName.ExecuteReader();
				while(reader.Read ())
				{
					directories.Add (reader.GetString(0));
					directoryID = reader.GetInt64(1);
				}
			}
			
			directories.Reverse();
			sb.Append(System.IO.Path.DirectorySeparatorChar);
			foreach (string directory in directories)
			{
				sb.Append(directory + System.IO.Path.DirectorySeparatorChar);
			}
			
			return sb.ToString();
		}
		
		public int StoreQueue(FDQueueItem item) {
			return 0;
		}
		
		//stores a message to the log table
		public int StoreLogMessage(FDLogVerbosity importance, string component, string smessage, string lmessage, DateTime timestamp)
		{
			int im = (int)importance; //cast to int for storage
			IDbCommand logInsert = dbcon.CreateCommand();
			
			logInsert.CommandText = 
				"INSERT INTO log VALUES (NULL," +
				im.ToString() + "," +
				"\"" + component + "\"," +
				"\"" + smessage + "\"," +
				"\"" + lmessage + "\"," +
				timestamp.ToBinary().ToString() + ");";
			
			return logInsert.ExecuteNonQuery();
		}
		
		private Int64 insertDirectoryTree(string[] directories)
		{
			Int64 lastParent = 0; //start at zero for parent
			
			//iterate over directory array
			for (int i = 1; i < directories.Length; i++)
			{
				IDbCommand checkDirectoryExists = dbcon.CreateCommand();
				
				checkDirectoryExists.CommandText = 
					"SELECT `directory_id` from `directory` WHERE `directory_name` = \"" + directories[i] + "\" " +
					"AND `directory_parent` = " + lastParent.ToString() + " LIMIT 1;";
				
				IDataReader reader = checkDirectoryExists.ExecuteReader();
				
				if(reader.Read()) //we found that directory, so don't create it
				{
					lastParent = reader.GetInt64(0);
				}
				else //we didn't find the directory, so insert it
				{
					lastParent = _insertDirectory(directories[i], lastParent);
				}
			}
			
			return lastParent;
		}
		
		private Int64 _insertDirectory(string directoryName, Int64 parent)
		{
			IDbCommand insertDirectory = dbcon.CreateCommand();
			
			insertDirectory.CommandText = 
				"INSERT INTO `directory` VALUES (NULL, \"" + directoryName + "\", " + parent.ToString() + ");";
			insertDirectory.ExecuteNonQuery();
			
			//TODO there must be a better/safer way to get last insert id
			IDbCommand lastParent = dbcon.CreateCommand();
			lastParent.CommandText = "SELECT max(directory_id) from `directory`;";
			
			return (Int64)lastParent.ExecuteScalar();
		}
	}
}

