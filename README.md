# Luna Multiplayer Mod (LMP)

*Multiplayer mod for KSP based on DMP*

---

Main features:
- Clean and optimized code, based on systems and windows which makes it easier to read and modify
- Multi threaded (as much as Unity allows)
- Better usage of coroutines
- Settings saved as XML
- UDP based using the Lidgren library for reliable UDP message handling
- Uses interpolation so the vessels shouldn't jump from one place to another
- Nat-punchtrough feature so a server doesn't need to open ports on it's router
- Servers are displayed within the application
- Better creation of UDP messages so they are easier to modify as you don't need to take care of serialization
- Based on Tasks instead of Threads
- Improved message compression algorithm (QuickLZ)

---
## [Main wiki](https://github.com/gavazquez/LunaMultiPlayer/wiki)

### Interesting  links:

[How to build LMP](https://github.com/gavazquez/LunaMultiPlayer/wiki/How-to-compile-LMP)  
[How to run LMP](https://github.com/gavazquez/LunaMultiPlayer/wiki/How-to-run-LMP)  
[How to debug LMP](https://github.com/gavazquez/LunaMultiPlayer/wiki/Debugging-in-Visual-studio)  

---

Build Server: https://ci.appveyor.com/project/DaggerES/lunamultiplayer
