using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HandModeCallback : MonoBehaviour
{
    protected virtual void Awake()
    {
        HandMode.OnHandModeChanged += OnHandModeChanged;
    }
    
    public abstract void OnHandModeChanged(HandModeType mode);

    protected virtual void OnDestroy()
    {
        HandMode.OnHandModeChanged -= OnHandModeChanged;
    }
}
