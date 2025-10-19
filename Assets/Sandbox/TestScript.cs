using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestScript : MonoBehaviour
{
    public FloorplanUI floorplanUI;
    public Room floorplan;

    private void Awake()
    {
        floorplanUI.Setup(floorplan.CreateInstance(Vector2Int.down));
    }

    private void Update()
    {
        if(Keyboard.current.spaceKey.wasPressedThisFrame) 
            floorplanUI.Setup(floorplan.CreateInstance(Vector2Int.down));
    }
}
