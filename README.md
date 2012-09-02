snowpack
========

A Graphical Client for Amazon Glacier, written in C# with the Mono Framework  
&copy; 2012 Far Dog LLC  
Licensed under [GPLv3](http://www.gnu.org/licenses/gpl-3.0.txt)

Intro
-----

**snowpack** is (someday) a complete GUI client and backup solution for
Amazon Glacier. Currently, it is a very early test while I bring my C# skills
back up to current. Please don't use it until then, you will be disappointed.

Status
------

Right now, snowpack can store any file to Glacier, and stores the ID of any
archived file to an SQLite DB for later retrieval. It stores full directory
structure, and will include a tree browser for requesting restores. There is
a basic de-duplication process in place, where file checksums and sizes are
stored, and a file is considered to be a duplicate if those two items match
a previously stored file. The file's location and other info are still stored
in the database as though it was uploaded.

Now that I feel like this is a successful test, the current areas of work are:

 - Creating an upload queue *(done)*
 - Separating upload code from interface code *(done)*
 - Adding support for recursive/directory uploads *(done)*
 - Proper error recovery (upload fails, etc.)
 - More thorough upload queue management, save on quit
 - GUI managed preferences for AWS keys and etc.
 - Adding a backup archive browser, and the ability to make file requests

The future will be:

 - Moving things to a client/server architecture with a background daemon that
   performs uploads and reads a queue and a client that manages that queue and
   checks progress.
 - Create backups of file index/SQLite database to Glacier
 - Retrieving vault list from AWS
 - Performing automated restores using Amazon SNS
 

Misc
----

This requires an App.config file with the following properties in the
*appSettings* section:

 -	*AWSAccessKey*
 -	*AWSSecretKey*
 -	*AWSRegion* (e.g. "us-west-2", "us-east-1", etc.)
 -	*GlacierVaultName*

This will be moved to a easily configurable interface in the future.


Warranty
--------

Copyright 2012 Far Dog LLC or its affiliates. All Rights Reserved.

Licensed under the GNU General Public License, Version 3.0 (the "License").
You may not use this file except in compliance with the License.
A copy of the License is located at

*http://www.gnu.org/licenses/gpl-3.0.txt*

or in the "gpl-3.0" file accompanying this file. This file is distributed
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
express or implied. See the License for the specific language governing
permissions and limitations under the License.