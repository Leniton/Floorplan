using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopWindow : MonoBehaviour
{
    [SerializeField] private ShopItem itemPrefab;
    [SerializeField] private GameObject window;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button ShopButton;

    private static ShopWindow instance;

    private List<PurchaseData> currentItems;
    private List<ShopItem> shopItems = new();

    private void Awake()
    {
        instance = this;
        Close();
        if (ReferenceEquals(ShopButton, null)) return;
        ShopButton.onClick.AddListener(Open);
    }

    public static void OpenShop(string title, List<PurchaseData> items)
    {
        SetupShop(title, items);
        instance.Open();
    }

    public static void SetupShop(string title, List<PurchaseData> items)
    {
        instance.currentItems = items;
        instance.titleText.text = title;
        if (ReferenceEquals(instance.ShopButton, null)) return;
        instance.ShopButton.gameObject.SetActive(true);
    }

    public static void CloseShop()
    {
        instance.Close();
        if (ReferenceEquals(instance.ShopButton, null)) return;
        instance.ShopButton.gameObject.SetActive(false);
    }

    private void Open()
    {
        window.SetActive(true);

        shopItems.EnsureEnoughInstances(itemPrefab, currentItems.Count, window.transform);
        for (int i = 0; i < currentItems.Count; i++)
        {
            shopItems[i].Setup(currentItems[i]);
        }
    }

    public void Close() => instance.window.SetActive(false);
}
