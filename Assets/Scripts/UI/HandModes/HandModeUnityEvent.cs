using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HandModeUnityEvent : HandModeCallback
{
    /// <summary>
    /// Bool when changed to righ-hand mode
    /// </summary>
    public UnityEvent<bool> onHandModeChanged;
    public UnityEvent onRightHandMode;
    public UnityEvent onLeftHandMode;

    public override void OnHandModeChanged(HandModeType mode)
    {
        onHandModeChanged?.Invoke(mode == HandModeType.Right);
        if (mode == HandModeType.Right) onRightHandMode?.Invoke();
        else onLeftHandMode?.Invoke();
    }
}
