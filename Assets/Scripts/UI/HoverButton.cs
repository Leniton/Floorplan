using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// It has 2 modes: First one triggers an action when pointer is released on top of it; 
/// the other expands its layout group to show more options in the form of another hover button
/// </summary>
public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    private bool interactable = true;
    [SerializeField] private Graphic graphic;

    public void OnPointerEnter(PointerEventData eventData)
    {
        print("pointer enter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        print("pointer exit");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        print("pointer up");
    }

    public void SetInteractable(bool value)
    {
        interactable = value;
        graphic.raycastTarget = value;
    }
}
