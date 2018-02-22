using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.Vessel
{
    public class PartSync
    {
        public uint PartFlightId;

        public int NumModules;
        public ModuleSync[] Modules = new ModuleSync[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(PartFlightId);
            lidgrenMsg.Write(NumModules);
            for (var i = 0; i < NumModules; i++)
            {
                Modules[i].Serialize(lidgrenMsg);
            }
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            PartFlightId = lidgrenMsg.ReadUInt32();
            if (Modules.Length < NumModules)
                Modules = new ModuleSync[NumModules];

            for (var i = 0; i < NumModules; i++)
            {
                if (Modules[i] == null)
                    Modules[i] = new ModuleSync();

                Modules[i].Deserialize(lidgrenMsg);
            }
        }

        public int GetByteCount()
        {
            var arraySize = 0;
            for (var i = 0; i < NumModules; i++)
            {
                arraySize += Modules[i].GetByteCount();
            }

            return sizeof(uint) + sizeof(int) + arraySize;
        }
    }

    public class ModuleSync
    {
        public string ModuleName;

        public int NumFields;
        public ModuleField[] ModuleFields = new ModuleField[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(NumFields);
            for (var i = 0; i < NumFields; i++)
            {
                ModuleFields[i].Serialize(lidgrenMsg);
            }
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            NumFields = lidgrenMsg.ReadInt32();
            if (ModuleFields.Length < NumFields)
                ModuleFields = new ModuleField[NumFields];

            for (var i = 0; i < NumFields; i++)
            {
                if (ModuleFields[i] == null)
                    ModuleFields[i] = new ModuleField();

                ModuleFields[i].Deserialize(lidgrenMsg);
            }
        }

        public int GetByteCount()
        {
            var arraySize = 0;
            for (var i = 0; i < NumFields; i++)
            {
                arraySize += ModuleFields[i].GetByteCount();
            }

            return sizeof(int) + arraySize;
        }
    }

    public class ModuleField
    {
        public string FieldName;
        public string Value;

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(FieldName);
            lidgrenMsg.Write(Value);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            FieldName = lidgrenMsg.ReadString();
            Value = lidgrenMsg.ReadString();
        }

        public int GetByteCount()
        {
            return FieldName.GetByteCount() + Value.GetByteCount();
        }
    }
}
