using YG;
using TMPro;
using UnityEngine;

public class Achievements : MonoBehaviour
{
    [SerializeField] private TMP_Text achievementsText;
    [SerializeField] private ClickerGame clickerGame;

    private int totalGold;
    private bool loaded;

    private readonly bool[] unlocked = new bool[5];

    public void Initialize()
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

        var data = YG2.saves;

        totalGold = data.achievementTotalGold;

        if (data.achievementsUnlocked == null)
        {
            data.achievementsUnlocked =
                new System.Collections.Generic.List<bool>();
        }

        while (data.achievementsUnlocked.Count < unlocked.Length)
        {
            data.achievementsUnlocked.Add(false);
        }

        for (int i = 0; i < unlocked.Length; i++)
        {
            unlocked[i] = data.achievementsUnlocked[i];
        }

        loaded = true;
    }

    public void RegisterClick()
    {
        if (!loaded)
        {
            return;
        }
        Unlock(0);
        UpdateUI();
    }

    public void RegisterGold(int amount)
    {
        if (!loaded)
        {
            return;
        }
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
        if (!loaded)
        {
            return;
        }
        Unlock(2);
        UpdateUI();
    }

    public void RegisterMiner()
    {
        if (!loaded)
        {
            return;
        }
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

        YG2.saves.achievementTotalGold = totalGold;

        for (int i = 0; i < unlocked.Length; i++)
        {
            YG2.saves.achievementsUnlocked[i] = unlocked[i];
        }
    }
}