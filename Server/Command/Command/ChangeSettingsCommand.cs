using Server.Command.Command.Base;
using System;
using System.Xml;
using Server.Command.Common;
using System.IO;
using System.Reflection;
using Server.Log;

namespace Server.Command.Command
{
    public class ChangeSettingsCommand : SimpleCommand
    {
        //Executes the ChangeSettingsCommand
        public override void Execute(string commandArgs)
        {
            //Set messages
            var msgUsage = Environment.NewLine
                + Environment.NewLine + "Usage:"
                + Environment.NewLine + "/changesettings settingsFile settingName newValue";

            var msgSettingFileInfo = Environment.NewLine + "Parameter settingsFile can be settings, gameplaysettings or debugsettings.";

            var msgDescription = Environment.NewLine
                + Environment.NewLine + "Description:"
                + msgSettingFileInfo
                + Environment.NewLine + "Parameter settingName corresponds to the names inside the settings xml files."
                + Environment.NewLine + "Parameter settingName is case sensitive."
                + Environment.NewLine + "Parameter newValue is the new value."
                + Environment.NewLine + "Use '-' to set the new value as empty."
                + Environment.NewLine + Environment.NewLine + "Examples:"
                + Environment.NewLine + "This will change server password to empty ..."
                + Environment.NewLine + "/changesettings settings Password -"
                + Environment.NewLine + "This will change MaxPlayers to 8 ..."
                + Environment.NewLine + "/changesettings settings MaxPlayers 8";

            //Get param array from command
            CommandSystemHelperMethods.SplitCommandParamArray(commandArgs, out var parameters);
            
            //Check parameters
            if (parameters == null)
            {
                LunaLog.Error(Environment.NewLine + "Syntax error. No parameters found." + msgUsage + msgDescription);
                return;
            }
            if (parameters.Length != 3)
            {
                LunaLog.Error(Environment.NewLine + "Syntax error. Wrong number of parameters." + msgUsage + msgDescription);
                return;
            }
            if (parameters[0] == "")
            {
                LunaLog.Error(Environment.NewLine + "Syntax error. First parameter not found." + msgUsage + msgDescription);
                return;
            }
            if (parameters[1] == "")
            {
                LunaLog.Error(Environment.NewLine + "Syntax error. Second parameter not found." + msgUsage + msgDescription);
                return;
            }
            if (parameters[2] == "")
            {
                LunaLog.Error(Environment.NewLine + "Syntax error. Third parameter not found." + msgUsage + msgDescription);
                return;
            }

            //Get param values
            var paramCount = 0;
            paramCount = parameters.Length;
            var settingsFile = parameters[0];
            var settingsFileName = "Settings.xml";
            var settingName = parameters[1];
            var newValue = parameters[2];

            //Check setting file
            var settingsFileLowered = settingsFile.ToLower();
            if (settingsFileLowered == "settings")
            {
                settingsFileName = "Settings.xml";
            }
            else if (settingsFileLowered == "gameplaysettings")
            {
                settingsFileName = "GameplaySettings.xml";
            }
            else if (settingsFileLowered == "debugsettings")
            {
                settingsFileName = "DebugSettings.xml";
            }
            else
            {
                LunaLog.Error(Environment.NewLine + "Syntax error. First parameter (settingsFile) wrong use." + Environment.NewLine + msgSettingFileInfo + msgUsage + msgDescription);
                return;
            }

            //Define settings xml path
            var settingsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Config\\" + settingsFileName;

            //Define stream reader and xml document
            var streamReader = new StreamReader(settingsPath);
            var document = new XmlDocument();

            //Try to change the settings
            try
            {
                //Change server settings
                ChangeServerSettings(settingName, newValue, streamReader, document, settingsPath);
            }
            catch (Exception e)
            {
                LunaLog.Error(Environment.NewLine + "Exception error: " + Environment.NewLine + e.ToString());
            }
            finally
            {
                //Close and Dispose streamReader if not already done
                if (streamReader != null) CloseDisposeStreamReader(streamReader);
            }
        }

        //Changes selected server option
        private static void ChangeServerSettings(string settingName, string newValue, StreamReader streamReader, XmlDocument document, string settingsPath)
        {
            //Set message
            var msgSettingNameInfo = Environment.NewLine + "Setting name error:"
                + Environment.NewLine + "'" + settingName + "'"
                + Environment.NewLine + "is not a valid setting name."
                + Environment.NewLine + "Setting name is case sensitive.";

            //Load document
            document.Load(streamReader);
            //Try to find the xml node and save the new value to setting file
            try
            {
                //Get node
                var nodeSetting = document.SelectSingleNode("//" + settingName);
                //Return if null
                if (nodeSetting == null)
                {
                    LunaLog.Error(msgSettingNameInfo);
                    return;
                }
                //Change node inner text
                if (newValue == "-")
                {
                    nodeSetting.InnerText = "";
                }
                else
                {
                    nodeSetting.InnerText = newValue;
                }
                //Save document
                SaveDocument(streamReader, document, settingsPath);
                //Send info message that setting has changed
                LunaLog.Normal(Environment.NewLine + "Setting changes saved." + Environment.NewLine + "You may need to restart the server for the changes to take effect.");
            }
            catch (Exception)
            {
                LunaLog.Error(msgSettingNameInfo);
                return;
            }
        }
        
        //Saves changes to document
        private static void SaveDocument(StreamReader streamReader, XmlDocument document, string settingsPath)
        {
            //Close and dispose streamReader to be able to save the document
            CloseDisposeStreamReader(streamReader);
            //Save document
            document.Save(settingsPath);
        }

        //Closes and disposes the stream reader
        private static void CloseDisposeStreamReader(StreamReader streamReader)
        {
            streamReader.Close();
            streamReader.Dispose();
        }
    }
}
