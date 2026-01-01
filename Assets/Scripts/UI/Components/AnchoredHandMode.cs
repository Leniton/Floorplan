using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AnchoredHandMode : HandModeCallback
{
    private RectTransform rectTransform;
    /// <summary>
    /// anchoredPosition on rightHandMode
    /// </summary>
    private Vector2 defaultAnchoredPosition;
    private Vector2 defaultAnchorMin;
    private Vector2 defaultAnchorMax;
    private Vector2 defaultPivot;
    private Vector2 defaultSize;

    protected override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
        defaultAnchorMin = rectTransform.anchorMin;
        defaultAnchorMax = rectTransform.anchorMax;
        defaultPivot = rectTransform.pivot;
        defaultAnchoredPosition = rectTransform.anchoredPosition;
        defaultSize = rectTransform.sizeDelta;
    }

    public override void OnHandModeChanged(HandModeType mode)
    {
        Vector2 newAnchorMin = defaultAnchorMin;
        Vector2 newAnchorMax = defaultAnchorMax;
        Vector2 newPivot = defaultPivot;
        Vector2 newAnchoredPosition = defaultAnchoredPosition;

        if (mode == HandModeType.Left)
        {
            if (newAnchorMin == newAnchorMax)
            {
                newAnchorMin.x = 1 - newAnchorMin.x;
                newAnchorMax.x = 1 - newAnchorMax.x;
            }

            newPivot.x = 1 - newPivot.x;
            newAnchoredPosition.x = -defaultAnchoredPosition.x;
        }
        
        rectTransform.anchorMin = newAnchorMin;
        rectTransform.anchorMax = newAnchorMax;
        rectTransform.pivot = newPivot;
        rectTransform.anchoredPosition = newAnchoredPosition;
        //rectTransform.sizeDelta = newSize;
    }
}
