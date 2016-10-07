using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data
{
    public class ModMsgData : MessageData
    {
        /// <summary>
        /// Name of the mod that creates this msg
        /// </summary>
        public string ModName { get; set; }
        /// <summary>
        /// Relay the msg to all players once it arrives to the serer
        /// </summary>
        public bool Relay { get; set; }
        /// <summary>
        /// Send it in reliable mode or in UDP-unreliable mode
        /// </summary>
        public bool Reliable { get; set; }
        /// <summary>
        /// Data to send
        /// </summary>
        public byte[] Data { get; set; }
    }
}