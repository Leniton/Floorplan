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

    private event Action _tapAction;

    private HoverButton lastButton;

    private void Awake()
    {
        hoverOptions.OnDoneMoving += OnDoneMoving;
    }

    public void SetupOptions(List<ButtonCallback> options, Action tapAction = null)
    {
        _tapAction = tapAction;
        hoverOptions.SetupOptions(options);
        hoverOptions.ForceState(false);
        mainButton.transform.SetAsLastSibling();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!ReferenceEquals(eventData.pointerCurrentRaycast.gameObject, mainButton)) return;
        SetHoverButtonsInteractable(false);
        hoverOptions.ChangeOptionsVisibility(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //trickle down
        if (eventData.pointerCurrentRaycast.gameObject?.TryGetComponent<HoverButton>(out var button) ?? false)
            button.OnPointerUp(eventData);

        //self effect when releasing on itself
        if(eventData.pointerCurrentRaycast.gameObject == mainButton)
            _tapAction?.Invoke();
        
        SetHoverButtonsInteractable(false);
        hoverOptions.ChangeOptionsVisibility(false);
    }

    private void OnDoneMoving()
    {
        if(!hoverOptions.expand) return;
        SetHoverButtonsInteractable(true);
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