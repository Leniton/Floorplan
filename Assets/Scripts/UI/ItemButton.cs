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
        toggle.isOn = false;
        onClick?.Invoke();
    }

    public void Setup(Item item)
    {
        itemName.text = item.Name;
        if (item is not ToggleItem)
        {
            toggle.isOn = false;
            return;
        }
        ToggleItem toggleItem = (ToggleItem)item;
        toggle.isOn = toggleItem.active;
    }
}
