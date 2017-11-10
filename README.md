# Luna Multiplayer Mod (LMP)

*Multiplayer mod for [Kerbal Space Program (KSP)](https://kerbalspaceprogram.com)*

<a href="https://discord.gg/S6bQR5q"><img src="https://img.shields.io/badge/chat-on%20discord-7289da.svg" alt="chat on Discord"></a>
<a href="https://paypal.me/gavazquez"><img src="https://img.shields.io/badge/paypal-donate-yellow.svg" alt="PayPal donate button"/></a>
---

Main features:

- Clean and optimized code, based on systems and windows which makes it easier to read and modify.
- Multi threaded (as much as Unity allows)
- Better usage of coroutines.
- Settings saved as XML.
- UDP based using the [Lidgren](https://github.com/lidgren/lidgren-network-gen3) library for reliable UDP message handling.
- Uses interpolation so the vessels shouldn't jump from one place to another.
- Nat-punchtrough feature so a server doesn't need to open ports on it's router.
- Servers are displayed within the application.
- Better creation of network messages so they are easier to modify as you don't need to take care of serialization.
- Every network message is cached in order to reduce the garbage collector spikes
- Based on tasks instead of threads.
- Improved message compression algorithm ([QuickLZ](http://www.quicklz.com))

---

## [Wiki](https://github.com/gavazquez/LunaMultiPlayer/wiki)

#### Interesting  links:

[How to build LMP](https://github.com/gavazquez/LunaMultiPlayer/wiki/How-to-compile-LMP)  
[How to run LMP](https://github.com/gavazquez/LunaMultiPlayer/wiki/How-to-run-LMP)  
[How to debug LMP](https://github.com/gavazquez/LunaMultiPlayer/wiki/Debugging-in-Visual-studio)  

---

## Status:

|            |   AppVeyor Build  |
| ---------- | ----------------- |
| **master** branch |[![AppVeyor Build](https://ci.appveyor.com/api/projects/status/mf00yf1j560lfm8f/branch/master?svg=true)](https://ci.appveyor.com/project/gavazquez/lunamultiplayer/branch/master)

---

#### Acknowledgements:

LMP in it's origin was based on the [DMP](https://github.com/godarklight/DarkMultiPlayer) and the latter is based on the inactive [KMP](https://github.com/TehGimp/KerbalMultiPlayer) mod
