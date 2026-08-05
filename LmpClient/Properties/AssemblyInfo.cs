using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Luna Multiplayer Mod")]
[assembly: AssemblyDescription("Luna Multiplayer Mod (client)")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("LMP")]
[assembly: AssemblyCopyright("Copyright © 2018")]
[assembly: AssemblyTrademark("Gabriel Vazquez")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("cc8e38bb-d6d5-4bb9-ab74-a3a1a11ddc8d")]

[assembly: AssemblyVersion("0.30.0")]
[assembly: AssemblyFileVersion("0.30.0")]
[assembly: AssemblyInformationalVersion("0.30.0-compiled")]

// Type forwarders that keep older mods binding LmpCommon types via LmpClient live in
// TypeForwarding.cs (single source to avoid duplicate CS0739 forwarder declarations).

[assembly: KSPAssembly("LMP", 0, 30)]