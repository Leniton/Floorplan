using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverMenu : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
{
    [SerializeField] private LenixSOLayoutGroup layoutGroup;
    [SerializeField] private float expandRadius;

    private List<HoverButton> hoverButtons;

    private bool expand;
    private Coroutine moveCoroutine;
    private bool moving => moveCoroutine != null;

    private void Awake()
    {
        RectTransform[] elements = layoutGroup.GetEnabledElements();
        hoverButtons = new (elements.Length);
        for (int i = 0; i < elements.Length; i++)
            hoverButtons.Add(elements[i].GetComponent<HoverButton>());
    }

    private void Start()
    {
        moveCoroutine = StartCoroutine(MoveAnimation());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        expand = true;
        if (!moving)
            moveCoroutine = StartCoroutine(MoveAnimation());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //trickle down
        if (eventData.pointerCurrentRaycast.gameObject?.TryGetComponent<HoverButton>(out var button) ?? false)
        {
            button.OnPointerUp(eventData);
        }

        expand = false;
        if (!moving)
            moveCoroutine = StartCoroutine(MoveAnimation());
    }

    private IEnumerator MoveAnimation()
    {
        const float duration = .08f;

        bool expandLayout = !expand;
        SetHoverButtonsInteractable(false);
        while (expandLayout != expand)
        {
            expandLayout = expand;
            if (!expandLayout)
                for (int i = 0; i < hoverButtons.Count; i++)
                    hoverButtons[i].ChangeOptionsVisibility(false);

            float goal = expandLayout ? expandRadius : 0;
            float origin = Mathf.Abs(goal - expandRadius);
            float time = 0;
            while (time < duration)
            {
                float scaledTime = time / duration;
                layoutGroup.spacing = Mathf.Lerp(origin, goal, scaledTime);
                layoutGroup.AdjustElements();
                yield return null;
                time += Time.deltaTime;
            }

            layoutGroup.spacing = goal;
            layoutGroup.AdjustElements();
        }

        SetHoverButtonsInteractable(expandLayout);
        moveCoroutine = null;
    }

    private void SetHoverButtonsInteractable(bool value)
    {
        for (int i = 0; i < hoverButtons.Count; i++)
            hoverButtons[i].SetInteractable(value);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var hoverButton = eventData.pointerCurrentRaycast.gameObject?.GetComponent<HoverButton>();
        if (ReferenceEquals(hoverButton, null)) return;
        if (!hoverButtons.Contains(hoverButton)) return;
        for (int i = 0; i < hoverButtons.Count; i++)
        {
            if (ReferenceEquals(hoverButtons[i], hoverButton)) continue;
            hoverButtons[i]?.ResetButton();
        }
    }
}