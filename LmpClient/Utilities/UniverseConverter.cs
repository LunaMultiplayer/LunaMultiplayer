using LmpCommon;
using LmpCommon.Xml;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LmpClient.Utilities
{
    public class UniverseConverter
    {
        private static string SavesFolder { get; } = CommonUtil.CombinePaths(MainSystem.KspPath, "saves");

        public static void GenerateUniverse(string saveName)
        {
            var universeFolder = CommonUtil.CombinePaths(MainSystem.KspPath, "Universe");
            if (Directory.Exists(universeFolder))
                Directory.Delete(universeFolder, true);

            var saveFolder = CommonUtil.CombinePaths(SavesFolder, saveName);
            if (!Directory.Exists(saveFolder))
            {
                LunaLog.Log($"[LMP]: Failed to generate a LMP universe for '{saveName}', Save directory doesn't exist");
                LunaScreenMsg.PostScreenMessage($"Failed to generate a LMP universe for '{saveName}', Save directory doesn't exist", 5f,
                    ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            var persistentFile = CommonUtil.CombinePaths(saveFolder, "persistent.sfs");
            if (!File.Exists(persistentFile))
            {
                LunaLog.Log($"[LMP]: Failed to generate a LMP universe for '{saveName}', persistent.sfs doesn't exist");
                LunaScreenMsg.PostScreenMessage($"Failed to generate a LMP universe for '{saveName}', persistent.sfs doesn't exist", 5f,
                    ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            Directory.CreateDirectory(universeFolder);
            var vesselFolder = CommonUtil.CombinePaths(universeFolder, "Vessels");
            Directory.CreateDirectory(vesselFolder);
            var scenarioFolder = CommonUtil.CombinePaths(universeFolder, "Scenarios");
            Directory.CreateDirectory(scenarioFolder);
            var kerbalFolder = CommonUtil.CombinePaths(universeFolder, "Kerbals");
            Directory.CreateDirectory(kerbalFolder);

            //Load game data
            var persistentData = ConfigNode.Load(persistentFile);
            if (persistentData == null)
            {
                LunaLog.Log($"[LMP]: Failed to generate a LMP universe for '{saveName}', failed to load persistent data");
                LunaScreenMsg.PostScreenMessage($"Failed to generate a LMP universe for '{saveName}', failed to load persistent data", 5f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            var gameData = persistentData.GetNode("GAME");
            if (gameData == null)
            {
                LunaLog.Log($"[LMP]: Failed to generate a LMP universe for '{saveName}', failed to load game data");
                LunaScreenMsg.PostScreenMessage($"Failed to generate a LMP universe for '{saveName}', failed to load game data", 5f,
                    ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            //Save vessels
            var flightState = gameData.GetNode("FLIGHTSTATE");
            if (flightState == null)
            {
                LunaLog.Log($"[LMP]: Failed to generate a LMP universe for '{saveName}', failed to load flight state data");
                LunaScreenMsg.PostScreenMessage($"Failed to generate a LMP universe for '{saveName}', failed to load flight state data", 5f,
                    ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            var vesselNodes = flightState.GetNodes("VESSEL");
            if (vesselNodes != null)
            {
                foreach (var cn in vesselNodes)
                {
                    var vesselId = Common.ConvertConfigStringToGuidString(cn.GetValue("pid"));
                    LunaLog.Log($"[LMP]: Saving vessel {vesselId}, Name: {cn.GetValue("name")}");

                    var xmlData = ConfigNodeXmlParser.ConvertToXml(Encoding.UTF8.GetString(ConfigNodeSerializer.Serialize(cn)));
                    File.WriteAllText(CommonUtil.CombinePaths(vesselFolder, $"{vesselId}.xml"), xmlData);
                }
            }

            //Save scenario data
            var scenarioNodes = gameData.GetNodes("SCENARIO");
            if (scenarioNodes != null)
            {
                foreach (var cn in scenarioNodes)
                {
                    var scenarioName = cn.GetValue("name");
                    if (string.IsNullOrEmpty(scenarioName)) continue;

                    LunaLog.Log($"[LMP]: Saving scenario: {scenarioName}");

                    var xmlData = ConfigNodeXmlParser.ConvertToXml(Encoding.UTF8.GetString(ConfigNodeSerializer.Serialize(cn)));
                    File.WriteAllText(CommonUtil.CombinePaths(scenarioFolder, $"{scenarioName}.xml"), xmlData);
                }
            }

            //Save kerbal data
            var kerbalNodes = gameData.GetNode("ROSTER").GetNodes("KERBAL");
            if (kerbalNodes != null)
            {
                foreach (var cn in kerbalNodes)
                {
                    var kerbalName = cn.GetValue("name");
                    LunaLog.Log($"[LMP]: Saving kerbal: {kerbalName}");

                    cn.Save(CommonUtil.CombinePaths(kerbalFolder, $"{kerbalName}.txt"));
                }
            }

            LunaLog.Log($"[LMP]: Generated KSP_folder/Universe from {saveName}");
            LunaScreenMsg.PostScreenMessage($"Generated KSP_folder/Universe from {saveName}", 5f, ScreenMessageStyle.UPPER_CENTER);
        }

        public static IEnumerable<string> GetSavedNames()
        {
            var returnList = new List<string>();
            var possibleSaves = Directory.GetDirectories(SavesFolder);
            foreach (var saveDirectory in possibleSaves)
            {
                var trimmedDirectory = saveDirectory;
                //Cut the trailing path character off if we need to
                if (saveDirectory[saveDirectory.Length - 1] == Path.DirectorySeparatorChar)
                    trimmedDirectory = saveDirectory.Substring(0, saveDirectory.Length - 2);
                var saveName = trimmedDirectory.Substring(trimmedDirectory.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                if (saveName.ToLower() != "training" && saveName.ToLower() != "scenarios" &&
                    File.Exists(CommonUtil.CombinePaths(saveDirectory, "persistent.sfs")))
                    returnList.Add(saveName);
            }
            return returnList;
        }
    }
}
