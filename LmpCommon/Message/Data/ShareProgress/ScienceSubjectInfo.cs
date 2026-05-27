using Lidgren.Network;
using LmpCommon.Message.Base;
using System;

namespace LmpCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Wrapper for transmitting the ksp ScienceSubject objects (science experiments).
    /// When IsDelta is true only the numeric fields are transmitted (~50 bytes).
    /// When IsDelta is false the full KSP ConfigNode binary is transmitted for first discovery.
    /// </summary>
    public class ScienceSubjectInfo
    {
        public string Id;

        // Numeric fields — always present (used for merge logic and delta updates)
        public float Science;
        public float ScienceCap;
        public float DataScale;
        public float ScientificValue;
        public float SubjectValue;

        // Metadata
        public bool WasTransmitted;
        public string CollectedBy;

        // Delta flag: when true, skip the heavy ConfigNode payload
        public bool IsDelta;

        // Full payload — only populated when IsDelta == false
        public int NumBytes;
        public byte[] Data = new byte[0];

        public ScienceSubjectInfo() { }

        public ScienceSubjectInfo(ScienceSubjectInfo copyFrom)
        {
            Id = copyFrom.Id;
            Science = copyFrom.Science;
            ScienceCap = copyFrom.ScienceCap;
            DataScale = copyFrom.DataScale;
            ScientificValue = copyFrom.ScientificValue;
            SubjectValue = copyFrom.SubjectValue;
            WasTransmitted = copyFrom.WasTransmitted;
            CollectedBy = copyFrom.CollectedBy;
            IsDelta = copyFrom.IsDelta;
            NumBytes = copyFrom.NumBytes;
            if (!IsDelta)
            {
                if (Data.Length < NumBytes)
                    Data = new byte[NumBytes];
                Array.Copy(copyFrom.Data, Data, NumBytes);
            }
        }

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(Id);
            lidgrenMsg.Write(Science);
            lidgrenMsg.Write(ScienceCap);
            lidgrenMsg.Write(DataScale);
            lidgrenMsg.Write(ScientificValue);
            lidgrenMsg.Write(SubjectValue);
            lidgrenMsg.Write(WasTransmitted);
            lidgrenMsg.Write(CollectedBy ?? string.Empty);
            lidgrenMsg.Write(IsDelta);
            if (!IsDelta)
            {
                lidgrenMsg.Write(NumBytes);
                lidgrenMsg.Write(Data, 0, NumBytes);
            }
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            Id = lidgrenMsg.ReadString();
            Science = lidgrenMsg.ReadFloat();
            ScienceCap = lidgrenMsg.ReadFloat();
            DataScale = lidgrenMsg.ReadFloat();
            ScientificValue = lidgrenMsg.ReadFloat();
            SubjectValue = lidgrenMsg.ReadFloat();
            WasTransmitted = lidgrenMsg.ReadBoolean();
            lidgrenMsg.SkipPadBits();
            CollectedBy = lidgrenMsg.ReadString();
            IsDelta = lidgrenMsg.ReadBoolean();
            lidgrenMsg.SkipPadBits();
            if (!IsDelta)
            {
                NumBytes = lidgrenMsg.ReadInt32();
                if (Data.Length < NumBytes)
                    Data = new byte[NumBytes];
                lidgrenMsg.ReadBytes(Data, 0, NumBytes);
            }
        }

        public int GetByteCount()
        {
            var size = Id.GetByteCount()
                + sizeof(float) * 5
                + 2 * sizeof(byte)  // WasTransmitted + IsDelta (1 bit each, padded to byte boundary)
                + (CollectedBy ?? string.Empty).GetByteCount();
            if (!IsDelta)
                size += sizeof(int) + sizeof(byte) * NumBytes;
            return size;
        }
    }
}
