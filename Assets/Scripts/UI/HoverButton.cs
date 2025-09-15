using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// It has 2 modes: First one triggers an action when pointer is released on top of it; 
/// the other expands its layout group to show more options in the form of another hover button
/// </summary>
public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerUpHandler
{
    [SerializeField] private Graphic graphic;
    [SerializeField] private Image icon;
    [SerializeField] private HoverOptions optionsExpansion;

    public HoverOptions HoverOptions
    {
        get => optionsExpansion;
        set
        {
            optionsExpansion = value;
            layoutRectTransform = optionsExpansion.rectTransform;
        }
    }
    private List<ButtonCallback> options = new();

    private RectTransform rectTransform;
    private RectTransform layoutRectTransform;
    private bool interactable = true;
    private bool open;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (ReferenceEquals(optionsExpansion, null)) return;
        layoutRectTransform = optionsExpansion.rectTransform;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;
        if (options.Count <= 1) return;
        HoverOptions?.SetupOptions(options);
        ChangeOptionsVisibility(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable) return;
        if (options.Count != 1) return;
        options[0].onPick?.Invoke();
    }

    public void SetInteractable(bool value)
    {
        value &= options.Count > 0;
        interactable = value;
        graphic.raycastTarget = value;
    }

    public void ChangeOptionsVisibility(bool value)
    {
        if (ReferenceEquals(optionsExpansion, null)) return;
        if(open == value) return;
        open = value;
        layoutRectTransform.anchoredPosition = rectTransform.anchoredPosition;
        HoverOptions?.ChangeOptionsVisibility(value);
    }

    public void ResetButton()
    {
        open = false;
        HoverOptions?.Hide();
    }

    public void SetOption(ButtonCallback option)
    {
        icon.gameObject.SetActive(!ReferenceEquals(option?.icon, null));
        if(option == null) return;
        options = new() { option };
        SetInteractable(true);
        if (!icon.gameObject.activeSelf) return;
        icon.sprite = option.icon;
        icon.color = option.color;
    }

    public void AddOption(ButtonCallback option)
    {
        SetInteractable(options.Count > 0);
        options.Add(option);
    }

    public void RemoveOption(ButtonCallback option)
    {
        options.Remove(option);
        SetInteractable(options.Count > 0);
    }
}

public class ButtonCallback
{
    public string Name;
    public Sprite icon;
    public Color color;
    public Action onPick;
}