(This is still being updated and verified - Wiki to be updated later this week)
# JRNViewer
VB.Net Utility Program to easily view AS/400 Journals
## Brief Description
Anyone who has been tasked with going in and reviewing AS/400 journals knows what a glorious pain in the ass it can be.  So during one assignment, I had a team of Data Warehouse folks that needed to eaily verify all the journal records on the AS/400 were being sent.

To accomplish this, this tool encapsulates all of the steps required to view AS/400 journal entries and presents it in a viewable window.  All the user has to do is select the table being journaled.
## Requisites
### AS/400 Requirements
1. User must have sufficient authority to view journal files
2. User must have command line authority.  
3. User must have SQL Authority.  The actual commands are executed via SQL
### Desktop Requirements
1. You must have iAccess (or whatever IBM calls it these days) with ODBC drivers installed.
## Quick Start
When you start up - the application will ask for the address of the AS/400 and your AS/400 credentials.  Once you provide that and are logged in, then you can drive to the library where the tables & journals are stored.
## Future Thoughts
As a number of AS/400s these days have the ZEND engine installed - it occurred to me that a likely better solution would have been to build this as as PHP site hosted on the AS/400.  So if you're ambitious and talented - I'd be interested to see this interpreted as a PHP site.  
