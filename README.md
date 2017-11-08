# Luna Multiplayer Mod (LMP)  <a href="https://ci.appveyor.com/project/gavazquez/lunamultiplayer"><img src="https://ci.appveyor.com/api/projects/status/mf00yf1j560lfm8f?svg=true" alt="build status"></a>

*Multiplayer mod for [KSP](https://kerbalspaceprogram.com) based on [DMP](https://github.com/godarklight/DarkMultiPlayer)*  

---

Main features:
- Clean and optimized code, based on systems and windows which makes it easier to read and modify.
- Every message is cached in order to reduce the garbage collector spikes
- Multi threaded (as much as Unity allows)
- Better usage of coroutines.
- Settings saved as XML.
- UDP based using the [Lidgren](https://github.com/lidgren/lidgren-network-gen3) library for reliable UDP message handling.
- Uses interpolation so the vessels shouldn't jump from one place to another.
- Nat-punchtrough feature so a server doesn't need to open ports on it's router.
- Servers are displayed within the application.
- Better creation of network messages so they are easier to modify as you don't need to take care of serialization.
- Based on tasks instead of threads.
- Improved message compression algorithm ([QuickLZ](http://www.quicklz.com))

---
## [Wiki](https://github.com/gavazquez/LunaMultiPlayer/wiki)

#### Interesting  links:

[How to build LMP](https://github.com/gavazquez/LunaMultiPlayer/wiki/How-to-compile-LMP)  
[How to run LMP](https://github.com/gavazquez/LunaMultiPlayer/wiki/How-to-run-LMP)  
[How to debug LMP](https://github.com/gavazquez/LunaMultiPlayer/wiki/Debugging-in-Visual-studio)  

---

Build Server: https://ci.appveyor.com/project/gavazquez/lunamultiplayer  
IRC: #LMP on EsperNet http://webchat.esper.net/?channels=LMP
