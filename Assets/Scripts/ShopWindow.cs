using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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
        ShopButton.onClick.AddListener(Open);
        Close();
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
        instance.ShopButton.gameObject.SetActive(true);
    }

    public static void CloseShop()
    {
        instance.Close();
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

    private IEnumerator OpenSequence()
    {
        yield return EraseCurrentData();
        for (int i = 0; i < currentItems.Count; i++)
        {
            PurchaseData data = currentItems[i];
            ShopItem item = Instantiate(itemPrefab, window.transform);
            item.Setup(data);
        }
    }

    private IEnumerator EraseCurrentData()
    {
        //first is the title
        while (window.transform.childCount > 2)
        {
            Destroy(window.transform.GetChild(2).gameObject);
            yield return null;
        }
    }
}
