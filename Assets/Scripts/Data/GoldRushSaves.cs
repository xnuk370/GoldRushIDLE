using System.Collections.Generic;

namespace YG
{
    public partial class SavesYG
    {
        public int goldRushSaveVersion = 0;

        public int gold = 0;
        public int goldPerClick = 1;
        public int miners = 0;
        public int tutorialStep = 0;

        public string lastRewardDate = "";
        public string lastActiveUtc = "";

        public string dailyMissionDate = "";

        public int dailyClicks = 0;
        public int dailyGold = 0;
        public int dailyUpgrade = 0;

        public bool dailyClicksClaimed = false;
        public bool dailyGoldClaimed = false;
        public bool dailyUpgradeClaimed = false;

        public bool dailyMissionPlanSaved = false;
        public int dailyGoldGoal = 200;

        public int dailyClicksReward = 10;
        public int dailyGoldReward = 20;
        public int dailyUpgradeReward = 15;

        public int achievementTotalGold = 0;

        public List<bool> achievementsUnlocked = new List<bool>
        {
            false, false, false, false, false
        };
    }
}