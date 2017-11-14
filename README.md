<p align="center">
    <img src="../master/External/logo.png" alt="Luna multiplayer logo" height="250" width="250"/>
</p>

<p align="center">
  <a href="https://discord.gg/5szq2r"><img src="https://img.shields.io/discord/378456662392045571.svg" alt="Chat on discord"/></a>
  <a href="https://paypal.me/gavazquez"><img src="https://img.shields.io/badge/paypal-donate-yellow.svg" alt="PayPal"/></a>
  <a href="../../wiki"><img src="https://img.shields.io/badge/documentation-Wiki-4BC51D.svg?style=flat" alt="Documentation" /></a>
</p>

---

# Luna Multiplayer Mod (LMP)

*Multiplayer mod for [Kerbal Space Program (KSP)](https://kerbalspaceprogram.com)*

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

Please check the [wiki](https://github.com/gavazquez/LunaMultiPlayer/wiki) to see how to [build](https://github.com/gavazquez/LunaMultiPlayer/wiki/How-to-compile-LMP), [run](https://github.com/gavazquez/LunaMultiPlayer/wiki/How-to-run-LMP) or [debug](https://github.com/gavazquez/LunaMultiPlayer/wiki/Debugging-in-Visual-studio) LMP

---

## Status:

|            |   Build  |   Tests  |  Last commit  |  Release  |
| ---------- | -------- | -------- | ------------- | ------------- |
| **master** branch |[![AppVeyor](https://img.shields.io/appveyor/ci/gavazquez/lunamultiplayer/master.svg?logo=appveyor)](https://ci.appveyor.com/project/gavazquez/lunamultiplayer/branch/master) | [![AppVeyor Tests](https://img.shields.io/appveyor/tests/gavazquez/lunamultiplayer.svg?logo=appveyor)](https://ci.appveyor.com/project/gavazquez/lunamultiplayer/build/tests) | [![GitHub last commit](https://img.shields.io/github/last-commit/gavazquez/lunamultiplayer/master.svg)]() | [![GitHub release](https://img.shields.io/github/release/gavazquez/lunamultiplayer.svg)]()
---

##### Acknowledgements:

*LMP in it's origin was based on the [DMP](https://github.com/godarklight/DarkMultiPlayer) and the latter is based on the inactive [KMP](https://github.com/TehGimp/KerbalMultiPlayer) mod*

---
<p align="center">
  <a href="mailto:gavazquez@gmail.com">
    <img src="https://img.shields.io/badge/email-gavazquez@gmail.com-blue.svg?style=flat" alt="Email: gavazquez@gmail.com" />
  </a>
  <a href="https://raw.githubusercontent.com/gavazquez/LunaMultiPlayer/master/LICENSE">
    <img src="https://img.shields.io/github/license/gavazquez/LunaMultiPlayer.svg" alt="License" />
  </a>
</p> 
