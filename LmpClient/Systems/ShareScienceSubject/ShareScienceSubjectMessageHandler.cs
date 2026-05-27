using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Extensions;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LmpClient.Systems.ShareScienceSubject
{
    public class ShareScienceSubjectMessageHandler : SubSystem<ShareScienceSubjectSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        // Tracks the highest science value we've seen per subject to prevent out-of-order downgrades
        private static readonly ConcurrentDictionary<string, float> _highWaterMark = new ConcurrentDictionary<string, float>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;

            switch (msgData.ShareProgressMessageType)
            {
                case ShareProgressMessageType.ScienceSubjectUpdate:
                    if (msgData is ShareProgressScienceSubjectMsgData updateData)
                    {
                        var copy = new ScienceSubjectInfo(updateData.ScienceSubject);
                        LunaLog.Log($"Queue science subject update: {copy.Id} sci={copy.Science} isDelta={copy.IsDelta}");
                        System.QueueAction(() => ApplyScienceSubjectUpdate(copy));
                    }
                    break;

                case ShareProgressMessageType.ScienceSubjectRevert:
                    if (msgData is ShareProgressScienceSubjectRevertMsgData revertData)
                    {
                        var copies = new ScienceSubjectInfo[revertData.SubjectCount];
                        for (var i = 0; i < revertData.SubjectCount; i++)
                            copies[i] = new ScienceSubjectInfo(revertData.Subjects[i]);
                        LunaLog.Log($"Queue science subject revert: {revertData.SubjectCount} subjects");
                        System.QueueAction(() => ApplyScienceSubjectRevert(copies));
                    }
                    break;
            }
        }

        private static void ApplyScienceSubjectUpdate(ScienceSubjectInfo info)
        {
            // Deduplicate: ignore if we already have a higher value (handles network reorder / duplicates)
            if (_highWaterMark.TryGetValue(info.Id, out var knownBest) && info.Science < knownBest)
            {
                LunaLog.Log($"Science subject '{info.Id}' ignored: incoming {info.Science} < known best {knownBest}");
                return;
            }

            System.StartIgnoringEvents();
            var currentSubjects = System.ScienceSubjects;

            if (info.IsDelta)
            {
                ApplyDelta(currentSubjects, info);
            }
            else
            {
                ApplyFull(currentSubjects, info);
            }

            _highWaterMark[info.Id] = Math.Max(info.Science, knownBest);
            System.StopIgnoringEvents();
            LunaLog.Log($"Science subject applied: {info.Id} sci={info.Science} collectedBy={info.CollectedBy}");
        }

        private static void ApplyDelta(Dictionary<string, ScienceSubject> subjects, ScienceSubjectInfo delta)
        {
            if (!subjects.TryGetValue(delta.Id, out var existing))
            {
                LunaLog.Warning($"Received delta for unknown subject '{delta.Id}' — ignored (not yet discovered locally)");
                return;
            }

            existing.science = delta.Science;
            existing.scienceCap = delta.ScienceCap;
            existing.dataScale = delta.DataScale;
            existing.scientificValue = delta.ScientificValue;
            existing.subjectValue = delta.SubjectValue;
        }

        private static void ApplyFull(Dictionary<string, ScienceSubject> subjects, ScienceSubjectInfo info)
        {
            var received = ConvertToScienceSubject(info.Data, info.NumBytes);
            if (received == null) return;

            if (!subjects.TryGetValue(info.Id, out var existing))
            {
                subjects.Add(received.id, received);
            }
            else
            {
                existing.science = received.science;
                existing.scienceCap = received.scienceCap;
                existing.dataScale = received.dataScale;
                existing.scientificValue = received.scientificValue;
                existing.subjectValue = received.subjectValue;
            }
        }

        private static void ApplyScienceSubjectRevert(ScienceSubjectInfo[] snapshot)
        {
            System.StartIgnoringEvents();
            var currentSubjects = System.ScienceSubjects;

            foreach (var info in snapshot)
            {
                if (info == null) continue;
                if (!currentSubjects.TryGetValue(info.Id, out var existing)) continue;

                // Merge: never roll back science that we or another player earned above this value
                if (existing.science > info.Science)
                {
                    LunaLog.Log($"Revert ignored for '{info.Id}': local {existing.science} > reverted {info.Science}");
                    continue;
                }

                existing.science = info.Science;
                existing.scienceCap = info.ScienceCap;
                existing.dataScale = info.DataScale;
                existing.scientificValue = info.ScientificValue;
                existing.subjectValue = info.SubjectValue;

                _highWaterMark[info.Id] = info.Science;
            }

            System.StopIgnoringEvents();
            LunaLog.Log($"Science subject revert applied: {snapshot.Length} subjects");
        }

        private static ScienceSubject ConvertToScienceSubject(byte[] data, int numBytes)
        {
            var node = new ConfigNode("Science");
            try
            {
                node.AddData(data.DeserializeToConfigNode(numBytes));
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while deserializing science subject configNode: {e}");
                return null;
            }
            return new ScienceSubject(node);
        }
    }
}
