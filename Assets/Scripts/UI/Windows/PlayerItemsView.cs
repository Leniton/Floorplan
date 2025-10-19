using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerItemsView : MonoBehaviour
{
    [SerializeField] private TMP_Text textPrefab;
    [SerializeField] private RectTransform content;

    private List<TMP_Text> itemsList = new();

    public void Open()
    {
        SetupItems();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void SetupItems()
    {
        itemsList.EnsureEnoughInstances(textPrefab, Player.items.Count, content);
        for (int i = 0; i < Player.items.Count; i++)
            itemsList[i].text = Player.items[i].Name;
    }
}
