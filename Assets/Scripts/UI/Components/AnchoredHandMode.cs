using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AnchoredHandMode : HandModeCallback
{
    private RectTransform rectTransform;
    /// <summary>
    /// position on rightHandMode
    /// </summary>
    private Vector2 defaultPosition;
    private Vector2 defaultAnchorMin;
    private Vector2 defaultAnchorMax;
    private Vector2 defaultPivot;
    private Vector2 defaultSize;

    protected override void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        defaultAnchorMin = rectTransform.anchorMin;
        defaultAnchorMax = rectTransform.anchorMax;
        defaultPivot = rectTransform.pivot;
        defaultPosition = rectTransform.localPosition;
        defaultSize = rectTransform.sizeDelta;
        base.Awake();
    }

    public override void OnHandModeChanged(HandModeType mode)
    {
        Vector2 newAnchorMin = defaultAnchorMin;
        Vector2 newAnchorMax = defaultAnchorMax;
        Vector2 newPivot = defaultPivot;
        Vector2 newPosition = defaultPosition;

        if (mode == HandModeType.Left)
        {
            if (newAnchorMin == newAnchorMax)
            {
                newAnchorMin.x = 1 - newAnchorMin.x;
                newAnchorMax.x = 1 - newAnchorMax.x;
            }

            newPivot.x = 1 - newPivot.x;
            newPosition.x = -defaultPosition.x;
        }
        
        rectTransform.anchorMin = newAnchorMin;
        rectTransform.anchorMax = newAnchorMax;
        rectTransform.pivot = newPivot;
        rectTransform.localPosition = newPosition;
        //rectTransform.sizeDelta = newSize;
    }
}
