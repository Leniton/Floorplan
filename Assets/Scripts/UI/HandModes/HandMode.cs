using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HandMode
{
    private static HandModeType currentHandModeType = HandModeType.Right;
    public static event Action<HandModeType> OnHandModeChanged;

    public static void ChangeHandMode(HandModeType mode)
    {
        if (currentHandModeType == mode) return;
        currentHandModeType = mode;
        OnHandModeChanged?.Invoke(currentHandModeType);
    }
}

public enum HandModeType
{
    Right = 0,
    Left = 1,
}