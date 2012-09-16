snowpack
========

A Graphical Client for Amazon Glacier, written in C# with the Mono Framework  
&copy; 2012 Far Dog LLC  
Licensed under [GPLv3](http://www.gnu.org/licenses/gpl-3.0.txt)

Intro
-----

**snowpack** is (someday) a complete GUI client and backup solution for
Amazon Glacier. Right now it's a heavy work-in-progress, but is more capable
all the time.

**snowpack is incomplete, and not recommended for anyone's use at the moment.**
While you'll be able to store files to Glacier, and view _what_ you've stored,
there is no method for retrieving files (or performing a test restore) yet.

Status
------

Right now, snowpack can store any file to Glacier, and stores the ID of any
archived file to an SQLite DB for later retrieval. There is a basic 
de-duplication process in place, where file checksums and sizes are stored, and
a file is considered to be a duplicate if those two items match a previously 
stored file. The file's location and other info are still stored in the 
database as though it was uploaded.

Current areas of work are:

 - File restores and deletions from Glacier
 - Log viewer for tracking any upload failures.
 - More thorough upload queue management, save on quit

The future will be:

 - Moving things to a client/server architecture with a background daemon that
   performs uploads and reads a queue and a client that manages that queue and
   checks progress.
 - Create backups of file index/SQLite database to Glacier
 - Retrieving vault list from AWS
 - Resuming restore jobs if the application is quit
 

Misc
----

snowpack requires AWS credentials and a Glacier vault, and will ask for them
on first run. You can create the necessary credentials and vault using your
AWS console. I'd recommend reading the 
[Getting Started](http://aws.amazon.com/glacier/) guide for Glacier prior to
trying out snowpack.


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