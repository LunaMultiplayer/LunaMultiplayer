using Server.Command.Command.Base;
using Server.Command.Common;
using Server.Context;
using Server.Log;
using System;
using System.IO;
using System.Xml;

namespace Server.Command.Command
{
    public class ChangeSettingsCommand : SimpleCommand
    {
        private static readonly string Usage = "Usage: /changesettings settingsFile settingName newValue" +
                                     "Description: Parameter settingsFile can be settings, gameplaysettings or debugsettings" +
                                     $"{Environment.NewLine}Parameter settingName corresponds to the names inside the settings xml files." +
                                     $"{Environment.NewLine}Parameter settingName is case sensitive." +
                                     $"{Environment.NewLine}Parameter newValue is the new value." +
                                     $"{Environment.NewLine}Use \'-\' to set the new value as empty." +
                                     $"{Environment.NewLine} Examples: This will change server password to empty ..." +
                                     $"{Environment.NewLine}/changesettings settings Password -" +
                                     $"{Environment.NewLine}This will change MaxPlayers to 8 ..." +
                                     $"{Environment.NewLine}/changesettings settings MaxPlayers 8";

        public override bool Execute(string commandArgs)
        {
            CommandSystemHelperMethods.SplitCommandParamArray(commandArgs, out var parameters);

            if (!ValidateParameters(parameters))
                return false;

            var settingsFile = parameters[0];
            var settingName = parameters[1];
            var newValue = parameters[2];

            var settingsPath = GetSettingsFilePath(settingsFile);
            if (string.IsNullOrEmpty(settingsPath))
            {
                LunaLog.Error($"Syntax error. Could not find a settings file called '{settingsFile}'");
                return false;
            }

            try
            {
                ChangeServerSettings(settingName, newValue, settingsPath);
            }
            catch (Exception e)
            {
                LunaLog.Error($"Error while changing settings: {e}");
            }

            return true;
        }

        private static bool ValidateParameters(string[] parameters)
        {
            if (parameters == null)
            {
                LunaLog.Error($"Syntax error. No parameters found. {Usage}");
                return false;
            }

            if (parameters.Length != 3)
            {
                LunaLog.Error($"Syntax error. Wrong number of parameters. {Usage}");
                return false;
            }

            if (string.IsNullOrEmpty(parameters[0]))
            {
                LunaLog.Error($"Syntax error. First parameter not found. {Usage}");
                return false;
            }

            if (string.IsNullOrEmpty(parameters[1]))
            {
                LunaLog.Error($"Syntax error. Second parameter not found. {Usage}");
                return false;
            }

            if (string.IsNullOrEmpty(parameters[2]))
            {
                LunaLog.Error($"Syntax error. Third parameter not found. {Usage}");
                return false;
            }

            return true;
        }

        private static string GetSettingsFilePath(string settingsFile)
        {
            foreach (var file in Directory.GetFiles(Path.Combine(ServerContext.ConfigDirectory)))
            {
                if (string.Equals(Path.GetFileNameWithoutExtension(file), settingsFile, StringComparison.InvariantCultureIgnoreCase))
                    return file;
            }

            return null;
        }

        private static void ChangeServerSettings(string settingName, string newValue, string settingsPath)
        {
            var document = new XmlDocument();
            document.Load(settingsPath);

            var nodeSetting = document.SelectSingleNode("//" + settingName);
            if (nodeSetting == null)
            {
                LunaLog.Error($"\'{settingName}\' is not a valid setting name. Setting name is case sensitive.");
                return;
            }

            nodeSetting.InnerText = newValue == "-" ? "" : newValue;
            document.Save(settingsPath);

            LunaLog.Normal("Setting changes saved. You may need to restart the server for the changes to take effect.");
        }
    }
}
