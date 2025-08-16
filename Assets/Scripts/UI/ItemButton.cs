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
        if (item is not ToggleItem)
        {
            toggle.SetIsOnWithoutNotify(false);
            return;
        }
        ToggleItem toggleItem = (ToggleItem)item;
        toggle.SetIsOnWithoutNotify(toggleItem.active);
    }
}
