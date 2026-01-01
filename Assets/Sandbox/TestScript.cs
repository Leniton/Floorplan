using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestScript : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.zKey.wasPressedThisFrame)
            HandMode.ChangeHandMode(HandModeType.Right);
        if (Keyboard.current.xKey.wasPressedThisFrame)
            HandMode.ChangeHandMode(HandModeType.Left);
    }
}
