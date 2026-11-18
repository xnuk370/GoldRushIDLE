using YG;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyMissions : MonoBehaviour
{
    [SerializeField] private TMP_Text clicksText;
    [SerializeField] private Button clicksButton;

    [SerializeField] private TMP_Text goldText;
    [SerializeField] private Button goldButton;

    [SerializeField] private TMP_Text upgradeText;
    [SerializeField] private Button upgradeButton;

    [SerializeField] private ClickerGame clickerGame;

    private int[] progress = new int[3];
    private bool[] claimed = new bool[3];

    private int[] goals = { 50, 200, 1 };
    private int[] rewards = { 10, 20, 15 };

    private readonly string[] keys =
    {
        "DailyClicks",
        "DailyGold",
        "DailyUpgrade"
    };

    private string missionDate;
    private bool loaded;

    public void Initialize()
    {
        LoadProgress();
        CheckNewDay();
        UpdateUI();

        InvokeRepeating(nameof(CheckNewDay), 1f, 1f);
    }

    private void LoadProgress()
    {
        if (loaded)
        {
            return;
        }

        var data = YG2.saves;

        progress[0] = data.dailyClicks;
        progress[1] = data.dailyGold;
        progress[2] = data.dailyUpgrade;

        claimed[0] = data.dailyClicksClaimed;
        claimed[1] = data.dailyGoldClaimed;
        claimed[2] = data.dailyUpgradeClaimed;

        missionDate = data.dailyMissionDate;
        loaded = true;
        if (data.dailyMissionPlanSaved)
        {
            goals[1] = Mathf.Max(1, data.dailyGoldGoal);

            rewards[0] = Mathf.Max(1, data.dailyClicksReward);
            rewards[1] = Mathf.Max(1, data.dailyGoldReward);
            rewards[2] = Mathf.Max(1, data.dailyUpgradeReward);
        }
    }

    public void RegisterClick()
    {
        AddProgress(0, 1);
    }

    public void RegisterGold(int amount)
    {
        AddProgress(1, amount);
    }

    public void RegisterUpgrade()
    {
        AddProgress(2, 1);
    }

    private void AddProgress(int index, int amount)
    {
        if (!loaded)
        {
            return;
        }
        CheckNewDay();

        progress[index] = (int)Math.Min(
            (long)progress[index] + Math.Max(0, amount),
            goals[index]
        );

        UpdateUI();
    }

    public void ClaimClicksReward()
    {
        ClaimReward(0);
    }

    public void ClaimGoldReward()
    {
        ClaimReward(1);
    }

    public void ClaimUpgradeReward()
    {
        ClaimReward(2);
    }

    private void ClaimReward(int index)
    {
        if (!loaded)
        {
            return;
        }
        CheckNewDay();

        if (progress[index] < goals[index] || claimed[index])
        {
            return;
        }

        claimed[index] = true;
        clickerGame.AddMissionReward(rewards[index]);

        UpdateUI();
        SaveProgress();
    }

    private void CheckNewDay()
    {
        if (!loaded)
        {
            return;
        }

        string today = GameClock.Today;

        // Не сбрасываем миссии, если тестовое время перевели назад.
        if (!string.IsNullOrEmpty(missionDate)
            && string.CompareOrdinal(today, missionDate) <= 0)
        {
            return;
        }

        missionDate = today;

        long income = Math.Max(
            1L,
            clickerGame.EstimatedActiveIncomePerSecond
        );

        goals[0] = 50;
        goals[1] = CalculateAmount(income, 120);
        goals[2] = 1;

        rewards[0] = CalculateAmount(income, 10);
        rewards[1] = CalculateAmount(income, 20);
        rewards[2] = CalculateAmount(income, 15);

        for (int i = 0; i < 3; i++)
        {
            progress[i] = 0;
            claimed[i] = false;
        }

        UpdateUI();
        SaveProgress();
    }

    private int CalculateAmount(long incomePerSecond, int seconds)
    {
        return (int)Math.Min(
            incomePerSecond * seconds,
            int.MaxValue
        );
    }

    private void UpdateUI()
    {
        clicksText.text = claimed[0]
            ? "Нажатия — награда получена!"
            : "Нажми на руду: " + progress[0] + "/" + goals[0];

        goldText.text = claimed[1]
            ? "Добыча — награда получена!"
            : "Добудь золото: " + progress[1] + "/" + goals[1];

        upgradeText.text = claimed[2]
            ? "Улучшение — награда получена!"
            : "Улучши клик: " + progress[2] + "/" + goals[2];

        UpdateRewardButton(clicksButton, 0);
        UpdateRewardButton(goldButton, 1);
        UpdateRewardButton(upgradeButton, 2);
    }

    private void UpdateRewardButton(Button button, int index)
    {
        button.interactable =
            progress[index] >= goals[index] && !claimed[index];

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);

        if (label != null)
        {
            label.text = claimed[index]
                ? "Получено"
                : "Забрать " + rewards[index] + " золота";
        }
    }

    public void SaveProgress()
    {
        if (!loaded)
        {
            return;
        }

        var data = YG2.saves;

        data.dailyClicks = progress[0];
        data.dailyGold = progress[1];
        data.dailyUpgrade = progress[2];

        data.dailyClicksClaimed = claimed[0];
        data.dailyGoldClaimed = claimed[1];
        data.dailyUpgradeClaimed = claimed[2];

        data.dailyMissionDate = missionDate;
        data.dailyMissionPlanSaved = true;

        data.dailyGoldGoal = goals[1];

        data.dailyClicksReward = rewards[0];
        data.dailyGoldReward = rewards[1];
        data.dailyUpgradeReward = rewards[2];
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
}