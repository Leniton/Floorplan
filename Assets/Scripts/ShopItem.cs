using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text name;
    [SerializeField] private TMP_Text cost;
    [SerializeField] private TMP_Text description;
    [SerializeField] private GameObject soldOutPanel;

    private PurchaseData data;
    
    public void Setup(PurchaseData newData)
    {
        if (data != null) button.onClick.RemoveListener(TryBuyItem);
        data = newData;
        button.onClick.AddListener(TryBuyItem);
        name.text = data.name;
        cost.text = data.cost.ToString();
        description.text = data.description;
        soldOutPanel.SetActive(data.amount <= 0);
    }

    private void TryBuyItem()
    {
        if (Player.coins < data.cost)
        {
            UIManager.ShowMessage($"You don't have enough money to buy this item");
            return;
        }

        Player.ChangeCoins(-data.cost);
        data.OnBuy?.Invoke();
        data.amount--;
        soldOutPanel.SetActive(data.amount <= 0);
    }
}

public class PurchaseData
{
    public string name;
    public int cost;
    public string description;
    public int amount = 1;
    public Action OnBuy;
}