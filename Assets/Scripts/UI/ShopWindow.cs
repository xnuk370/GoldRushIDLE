using UnityEngine;

public class ShopWindow : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject achievementsPanel;

    private void Start()
    {
        shopPanel.SetActive(false);
    }

    public void Open()
    {
        achievementsPanel.SetActive(false);
        shopPanel.SetActive(true);
    }

    public void Close()
    {
        shopPanel.SetActive(false);
    }
}