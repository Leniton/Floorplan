using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField, Range(0, 1)] private float distanceTreshold;
    [SerializeField] GameObject bg;
    [SerializeField] Transform verticalArrow;
    [SerializeField] Transform horizontalArrow;

    public Vector2 currentDirection;

    private int directionMultiplier = -1;

    private Pointer pointer => Pointer.current;
    private Vector2? startingSpot;
    private Vector2? direction;

    public event Action<Vector2Int> OnMove;

    public bool canMove { get; set; }

    private static Player player;

    #region Resources
    public static int steps;
    public static int keys;
    public static int coins;
    public static int dices;
    public static bool activeSledgeHammer => currentSledgeHammer?.active ?? false;
    private static SledgeHammer currentSledgeHammer;
    public static List<Item> items;
    #endregion

    private void Awake()
    {
        player = this;
        ResetPlayer();
    }

    private void Update()
    {
        HandleDragMovement();
        DisplayDirection();
#if UNITY_EDITOR
        directionMultiplier = 1;
        if (Keyboard.current.aKey.wasPressedThisFrame) Shift(Vector2Int.left);
        if (Keyboard.current.dKey.wasPressedThisFrame) Shift(Vector2Int.right);
        if (Keyboard.current.wKey.wasPressedThisFrame) Shift(Vector2Int.up);
        if (Keyboard.current.sKey.wasPressedThisFrame) Shift(Vector2Int.down);
#endif
    }

    private void HandleDragMovement()
    {
        if (ReferenceEquals(pointer, null)) return;
        if (!(pointer.press?.isPressed ?? false))
        {
            if (direction.HasValue)
            {
                bg.SetActive(false);
                Vector2 dir = direction.Value;
                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) Shift(new((int)Mathf.Sign(dir.x * directionMultiplier), 0));
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

    private void DisplayDirection()
    {
        bg.SetActive(startingSpot.HasValue);
        if (direction.HasValue)
        {
            Vector2 dir = direction.Value;
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) currentDirection = (new((int)Mathf.Sign(dir.x * directionMultiplier), 0));
            else currentDirection = (new(0, (int)Mathf.Sign(dir.y * directionMultiplier)));
        }
        else currentDirection = Vector2.zero;

        verticalArrow.localScale = Vector2.one * currentDirection.y;
        horizontalArrow.localScale = Vector2.one * currentDirection.x;
    }

    public static void ChangeSteps(int delta)
    {
        steps += delta;
    }

    public static void ChangeCoins(int delta)
    {
        coins += delta;
    }

    public static void ChangeKeys(int delta)
    {
        keys += delta;
    }

    public static void ActivateSledgeHammer(SledgeHammer sledgeHammer)
    {
        if(sledgeHammer == null) return;
        if(!items.Contains(sledgeHammer)) items.Add(sledgeHammer);
        currentSledgeHammer = sledgeHammer;
    }

    public static void ConsumeSledgeHammer()
    {
        items.Remove(currentSledgeHammer);
        currentSledgeHammer = null;
    }

    public static void ResetPlayer()
    {
        steps = 20;
        keys = 2;
        coins = 5;
        dices = 0;
        items = new();
        currentSledgeHammer = null;
    }
}
