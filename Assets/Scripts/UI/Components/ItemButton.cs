using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private TMP_Text itemName;

    public Action onClick;

    private void Awake()
    {
        toggle.onValueChanged.AddListener(OnClick);
    }

    private void OnClick(bool value)
    {
        toggle.SetIsOnWithoutNotify(false);
        onClick?.Invoke();
    }

    public void Setup(Item item)
    {
        itemName.text = item.Name;
        if (item is ToggleItem)
        {
            ToggleItem toggleItem = (ToggleItem)item;
            itemName.text = $"({(toggleItem.active ? "A" : "a")}) {item.Name}";
            toggle.SetIsOnWithoutNotify(toggleItem.active);
            return;
        }
        if (item is PlaceableItem)
        {
            PlaceableItem placeableItem = (PlaceableItem)item;
            itemName.text = $"({(placeableItem.placed ? "P" : "p")}) {item.Name}";
            toggle.SetIsOnWithoutNotify(placeableItem.placed);
            return;
        }
        toggle.SetIsOnWithoutNotify(false);
    }
}
