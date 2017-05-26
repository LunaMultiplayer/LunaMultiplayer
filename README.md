# Luna Multiplayer Mod (LMP)

*Multiplayer mod for KSP based on DMP*

---

Main features:
- Better and optimized code, easier to read and modify
- Better usage of coroutines
- UDP based
- Uses interpolation so the vessels shouldn't jump from one place to another
- Uses the Lidgren library for reliable UDP message handling
- Nat-punchtrough feature so a server doesn't need to open ports on it's router
- Servers are displayed within the application
- Better creation of UDP messages so they are easier to modify as you don't need to take care of serialization
- Based on Tasks instead of Threads
- Faster message compression algorithm (QuickLZ)

---


### How to build LMP:
1) Download and install Visual Studio 2017 to write and build the dll. Any version will work.

You can get the community version here:
https://www.visualstudio.com/es/downloads/

2) Download the unity addon.

The tools for unity addin can be found here: 
https://visualstudiogallery.msdn.microsoft.com/8d26236e-4a64-4d64-8486-7df95156aba9

3) Now that you've downloaded the necessary tools, use git to checkout and pull the latest copy of the LunaMultiplayer project.  However, you've probably already done this, since you're reading this document.

4) Go to the main folder and double click the LunaMultiPlayer.sln file, opening it in Visual Studio.

5) Adjust the CopyToKSPDirectory.bat file with the correct KSP Directory.  The line to edit is this one:
SET KSPPATH=C:\games\Steam\SteamApps\common\Kerbal Space Program

6) Copy the Assembly-CSharp.dll file from your Kerbal Installation to External/KSPLibraries.  You MUST repeat this process every time KSP is updated.  This file is located in
the C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program\KSP_x64_Data\Managed folder.

7) Make sure you have Debug mode set in Visual studio at the top.  Do not use Release mode as developers only test with Debug.

8) Compile in visual studio by going to build->rebuild project.

9) Once built, the plugin should automatically be updated in your Kerbal Space Program plugins folder.  You will need to restart Kerbal Space Program, if it's open.


### How to run LMP:
1) To start the server, select "Server" in the dropdown at the top of Visual Studio 2015 for the list of projects.  Then click the start button.

2) When KSP starts, it will give you a menu on the main screen.  Add a server for "localhost", leaving the port unchanged.  Then click the connect button, and you should connect to the server you started in step #1.


### How to debug LMP:
You can learn how to debug the code with visual studio here: 
https://github.com/DaggerES/LunaMultiPlayer/wiki/Debugging-in-VS2017

Build Server: https://ci.appveyor.com/project/DaggerES/lunamultiplayer
