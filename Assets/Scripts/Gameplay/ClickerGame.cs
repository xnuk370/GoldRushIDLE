using TMPro;
using UnityEngine;
using System;
using UnityEngine.UI;

public class ClickerGame : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private Button dailyRewardButton;
    [SerializeField] private TMP_Text dailyRewardText;
    [SerializeField] private DailyMissions dailyMissions;
    [SerializeField] private Achievements achievements;
    [SerializeField] private TMP_Text offlineIncomeText;

    private string lastRewardDate = "";

    private int gold = 0;
    private int goldPerClick = 1;
    private int miners = 0;
    private const int MaxOfflineSeconds = 8 * 60 * 60;
    private int upgradeCost = 10;
    private int minerCost = 25;
    private float miningTimer = 0f;

    private int tutorialStep = 0;

    private void Start()
    {
        gold = PlayerPrefs.GetInt("Gold", 0);
        goldPerClick = PlayerPrefs.GetInt("GoldPerClick", 1);
        miners = PlayerPrefs.GetInt("Miners", 0);
        tutorialStep = PlayerPrefs.GetInt("TutorialStep", 0);

        lastRewardDate = PlayerPrefs.GetString("LastRewardDate", "");
        ApplyOfflineIncome();
        UpdateDailyRewardUI();
        UpdateGoldText();
        UpdateTutorial();

        InvokeRepeating(nameof(SaveProgress), 5f, 5f);
        InvokeRepeating(nameof(UpdateDailyRewardUI), 1f, 1f);
        SaveProgress();
    }

    private void Update()
    {
        if (miners == 0)
        {
            return;
        }

        miningTimer += Time.deltaTime;

        while (miningTimer >= 1f)
        {
            miningTimer -= 1f;
            gold += miners;
            dailyMissions.RegisterGold(miners);
            achievements.RegisterGold(miners);

            UpdateGoldText();
            UpdateTutorial();
        }
    }

    public void MineGold()
    {
        gold += goldPerClick;
        dailyMissions.RegisterClick();
        dailyMissions.RegisterGold(goldPerClick);
        achievements.RegisterClick();
        achievements.RegisterGold(goldPerClick);

        if (tutorialStep == 0)
        {
            tutorialStep = 1;
        }

        UpdateGoldText();
        UpdateTutorial();
    }

    public void UpgradeClick()
    {
        if (gold < upgradeCost)
        {
            return;
        }

        gold -= upgradeCost;
        goldPerClick++;
        dailyMissions.RegisterUpgrade();
        achievements.RegisterUpgrade();

        if (tutorialStep == 2)
        {
            tutorialStep = 3;
        }

        UpdateGoldText();
        UpdateTutorial();
        SaveProgress();
    }

    public void BuyMiner()
    {
        if (gold < minerCost)
        {
            return;
        }

        gold -= minerCost;
        miners++;
        achievements.RegisterMiner();

        UpdateGoldText();
        UpdateTutorial();
        SaveProgress();
    }

    private void UpdateGoldText()
    {
        goldText.text = "Золото: " + gold
            + "\nЗа клик: " + goldPerClick
            + "\nВ секунду: " + miners;
    }

    private void UpdateTutorial()
    {
        if (tutorialStep == 1 && gold >= upgradeCost)
        {
            tutorialStep = 2;
        }

        tutorialText.gameObject.SetActive(tutorialStep < 3);

        switch (tutorialStep)
        {
            case 0:
                tutorialText.text = "Нажми на руду, чтобы добыть золото";
                break;

            case 1:
                tutorialText.text = "Накопи 10 золота для улучшения клика";
                break;

            case 2:
                tutorialText.text = "Нажми «Улучшить клик»: добыча увеличится на 1";
                break;
        }
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("Gold", gold);
        PlayerPrefs.SetInt("GoldPerClick", goldPerClick);
        PlayerPrefs.SetInt("Miners", miners);
        PlayerPrefs.SetInt("TutorialStep", tutorialStep);
        PlayerPrefs.SetString("LastRewardDate", lastRewardDate);
        achievements.SaveProgress();
        PlayerPrefs.SetString(
            "LastActiveUtc",
            DateTime.UtcNow.Ticks.ToString()
        );
        PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveProgress();
        }
    }

    private void OnApplicationQuit()
    {
        SaveProgress();
    }

    public void ClaimDailyReward()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        if (lastRewardDate == today)
        {
            return;
        }

        gold += 20;
        lastRewardDate = today;

        UpdateGoldText();
        UpdateTutorial();
        UpdateDailyRewardUI();
        SaveProgress();
    }

    private void UpdateDailyRewardUI()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        bool available = lastRewardDate != today;

        dailyRewardButton.interactable = available;

        dailyRewardText.text = available
            ? "Ежедневная награда доступна"
            : "Награда получена. Возвращайся завтра!";
    }

    public void AddMissionReward(int amount)
    {
        gold += amount;

        UpdateGoldText();
        UpdateTutorial();
        SaveProgress();
    }

    private void ApplyOfflineIncome()
    {
        offlineIncomeText.text = "";
        string savedTime = PlayerPrefs.GetString("LastActiveUtc", "");

        if (!long.TryParse(savedTime, out long savedTicks))
        {
            return;
        }

        long nowTicks = DateTime.UtcNow.Ticks;

        if (savedTicks <= 0 || savedTicks > nowTicks)
        {
            return;
        }

        long elapsedSeconds =
            (nowTicks - savedTicks) / TimeSpan.TicksPerSecond;

        int seconds = (int)Math.Min(
            elapsedSeconds,
            MaxOfflineSeconds
        );

        if (seconds == 0 || miners == 0)
        {
            return;
        }

        long income = (long)seconds * miners;

        int addedGold = (int)Math.Min(
            income,
            (long)int.MaxValue - gold
        );

        gold += addedGold;

        if (addedGold > 0)
        {
            offlineIncomeText.text =
                "Пока тебя не было, шахтёры добыли "
                + addedGold + " золота!";

            CancelInvoke(nameof(HideOfflineIncome));
            Invoke(nameof(HideOfflineIncome), 10f);
        }

        Debug.Log("Доход за отсутствие: " + addedGold + " золота");
    }

    private void HideOfflineIncome()
    {
        offlineIncomeText.text = "";
    }
}