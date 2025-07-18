using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField, Range(0, 1)] private float distanceTreshold;

    private int directionMultiplier = -1;

    private Pointer pointer => Pointer.current;
    private Vector2? startingSpot;
    private Vector2? direction;

    public event Action<Vector2Int> OnMove;

    public bool canMove { get; set; }

    private void Update()
    {
        HandleDragMovement();

#if UNITY_EDITOR
        directionMultiplier = 1;
        if (Keyboard.current.aKey.wasPressedThisFrame) Shift(Vector2Int.left);
        if (Keyboard.current.dKey.wasPressedThisFrame) Shift(Vector2Int.right);
        if (Keyboard.current.wKey.wasPressedThisFrame) Shift(Vector2Int.up);
        if (Keyboard.current.sKey.wasPressedThisFrame) Shift(Vector2Int.down);
#endif
        if (Keyboard.current.numpadPlusKey.wasPressedThisFrame) Test(1);
        if (Keyboard.current.numpadMinusKey.wasPressedThisFrame) Test(-1);
    }

    int testValue = -1;
    private void Test(int delta)
    {
        testValue = ((testValue + delta + 4) % 4);
        int t = testValue + 1;
        Vector2Int test = Vector2Int.zero;
        test.x = (t % 2) - (t / 2);

        int x = (t) / 1;

        print($"{testValue} => {test} | ");
    }

    private void HandleDragMovement()
    {
        if (ReferenceEquals(pointer, null)) return;
        if (!(pointer.press?.isPressed ?? false))
        {
            if (direction.HasValue)
            {
                Vector2 dir = direction.Value;
                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) gridManager.ShiftSelection(new((int)Mathf.Sign(dir.x * directionMultiplier), 0));
                else Shift(new(0, (int)Mathf.Sign(dir.y * directionMultiplier)));
            }
            direction = null;
            startingSpot = null;
            return;
        }

        if (!canMove) return;
        //read position
        Vector2 pointerPosition = pointer.position.ReadValue();
        if (!startingSpot.HasValue)
        {
            startingSpot ??= pointerPosition;
            return;
        }

        Vector2 delta = pointerPosition - startingSpot.Value;
        float treshold = Mathf.Pow((Screen.width * distanceTreshold) / 2f, 2);

        if (delta.sqrMagnitude < treshold)
        {
            direction = null;
            return;
        }
        direction = delta;
    }

    private void Shift(Vector2Int direction)
    {
        OnMove?.Invoke(direction);
    }
}
