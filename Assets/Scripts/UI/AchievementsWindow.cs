using UnityEngine;

public class AchievementsWindow : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject achievementsPanel;

    private void Start()
    {
        achievementsPanel.SetActive(false);
    }

    public void Open()
    {
        shopPanel.SetActive(false);
        achievementsPanel.SetActive(true);
    }

    public void Close()
    {
        achievementsPanel.SetActive(false);
    }
}