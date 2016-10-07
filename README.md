# ActiveRecordInspector
Search directory which contain class libraries or single class library for classes that contain [ActiveRecord](http://www.castleproject.org/projects/activerecord/) based classes and list them in tree or list form. Get each class description in form suitable for quick review or documentation making.

To quickly start to use it, get latest [ActiveRecordInspector.zip](https://github.com/isindicic/ActiveRecordInspector/releases/download/1.0/ActiveRecordInspector.zip) and unzip it somewhere. 
Simply execute ActiveRecordInspector.exe with path or filename as command line argument or just execute ActiveRecordInspector.exe from explorer and then select path or filename from menu. 
We assume that "path" is a directory with one or more class libraries that contain ActiveRecord classes and "filename" is a single library which contain ActiveRecord classes.

Whatever you  select what to inspect, once you finish with that, inspection of class libraries will start. Once finished, result will be immediately visible in user interface and errors (if any) will be visible from menu.

To successfully compile solution you'll need Visual Studio 2010 (or compatible) and two Castle.ActiveRecord dlls that must be placed into libs directory (for more info check readme.txt from libs directory). 

Solution targets .NET 4.0 and application require such framework version installed in order to run. However, application can inspect libraries that target lower version of framework without any problems.

