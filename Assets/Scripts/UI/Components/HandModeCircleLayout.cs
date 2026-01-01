using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleLayoutGroup))]
public class HandModeCircleLayout : HandModeCallback
{
    private CircleLayoutGroup circleLayoutGroup;
    private Rotation defaultRotation;
    
    protected override void Awake()
    {
        circleLayoutGroup = GetComponent<CircleLayoutGroup>();
        defaultRotation = circleLayoutGroup.rotation;
        base.Awake();
    }

    public override void OnHandModeChanged(HandModeType mode)
    {
        circleLayoutGroup.rotation = (Rotation)((int)defaultRotation * (mode == HandModeType.Right ? 1 : -1));
        circleLayoutGroup.AdjustElements();
    }
}
