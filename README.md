# ActiveRecordInspector
Search directory which contain class libraries or single class library for classes that contain ActiveRecord based classes and list them in tree or list form. Get each class description in form suitable for quick review or documentation making.

To succesfully compile solution you'll need Visual Studio 2010 (or compatible) and Castle.ActiveRecord.dll that must be placed into libs directory. Solution targets .NET 3.5

Once succesfully compiled, execute ActiveRecordInspector.exe with path or filename as command line argument or just execute ActiveRecordInspector.exe and then select path or filename from menu. We assume that path is a directory with libraries that contain ActiveRecord classes and filename is a single library which contain ActiveRecord classes.

However you select what to inspect, once you select a valid path or filename, inspection of class libraries will start. Once finished, result will be immediately visible in user interface and errors (if any) will be visible from menu.
