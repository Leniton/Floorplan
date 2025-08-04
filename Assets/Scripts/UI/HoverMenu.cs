using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverMenu : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
{
    [SerializeField] private GameObject mainButton;
    [SerializeField] private HoverOptions hoverOptions;
    [SerializeField] private HoverOptions subOptions;

    private HoverButton lastButton;

    private void Start()
    {
        hoverOptions.SetupOptions(new()
        {
            null,
            null,
            null,
        });
        hoverOptions.ChangeOptionsVisibility(false);
        for (int i = 0; i < hoverOptions.optionsButton.Count; i++)
            hoverOptions.optionsButton[i].HoverOptions = subOptions;
        mainButton.transform.SetAsLastSibling();

        //test
        hoverOptions.optionsButton[0].gameObject.name = "option 1";
        hoverOptions.optionsButton[0].AddOption(new() { onPick = () => Debug.Log("0 => 0") });
        hoverOptions.optionsButton[0].AddOption(new() { onPick = () => Debug.Log("0 => 1") });
        hoverOptions.optionsButton[0].AddOption(new() { onPick = () => Debug.Log("0 => 2") });
        hoverOptions.optionsButton[1].gameObject.name = "option 2";
        hoverOptions.optionsButton[1].AddOption(new() { onPick = () => Debug.Log("1 => 0") });
        hoverOptions.optionsButton[1].gameObject.name = "option 3";
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!ReferenceEquals(eventData.pointerCurrentRaycast.gameObject, mainButton)) return;
        SetHoverButtonsInteractable(true);
        hoverOptions.ChangeOptionsVisibility(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //trickle down
        if (eventData.pointerCurrentRaycast.gameObject?.TryGetComponent<HoverButton>(out var button) ?? false)
            button.OnPointerUp(eventData);
        
        SetHoverButtonsInteractable(false);
        hoverOptions.ChangeOptionsVisibility(false);
    }

    private void SetHoverButtonsInteractable(bool value)
    {
        for (int i = 0; i < hoverOptions.optionsButton.Count; i++)
        {
            hoverOptions.optionsButton[i].SetInteractable(value);
            if(!value) hoverOptions.optionsButton[i].ResetButton();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var hoverButton = eventData.pointerCurrentRaycast.gameObject?.GetComponent<HoverButton>();
        if (ReferenceEquals(hoverButton, null)) return;
        if (ReferenceEquals(hoverButton, lastButton)) return;
        if (!hoverOptions.optionsButton.Contains(hoverButton)) return;
        for (int i = 0; i < hoverOptions.optionsButton.Count; i++)
            hoverOptions.optionsButton[i].ResetButton();
        hoverButton.OnPointerEnter(eventData);
        lastButton = hoverButton;
    }
}