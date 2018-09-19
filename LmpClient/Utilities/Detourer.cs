using System;
using System.Collections.Generic;
using System.Reflection;

namespace LmpClient.Utilities
{
    /// <summary>
    /// This class detours a KSP method to your own implementation
    /// </summary>
    public static class Detourer
    {
        private static readonly Dictionary<string,string> Detours = new Dictionary<string, string>();

        /// <summary>
        /// Taken from https://github.com/RimWorldCCLTeam/CommunityCoreLibrary
        /// Detours one method to another one
        /// </summary>
        public static bool TryDetourFromTo(MethodInfo source, MethodInfo destination)
        {
            if (source == null || destination == null)
            {
                LunaLog.LogError("[Detour] Source/Destination MethodInfo cannot be null");
                return false;
            }

            if (!CheckDetouring(source, destination))
                return false;

            if (IntPtr.Size == sizeof(long))
            {
                Create64BitsDetour(source, destination);
            }
            else
            {
                Create32BitsDetour(source, destination);
            }
            
            return true;
        }
        
        private static bool CheckDetouring(MethodInfo source, MethodInfo destination)
        {
            var sourceStr = source.DeclaringType?.FullName + "." + source.Name + " @ 0x" +
                            source.MethodHandle.GetFunctionPointer().ToString("X" + IntPtr.Size * 2);

            var destStr = destination.DeclaringType?.FullName + "." + destination.Name + " @ 0x" +
                          destination.MethodHandle.GetFunctionPointer().ToString("X" + IntPtr.Size * 2);

            if (Detours.ContainsKey(sourceStr))
            {
                //Othwerise we are just detouring to the same method...
                if (destStr != Detours[sourceStr])
                {
                    LunaLog.LogWarning($"[Detour] Source method('{sourceStr}') was previously detoured to '{Detours[sourceStr]}'");
                }
                
                return false;
            }

            Detours.Add(sourceStr, destStr);
            LunaLog.Log($"[Detour] Detouring '{sourceStr}' to '{destStr}'");

            return true;
        }

        private static unsafe void Create32BitsDetour(MethodInfo source, MethodInfo destination)
        {
            // 32-bit systems use 32-bit relative offset and jump
            // 5 byte destructive

            // Get function pointers
            var sourceBase = source.MethodHandle.GetFunctionPointer().ToInt32();
            var destinationBase = destination.MethodHandle.GetFunctionPointer().ToInt32();
            
            // Native source address
            var pointerRawSource = (byte*) sourceBase;

            // Pointer to insert jump address into native code
            var pointerRawAddress = (int*) (pointerRawSource + 1);

            // Jump offset (less instruction size)
            var offset = (destinationBase - sourceBase) - 5;

            // Insert 32-bit relative jump into native code
            *pointerRawSource = 0xE9;
            *pointerRawAddress = offset;
        }

        private static unsafe void Create64BitsDetour(MethodInfo source, MethodInfo destination)
        {
            // 64-bit systems use 64-bit absolute address and jumps
            // 12 byte destructive

            // Get function pointers
            var sourceBase = source.MethodHandle.GetFunctionPointer().ToInt64();
            var destinationBase = destination.MethodHandle.GetFunctionPointer().ToInt64();

            // Native source address
            var pointerRawSource = (byte*) sourceBase;

            // Pointer to insert jump address into native code
            var pointerRawAddress = (long*) (pointerRawSource + 0x02);

            // Insert 64-bit absolute jump into native code (address in rax)
            // mov rax, immediate64
            // jmp [rax]
            *(pointerRawSource + 0x00) = 0x48;
            *(pointerRawSource + 0x01) = 0xB8;
            *pointerRawAddress = destinationBase; // ( Pointer_Raw_Source + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
            *(pointerRawSource + 0x0A) = 0xFF;
            *(pointerRawSource + 0x0B) = 0xE0;
        }
    }
}
