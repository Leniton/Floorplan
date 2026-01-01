using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HandMode
{
    public static HandModeType currentHandModeType { get; private set; } = HandModeType.Right;
    public static event Action<HandModeType> OnHandModeChanged;

    public static void ChangeHandMode(HandModeType mode)
    {
        if (currentHandModeType == mode) return;
        currentHandModeType = mode;
        OnHandModeChanged?.Invoke(currentHandModeType);
    }

    public static void ToggleHandMode()
    {
        ChangeHandMode(currentHandModeType == HandModeType.Left ? HandModeType.Right : HandModeType.Left);
    }
}

public enum HandModeType
{
    Right = 0,
    Left = 1,
}