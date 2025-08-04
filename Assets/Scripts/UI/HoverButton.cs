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
    [SerializeField] private LenixSOLayoutGroup optionsExpansion;

    private RectTransform rectTransform;
    private RectTransform layoutRectTransform;

    private HoverButton prefab;
    private List<HoverButton> optionsButton = new();
    private List<HoverOption> options = new();
    private bool interactable = true;

    private float openOffset;
    private float openSpacing;
    private bool expand = true;
    private Coroutine moveCoroutine;
    private bool moving => moveCoroutine != null;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Addressables.LoadAssetAsync<GameObject>("HoverButton").Completed += (o) => prefab = o.Result.GetComponent<HoverButton>();
        if (ReferenceEquals(optionsExpansion, null)) return;
        layoutRectTransform = optionsExpansion.GetComponent<RectTransform>();
        openOffset = optionsExpansion.offset;
        openSpacing = optionsExpansion.spacing;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!interactable) return;
        ChangeOptionsVisibility(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable) return;
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
        if (expand == value) return;
        expand = value;
        optionsExpansion.gameObject.SetActive(true);
        layoutRectTransform.anchoredPosition = rectTransform.anchoredPosition;
        if (!moving)
            moveCoroutine = StartCoroutine(MoveAnimation());
    }

    public void ResetButton()
    {
        expand = false;
        StopAllCoroutines();
        moveCoroutine = null;
    }

    public void AddOption(HoverOption option)
    {
        options.Add(option);
    }

    private IEnumerator MoveAnimation()
    {
        const float duration = .08f;
        const float stepDelay = 0.01f;
        WaitForSeconds delay = new WaitForSeconds(stepDelay);

        bool expandLayout = !expand;
        while (expandLayout != expand)
        {
            expandLayout = expand;
            float targetSpacing = expandLayout ? openSpacing : 0;
            float initialSpacing = Mathf.Abs(targetSpacing - openSpacing);
            float targetOffset = expandLayout ? openOffset : 0;
            float initialOffset = Mathf.Abs(targetOffset - openOffset);
            float time = 0;
            while (time < duration)
            {
                float scaledTime = time / duration;
                optionsExpansion.offset = Mathf.Lerp(initialOffset, targetOffset, scaledTime);
                optionsExpansion.spacing = Mathf.Lerp(initialSpacing, targetSpacing, scaledTime);
                optionsExpansion.AdjustElements();
                yield return delay;
                time += stepDelay;
            }
            optionsExpansion.offset = targetOffset;
            optionsExpansion.spacing = targetSpacing;
            optionsExpansion.AdjustElements();
        }
        if (!expand) optionsExpansion.gameObject.SetActive(false);
        moveCoroutine = null;
    }
}

public class HoverOption
{
    public string Name;
    public Action onPick;
}