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

    private readonly int[] goals = { 50, 200, 1 };
    private readonly int[] rewards = { 10, 20, 15 };

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

        progress[index] = Mathf.Min(
            progress[index] + amount,
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

        if (missionDate == today)
        {
            return;
        }

        missionDate = today;

        for (int i = 0; i < 3; i++)
        {
            progress[i] = 0;
            claimed[i] = false;
        }

        UpdateUI();
        SaveProgress();
    }

    private void UpdateUI()
    {
        clicksText.text = claimed[0]
            ? "50 нажатий — награда получена!"
            : "Нажми на руду: " + progress[0] + "/50";

        goldText.text = claimed[1]
            ? "200 золота — награда получена!"
            : "Добудь золото: " + progress[1] + "/200";

        upgradeText.text = claimed[2]
            ? "Улучшение — награда получена!"
            : "Улучши клик: " + progress[2] + "/1";

        clicksButton.interactable =
            progress[0] >= goals[0] && !claimed[0];

        goldButton.interactable =
            progress[1] >= goals[1] && !claimed[1];

        upgradeButton.interactable =
            progress[2] >= goals[2] && !claimed[2];
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