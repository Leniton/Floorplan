using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player
{
    private static Player player = new();

    #region Resources
    public static int steps;
    public static int keys;
    public static int coins = 500;
    public static int dices;
    public static bool activeSledgeHammer => currentSledgeHammer?.active ?? false;
    public static bool activeKey => currentKey != null;
    public static List<Item> items = new();
    private static SledgeHammer currentSledgeHammer;
    public static ColorKey currentKey;
    #endregion

    public static void ChangeSteps(int delta)
    {
        steps += delta;
        GameEvent.onStepsChanged?.Invoke(new(delta));
    }

    public static void ChangeCoins(int delta)
    {
        coins += delta;
        GameEvent.onCoinsChanged?.Invoke(new(delta));
    }

    public static void ChangeKeys(int delta)
    {
        keys += delta;
        GameEvent.onKeysChanged?.Invoke(new(delta));
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
        items = new();
        steps = 20;
        keys = 2;
        currentSledgeHammer = null;
        currentKey = null;
    }
}
