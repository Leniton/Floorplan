using AddressableAsyncInstances;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// It has 2 modes: First one triggers an action when pointer is released on top of it; 
/// the other expands its layout group to show more options in the form of another hover button
/// </summary>
public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    [SerializeField] private Graphic graphic;
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
    private List<HoverOption> options = new();

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
        if(!interactable) return;
        if(options.Count <= 1) return;
        HoverOptions?.SetupOptions(options);
        ChangeOptionsVisibility(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable) return;
        if (options.Count > 1) return;
        print("pointer up");
    }

    public void SetInteractable(bool value)
    {
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

    public void AddOption(HoverOption option)
    {
        options.Add(option);
    }
}

public class HoverOption
{
    public string Name;
    public Action onPick;
}