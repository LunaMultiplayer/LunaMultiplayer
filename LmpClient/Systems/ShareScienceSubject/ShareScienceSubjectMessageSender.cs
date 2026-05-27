using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Extensions;
using LmpClient.Network;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using System;
using System.Collections.Generic;

namespace LmpClient.Systems.ShareScienceSubject
{
    public class ShareScienceSubjectMessageSender : SubSystem<ShareScienceSubjectSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }

        public void SendScienceSubjectMessage(ScienceSubject subject, bool wasTransmitted)
        {
            var isUpdate = System.ScienceSubjects.ContainsKey(subject.id);
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressScienceSubjectMsgData>();

            msgData.ScienceSubject.Id = subject.id;
            msgData.ScienceSubject.Science = subject.science;
            msgData.ScienceSubject.ScienceCap = subject.scienceCap;
            msgData.ScienceSubject.DataScale = subject.dataScale;
            msgData.ScienceSubject.ScientificValue = subject.scientificValue;
            msgData.ScienceSubject.SubjectValue = subject.subjectValue;
            msgData.ScienceSubject.WasTransmitted = wasTransmitted;
            msgData.ScienceSubject.CollectedBy = SettingsSystem.CurrentSettings.PlayerName;
            msgData.ScienceSubject.IsDelta = isUpdate;

            if (!isUpdate)
            {
                // First discovery: include the full ConfigNode so the server can persist it
                var configNode = ConvertScienceSubjectToConfigNode(subject);
                if (configNode == null) return;

                var data = configNode.Serialize();
                var numBytes = data.Length;

                msgData.ScienceSubject.NumBytes = numBytes;
                if (msgData.ScienceSubject.Data.Length < numBytes)
                    msgData.ScienceSubject.Data = new byte[numBytes];
                Array.Copy(data, msgData.ScienceSubject.Data, numBytes);
            }

            SendMessage(msgData);
            LunaLog.Log($"Science experiment \"{subject.id}\" sent (isDelta={isUpdate}, wasTransmitted={wasTransmitted})");
        }

        /// <summary>
        /// Sends the pre-flight science snapshot back to the server so it can revert.
        /// Called when the player reverts to launch or editor.
        /// </summary>
        public void SendScienceSubjectRevert(IReadOnlyCollection<KeyValuePair<string, ScienceSubject>> snapshot)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressScienceSubjectRevertMsgData>();
            msgData.SubjectCount = snapshot.Count;

            if (msgData.Subjects.Length < snapshot.Count)
                msgData.Subjects = new ScienceSubjectInfo[snapshot.Count];

            var i = 0;
            foreach (var kvp in snapshot)
            {
                var subject = kvp.Value;
                var info = new ScienceSubjectInfo
                {
                    Id = subject.id,
                    Science = subject.science,
                    ScienceCap = subject.scienceCap,
                    DataScale = subject.dataScale,
                    ScientificValue = subject.scientificValue,
                    SubjectValue = subject.subjectValue,
                    WasTransmitted = false,
                    CollectedBy = SettingsSystem.CurrentSettings.PlayerName,
                    IsDelta = true
                };
                msgData.Subjects[i++] = info;
            }

            SendMessage(msgData);
            LunaLog.Log($"Science subject revert sent: {snapshot.Count} subjects");
        }

        private static ConfigNode ConvertScienceSubjectToConfigNode(ScienceSubject subject)
        {
            var configNode = new ConfigNode();
            try
            {
                subject.Save(configNode);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while saving science subject: {e}");
                return null;
            }
            return configNode;
        }
    }
}
