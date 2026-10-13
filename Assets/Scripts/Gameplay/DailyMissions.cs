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

    private void Start()
    {
        CheckNewDay();
        UpdateUI();

        InvokeRepeating(nameof(CheckNewDay), 1f, 1f);
        InvokeRepeating(nameof(SaveProgress), 5f, 5f);
    }

    private void LoadProgress()
    {
        if (loaded)
        {
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            progress[i] = PlayerPrefs.GetInt(keys[i], 0);
            claimed[i] =
                PlayerPrefs.GetInt(keys[i] + "Claimed", 0) == 1;
        }

        missionDate = PlayerPrefs.GetString("DailyMissionDate", "");
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
        LoadProgress();

        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");

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

    private void SaveProgress()
    {
        if (!loaded)
        {
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            PlayerPrefs.SetInt(keys[i], progress[i]);
            PlayerPrefs.SetInt(
                keys[i] + "Claimed",
                claimed[i] ? 1 : 0
            );
        }

        PlayerPrefs.SetString("DailyMissionDate", missionDate);
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
}