using YG;
using System.Collections;
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
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeButtonText;
    [SerializeField] private Button minerButton;
    [SerializeField] private TMP_Text minerButtonText;

    private string lastRewardDate = "";

    private int gold = 0;
    private int goldPerClick = 1;
    private int miners = 0;
    private const int MaxOfflineSeconds = 4 * 60 * 60;
    private int upgradeCost = 10;
    private int minerCost = 50;
    private float miningTimer = 0f;

    private int tutorialStep = 0;
    private bool initialized;

    private IEnumerator Start()
    {
        goldText.text = "Загрузка прогресса...";
        dailyRewardButton.interactable = false;

        while (!YG2.isSDKEnabled)
        {
            yield return null;
        }

        var data = YG2.saves;

        gold = data.gold;
        goldPerClick = data.goldPerClick;
        miners = data.miners;
        tutorialStep = data.tutorialStep;
        lastRewardDate = data.lastRewardDate;
        gold = Mathf.Max(0, gold);
        goldPerClick = Mathf.Max(1, goldPerClick);
        miners = Mathf.Max(0, miners);

        upgradeCost = CalculatePrice(10, 125, goldPerClick - 1);
        minerCost = CalculatePrice(50, 135, miners);

        dailyMissions.Initialize();
        achievements.Initialize();

        ApplyOfflineIncome();

        initialized = true;

        UpdateDailyRewardUI();
        UpdateGoldText();
        UpdateTutorial();

        InvokeRepeating(nameof(SaveProgress), 15f, 15f);
        InvokeRepeating(nameof(UpdateDailyRewardUI), 1f, 1f);

        SaveProgress();
    }

    private void Update()
    {
        if (!initialized || miners == 0)
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
        if (!initialized)
        {
            return;
        }

        int income = (int)Math.Min(
            (long)ClickIncome,
            (long)int.MaxValue - gold
        );

        gold += income;

        dailyMissions.RegisterClick();
        dailyMissions.RegisterGold(income);

        achievements.RegisterClick();
        achievements.RegisterGold(income);

        if (tutorialStep == 0)
        {
            tutorialStep = 1;
        }

        UpdateGoldText();
        UpdateTutorial();
    }

    public void UpgradeClick()
    {
        if (!initialized)
        {
            return;
        }
        if (gold < upgradeCost || upgradeCost == int.MaxValue)
        {
            return;
        }

        gold -= upgradeCost;
        goldPerClick++;
        upgradeCost = CalculatePrice(10, 125, goldPerClick - 1);
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
        if (!initialized)
        {
            return;
        }
        if (gold < minerCost || minerCost == int.MaxValue)
        {
            return;
        }

        gold -= minerCost;
        miners++;
        minerCost = CalculatePrice(50, 135, miners);
        achievements.RegisterMiner();

        UpdateGoldText();
        UpdateTutorial();
        SaveProgress();
    }

    private void UpdateGoldText()
    {
        goldText.text = "Золото: " + gold
            + "\nЗа клик: " + ClickIncome
            + " (база " + goldPerClick
            + " + бонус " + miners / 2 + ")"
            + "\nАвтодобыча: " + miners + "/с";

        UpdateShopUI();
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
                tutorialText.text = "Накопи " + upgradeCost + " золота для улучшения клика";
                break;

            case 2:
                tutorialText.text = "Открой магазин и купи улучшение клика";
                break;
        }
    }

    private void SaveProgress()
    {
        if (!initialized)
        {
            return;
        }

        var data = YG2.saves;

        data.goldRushSaveVersion = 1;
        data.gold = gold;
        data.goldPerClick = goldPerClick;
        data.miners = miners;
        data.tutorialStep = tutorialStep;
        data.lastRewardDate = lastRewardDate;
        data.lastActiveUtc = GameClock.UtcNow.Ticks.ToString();

        dailyMissions.SaveProgress();
        achievements.SaveProgress();

        YG2.SaveProgress();
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
        if (!initialized)
        {
            return;
        }
        string today = GameClock.Today;

        if (lastRewardDate == today)
        {
            return;
        }

        int reward = (int)Math.Min(
            (long)DailyRewardAmount,
            (long)int.MaxValue - gold
        );

        gold += reward;
        lastRewardDate = today;

        UpdateGoldText();
        UpdateTutorial();
        UpdateDailyRewardUI();
        SaveProgress();
    }

    private void UpdateDailyRewardUI()
    {
        string today = GameClock.Today;
        bool available = lastRewardDate != today;

        dailyRewardButton.interactable = available;

        dailyRewardText.text = available
            ? "Ежедневная награда: " + DailyRewardAmount + " золота"
            : "Награда получена. Возвращайся завтра!";
    }

    public void AddMissionReward(int amount)
    {
        if (!initialized)
        {
            return;
        }
        gold += amount;

        UpdateGoldText();
        UpdateTutorial();
        SaveProgress();
    }

    private void ApplyOfflineIncome()
    {
        offlineIncomeText.text = "";
        string savedTime = YG2.saves.lastActiveUtc;

        if (!long.TryParse(savedTime, out long savedTicks))
        {
            return;
        }

        long nowTicks = GameClock.UtcNow.Ticks;

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

        long income = (long)seconds * miners / 10;

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

    private int ClickIncome
    {
        get
        {
            return (int)Math.Min(
                (long)goldPerClick + miners / 2,
                int.MaxValue
            );
        }
    }

    private int CalculatePrice(int initialPrice, int growthPercent, int purchases)
    {
        long price = initialPrice;

        for (int i = 0; i < purchases; i++)
        {
            // Целочисленное округление вверх.
            price = (price * growthPercent + 99) / 100;

            if (price >= int.MaxValue)
            {
                return int.MaxValue;
            }
        }

        return (int)price;
    }

    private void UpdateShopUI()
    {
        upgradeButton.interactable =
            initialized && gold >= upgradeCost
            && upgradeCost < int.MaxValue;

        minerButton.interactable =
            initialized && gold >= minerCost
            && minerCost < int.MaxValue;

        upgradeButtonText.text =
            "Улучшить клик — " + upgradeCost + " золота"
            + "\nБазовая сила: " + goldPerClick
            + "\nПокупка: +1 золото за клик";

        minerButtonText.text =
            "Купить шахтёра — " + minerCost + " золота"
            + "\nШахтёров: " + miners
            + "\nПокупка: +1 золото/с"
            + "\nКаждые 2 шахтёра: +1 за клик";
    }

    private int DailyRewardAmount
    {
        get
        {
            long activeIncomePerSecond =
                (long)ClickIncome * 2 + miners;

            return (int)Math.Min(
                Math.Max(20L, activeIncomePerSecond * 30),
                int.MaxValue
            );
        }
    }
    public long EstimatedActiveIncomePerSecond
    {
        get
        {
            // Расчётный темп: два нажатия в секунду.
            return (long)ClickIncome * 2 + miners;
        }
    }
}