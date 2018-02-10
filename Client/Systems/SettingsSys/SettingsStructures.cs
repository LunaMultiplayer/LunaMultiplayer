using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.Toolbar;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Systems.SettingsSys
{
    [Serializable]
    public class SettingStructure
    {
        public string PlayerName { get; set; } = "Player";
        public int ConnectionTries { get; set; } = 3;
        public int InitialConnectionMsTimeout { get; set; } = 5000;
        public int SendReceiveMsInterval { get; set; } = 5;
        public int MsBetweenConnectionTries { get; set; } = 3000;
        public int HearbeatMsInterval { get; set; } = 2000;
        public bool DisclaimerAccepted { get; set; } = false;
        public Color PlayerColor { get; set; } = PlayerColorSystem.GenerateRandomColor();
        public KeyCode ChatKey { get; set; } = KeyCode.BackQuote;
        public string SelectedFlag { get; set; } = "Squad/Flags/default";
        public LmpToolbarType ToolbarType { get; set; } = LmpToolbarType.BlizzyIfInstalled;
        public List<ServerEntry> Servers { get; set; } = new List<ServerEntry>();
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public int InitialConnectionSyncTimeRequests { get; set; } = 10;
        public bool RevertEnabled { get; set; }
        public bool InterpolationEnabled { get; set; } = false;
        public bool CloseBtnInConnectionWindow { get; set; } = true;
        public int MaxGroupsPerPlayer { get; set; } = 1;
        public int PositionSystem { get; set; } = 2;

#if DEBUG

        /*
         * You can use this debug switches for testing purposes. 
         * For example do one part or the code or another in case the debugX is on/off
         * NEVER upload the code with those switches in use as some other developer might need them!!!!!
         */

        public bool Debug1 { get; set; } = false;
        public bool Debug2 { get; set; } = false;
        public bool Debug3 { get; set; } = false;
        public bool Debug4 { get; set; } = false;
        public bool Debug5 { get; set; } = false;
        public bool Debug6 { get; set; } = false;
        public bool Debug7 { get; set; } = false;
        public bool Debug8 { get; set; } = false;
        public bool Debug9 { get; set; } = false;

#endif

    }

    [Serializable]
    public class ServerEntry
    {
        public int Port;
        public string Name { get; set; }
        public string Address { get; set; }
    }
}