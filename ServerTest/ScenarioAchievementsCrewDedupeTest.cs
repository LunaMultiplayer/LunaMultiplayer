using LunaConfigNode.CfgNode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server.System.Scenario;
using System.Linq;

namespace ServerTest
{
    [TestClass]
    public class ScenarioAchievementsCrewDedupeTest
    {
        private const string AchievementWithDuplicateCrew = @"
KerbinFlyBy
{
    Reach
    {
        completed = 12345.6
        crew
        {
            item = Jebediah Kerman
            item = Bill Kerman
            item = Jebediah Kerman
            item = Jebediah Kerman
            item = Bob Kerman
            item = Bill Kerman
        }
        vessels
        {
            item = ProbeOne
        }
    }
    Orbit
    {
        completed = 12400.0
        crew
        {
            item = Jebediah Kerman
            item = Jebediah Kerman
        }
    }
}";

        private const string AchievementWithoutCrew = @"
KerbinFlyBy
{
    Reach
    {
        completed = 12345.6
        vessels
        {
            item = ProbeOne
        }
    }
}";

        private static string[] GetItemValues(ConfigNode node)
        {
            return node.GetAllValues()
                .Where(v => v.Key == "item")
                .Select(v => v.Value)
                .ToArray();
        }

        [TestMethod]
        public void DedupeCrewLists_LeavesUniqueItemsAlone()
        {
            var root = new ConfigNode(AchievementWithoutCrew);
            var achievement = root.GetNode("KerbinFlyBy").Value;

            ScenarioDataUpdater.DedupeCrewLists(achievement);

            var vessels = achievement.GetNode("Reach").Value.GetNode("vessels").Value;
            CollectionAssert.AreEqual(new[] { "ProbeOne" }, GetItemValues(vessels));
        }

        [TestMethod]
        public void DedupeCrewLists_RemovesDuplicatesPreservingFirstOccurrenceOrder()
        {
            var root = new ConfigNode(AchievementWithDuplicateCrew);
            var achievement = root.GetNode("KerbinFlyBy").Value;

            ScenarioDataUpdater.DedupeCrewLists(achievement);

            var reachCrew = achievement.GetNode("Reach").Value.GetNode("crew").Value;
            CollectionAssert.AreEqual(
                new[] { "Jebediah Kerman", "Bill Kerman", "Bob Kerman" },
                GetItemValues(reachCrew));

            var orbitCrew = achievement.GetNode("Orbit").Value.GetNode("crew").Value;
            CollectionAssert.AreEqual(
                new[] { "Jebediah Kerman" },
                GetItemValues(orbitCrew));
        }

        [TestMethod]
        public void DedupeCrewLists_OnlyTouchesCrewSubNodes()
        {
            const string nodeWithDupesElsewhere = @"
SomeAchievement
{
    Reach
    {
        completed = 12345.6
        vessels
        {
            item = ProbeOne
            item = ProbeOne
        }
        crew
        {
            item = Jeb
            item = Jeb
        }
    }
}";

            var root = new ConfigNode(nodeWithDupesElsewhere);
            var achievement = root.GetNode("SomeAchievement").Value;

            ScenarioDataUpdater.DedupeCrewLists(achievement);

            var crew = achievement.GetNode("Reach").Value.GetNode("crew").Value;
            CollectionAssert.AreEqual(new[] { "Jeb" }, GetItemValues(crew));

            // The non-crew "vessels" list must not be touched: there are valid KSP cases where
            // duplicates carry meaning (e.g. a player launching the same craft twice), and the
            // bug reported in #542 is specifically about the crew list.
            var vessels = achievement.GetNode("Reach").Value.GetNode("vessels").Value;
            CollectionAssert.AreEqual(new[] { "ProbeOne", "ProbeOne" }, GetItemValues(vessels));
        }

        [TestMethod]
        public void DedupeCrewLists_HandlesEmptyAndMissingCrew()
        {
            const string nodeWithEmptyCrew = @"
SomeAchievement
{
    Reach
    {
        crew
        {
        }
    }
}";

            var root = new ConfigNode(nodeWithEmptyCrew);
            var achievement = root.GetNode("SomeAchievement").Value;

            ScenarioDataUpdater.DedupeCrewLists(achievement);

            var crew = achievement.GetNode("Reach").Value.GetNode("crew").Value;
            Assert.AreEqual(0, GetItemValues(crew).Length);
        }

        [TestMethod]
        public void DedupeCrewLists_NullRootIsNoop()
        {
            ScenarioDataUpdater.DedupeCrewLists(null);
        }

        [TestMethod]
        public void MigrateProgressTrackingScenario_DedupesAllAchievementsUnderProgress()
        {
            const string scenarioText = @"
name = ProgressTracking
scene = 7, 8, 5
Progress
{
    KerbinFlyBy
    {
        Reach
        {
            crew
            {
                item = Jeb
                item = Jeb
                item = Bill
            }
        }
    }
    MunFlyBy
    {
        Reach
        {
            crew
            {
                item = Bob
                item = Bob
            }
        }
    }
}";

            var scenario = new ConfigNode(scenarioText);

            ScenarioDataUpdater.MigrateProgressTrackingScenario(scenario);

            var progress = scenario.GetNode("Progress").Value;
            var kerbinCrew = progress.GetNode("KerbinFlyBy").Value.GetNode("Reach").Value.GetNode("crew").Value;
            var munCrew = progress.GetNode("MunFlyBy").Value.GetNode("Reach").Value.GetNode("crew").Value;

            CollectionAssert.AreEqual(new[] { "Jeb", "Bill" }, GetItemValues(kerbinCrew));
            CollectionAssert.AreEqual(new[] { "Bob" }, GetItemValues(munCrew));
        }

        [TestMethod]
        public void MigrateProgressTrackingScenario_MissingProgressNodeIsNoop()
        {
            const string scenarioText = @"
name = ProgressTracking
scene = 7, 8, 5
";

            var scenario = new ConfigNode(scenarioText);

            ScenarioDataUpdater.MigrateProgressTrackingScenario(scenario);
        }

        [TestMethod]
        public void MigrateProgressTrackingScenario_NullScenarioIsNoop()
        {
            ScenarioDataUpdater.MigrateProgressTrackingScenario(null);
        }

        [TestMethod]
        public void DedupeCrewLists_RoundTripsThroughSerialization()
        {
            var root = new ConfigNode(AchievementWithDuplicateCrew);
            var achievement = root.GetNode("KerbinFlyBy").Value;

            ScenarioDataUpdater.DedupeCrewLists(achievement);

            var serialized = root.ToString();
            var rehydratedRoot = new ConfigNode(serialized);
            var rehydratedAchievement = rehydratedRoot.GetNode("KerbinFlyBy").Value;

            var reachCrew = rehydratedAchievement.GetNode("Reach").Value.GetNode("crew").Value;
            CollectionAssert.AreEqual(
                new[] { "Jebediah Kerman", "Bill Kerman", "Bob Kerman" },
                GetItemValues(reachCrew));
        }
    }
}
