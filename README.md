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

####After compiling, adjust the CopyToKSPDirectory.bat file with the correct KSP directory and run it.

If you want to know how to debug the code, read the .txt in ExternalLibraries\HowToDebugLMP
