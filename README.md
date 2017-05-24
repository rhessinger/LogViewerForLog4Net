*Project Description*
LogViewer is a WPF application allowing to view Log4Net log files.Logs must be in Xml (just need a few lines in your application config file to configure it to use this format).Search, log merging, filtering are already supported but the project is open for new enhancements.

[image:LogViewer.png]

*Setup*
Download latest binaries, unzip in any folder. Launch LogViewer.exe. That's all.To be able to manage your Log4Net logs with this application you must modify the way logs are formated. Open the LogViewer About Box and you'll see a little code sample that you must copy within your app.config file. Once it is done, your application will generate XML log files that can be read by LogViewer. Of course you can add this Log4Net configuration beside the existing one if any. For more information about Log4Net XML output please read the Log4Net documentation ([url:http://logging.apache.org/log4net/index.html]).

*History*
First version has been created by Ken C Len and published on CodeProject in 2009.CodePlex version is an enhanced release by Olivier Dahan, MVP CAD 2010, MVP C# 2009.
This software is under .NET 4.0 and WPF. Project can be edited using VS2010 and Blend 4.

*License*
I'm publishing this enhanced version of the original code with the permission of original author. This particular release is under the Ms-RL license.

*Future*
Any contribution to this project will be appreciate. There are many things that can be done to make this usefull tool even better. Feel free to contact me to be involved in next releases !
To build the project you'll need to reference WPF.Themes DLL. This is also a CodePlex project : [http://wpfthemes.codeplex.com/]To build the project you'll need to reference WPF.Themes DLL. This is also a CodePlex project : [http://wpfthemes.codeplex.com/]
