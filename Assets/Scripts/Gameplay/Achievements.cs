using TMPro;
using UnityEngine;

public class Achievements : MonoBehaviour
{
    [SerializeField] private TMP_Text achievementsText;
    [SerializeField] private ClickerGame clickerGame;

    private int totalGold;
    private bool loaded;

    private readonly bool[] unlocked = new bool[5];

    private void Start()
    {
        LoadProgress();
        UpdateUI();
    }

    private void LoadProgress()
    {
        if (loaded)
        {
            return;
        }

        totalGold = PlayerPrefs.GetInt("AchievementTotalGold", 0);

        for (int i = 0; i < unlocked.Length; i++)
        {
            unlocked[i] =
                PlayerPrefs.GetInt("AchievementUnlocked" + i, 0) == 1;
        }

        loaded = true;
    }

    public void RegisterClick()
    {
        LoadProgress();
        Unlock(0);
        UpdateUI();
    }

    public void RegisterGold(int amount)
    {
        LoadProgress();
        totalGold += Mathf.Min(amount, 1000 - totalGold);

        if (totalGold >= 100)
        {
            Unlock(1);
        }

        if (totalGold >= 1000)
        {
            Unlock(4);
        }

        UpdateUI();
    }

    public void RegisterUpgrade()
    {
        LoadProgress();
        Unlock(2);
        UpdateUI();
    }

    public void RegisterMiner()
    {
        LoadProgress();
        Unlock(3);
        UpdateUI();
    }

    private void Unlock(int index)
    {
        if (unlocked[index])
        {
            return;
        }

        unlocked[index] = true;
        clickerGame.AddMissionReward(10);
        SaveProgress();
    }

    private void UpdateUI()
    {
        achievementsText.text =
            "Первый клик — " + Status(0)
            + "\n\nДобыть 100 золота — "
            + Mathf.Min(totalGold, 100) + "/100 — " + Status(1)
            + "\n\nКупить улучшение клика — " + Status(2)
            + "\n\nКупить первого шахтёра — " + Status(3)
            + "\n\nДобыть 1000 золота — "
            + totalGold + "/1000 — " + Status(4)
            + "\n\nНаграда: 10 золота за каждое достижение";
    }

    private string Status(int index)
    {
        return unlocked[index] ? "Получено" : "Не получено";
    }

    public void SaveProgress()
    {
        if (!loaded)
        {
            return;
        }

        PlayerPrefs.SetInt("AchievementTotalGold", totalGold);

        for (int i = 0; i < unlocked.Length; i++)
        {
            PlayerPrefs.SetInt(
                "AchievementUnlocked" + i,
                unlocked[i] ? 1 : 0
            );
        }
    }
}