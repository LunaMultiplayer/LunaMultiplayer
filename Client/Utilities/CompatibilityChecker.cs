/**
 * Copyright (c) 2014, Majiir
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, are permitted 
 * provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this list of 
 * conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright notice, this list of 
 * conditions and the following disclaimer in the documentation and/or other materials provided 
 * with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR 
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

// ReSharper disable All

/*-----------------------------------------*\
|   SUBSTITUTE YOUR MOD'S NAMESPACE HERE.   |
\*-----------------------------------------*/

namespace LunaClient.Utilities
{
    /**
                 * This utility displays a warning with a list of mods that determine themselves
                 * to be incompatible with the current running version of Kerbal Space Program.
                 * 
                 * See this forum thread for details:
                 * http://forum.kerbalspaceprogram.com/threads/65395-Voluntarily-Locking-Plugins-to-a-Particular-KSP-Version
                 */

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    internal class CompatibilityChecker : MonoBehaviour
    {
        // Version of the compatibility checker itself.
        private static int _version = 3;

        public static bool IsCompatible()
        {
            const int compatibleMajor = 1;
            const int compatibleMinor = 3;
            const int compatibleRevision = 1;
            return (Versioning.version_major == compatibleMajor) && (Versioning.version_minor == compatibleMinor) && (Versioning.Revision == compatibleRevision);

            /*-----------------------------------------------*\
            | IMPLEMENTERS SHOULD NOT EDIT BEYOND THIS POINT! |
            \*-----------------------------------------------*/
        }

        public static bool IsUnityCompatible()
        {
            /*-----------------------------------------------*\
            |    BEGIN IMPLEMENTATION-SPECIFIC EDITS HERE.    |
            \*-----------------------------------------------*/

            return Application.unityVersion == "5.4.0p4";

            /*-----------------------------------------------*\
            | IMPLEMENTERS SHOULD NOT EDIT BEYOND THIS POINT! |
            \*-----------------------------------------------*/
        }

        public void Start()
        {
            // Checkers are identified by the type Name and version field Name.
            var fields = GetAllTypes()
                    .Where(t => t.Name == "CompatibilityChecker")
                    .Select(t => t.GetField("_version", BindingFlags.Static | BindingFlags.NonPublic))
                    .Where(f => f != null)
                    .Where(f => f.FieldType == typeof(int));

            // Let the latest version of the checker execute.
            if (_version != fields.Max(f => (int)f.GetValue(null))) return;

            LunaLog.Log($"[CompatibilityChecker] Running checker version {_version} from '{Assembly.GetExecutingAssembly().GetName().Name}'");

            // Other checkers will see this version and not run.
            // This accomplishes the same as an explicit "ran" flag with fewer moving parts.
            _version = int.MaxValue;

            // A mod is incompatible if its compatibility checker has an IsCompatible method which returns false.
            var incompatible =
                fields
                    .Select(f => f.DeclaringType.GetMethod("IsCompatible", Type.EmptyTypes))
                    .Where(m => m.IsStatic)
                    .Where(m => m.ReturnType == typeof(bool))
                    .Where(m =>
                    {
                        try
                        {
                            return !(bool)m.Invoke(null, new object[0]);
                        }
                        catch (Exception e)
                        {
                            // If a mod throws an exception from IsCompatible, it's not compatible.
                            LunaLog.LogWarning($"[CompatibilityChecker] Exception while invoking IsCompatible() from '{m.DeclaringType?.Assembly.GetName().Name}':\n\n{e}");
                            return true;
                        }
                    })
                    .Select(m => m.DeclaringType?.Assembly.GetName().Name)
                    .ToArray();

            // A mod is incompatible with Unity if its compatibility checker has an IsUnityCompatible method which returns false.
            var incompatibleUnity =
                fields
                    .Select(f => f.DeclaringType.GetMethod("IsUnityCompatible", Type.EmptyTypes))
                    .Where(m => m != null) // Mods without IsUnityCompatible() are assumed to be compatible.
                    .Where(m => m.IsStatic)
                    .Where(m => m.ReturnType == typeof(bool))
                    .Where(m =>
                    {
                        try
                        {
                            return !(bool)m.Invoke(null, new object[0]);
                        }
                        catch (Exception e)
                        {
                            // If a mod throws an exception from IsUnityCompatible, it's not compatible.
                            LunaLog.LogWarning($"[CompatibilityChecker] Exception while invoking IsUnityCompatible() from '{m.DeclaringType?.Assembly.GetName().Name}':\n\n{e}");
                            return true;
                        }
                    })
                    .Select(m => m.DeclaringType?.Assembly.GetName().Name)
                    .ToArray();

            Array.Sort(incompatible);
            Array.Sort(incompatibleUnity);

            var message =
                "Some installed mods may be incompatible with this version of Kerbal Space Program. Features may be broken or disabled. Please check for updates to the listed mods.";

            if (incompatible.Length > 0)
            {
                LunaLog.LogWarning($"[CompatibilityChecker] Incompatible mods detected: {string.Join(", ", incompatible)}");
                message += $"\n\nThese mods are incompatible with KSP {Versioning.version_major}.{Versioning.version_minor}.{Versioning.Revision}:\n\n";
                message += string.Join("\n", incompatible);
            }

            if (incompatibleUnity.Length > 0)
            {
                LunaLog.LogWarning("[CompatibilityChecker] Incompatible mods (Unity) detected: " +
                                 string.Join(", ", incompatibleUnity));
                message += $"\n\nThese mods are incompatible with Unity {Application.unityVersion}:\n\n";
                message += string.Join("\n", incompatibleUnity);
            }

            if ((incompatible.Length > 0) || (incompatibleUnity.Length > 0))
                PopupDialog.SpawnPopupDialog(new MultiOptionDialog("CompatibilityChecker", message, "Incompatible Mods Detected", HighLogic.UISkin), true, HighLogic.UISkin);
        }

        private static IEnumerable<Type> GetAllTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (Exception)
                {
                    types = Type.EmptyTypes;
                }

                foreach (var type in types)
                    yield return type;
            }
        }
    }
}