using System.Collections.Generic;
using UnityEngine;

public class Player
{
    private static Player player = new();

    public static int minSteps;
    public static int minCoins;
    public static int minKeys;

    #region Resources
    public static int steps;
    public static int keys;
    public static int coins;
    public static int dices;
    public static bool activeSledgeHammer => currentSledgeHammer?.active ?? false;
    public static bool activeKey => currentKey != null;
    public static List<Item> items = new();
    private static SledgeHammer currentSledgeHammer;
    public static ColorKey currentKey;
    #endregion

    public static void ChangeSteps(int delta)
    {
        steps = Mathf.Max(steps + delta, minSteps);
        GameEvent.onStepsChanged?.Invoke(new(delta));
    }

    public static void ChangeCoins(int delta)
    {
        coins = Mathf.Max(coins + delta, minCoins);
        GameEvent.onCoinsChanged?.Invoke(new(delta));
    }

    public static void ChangeKeys(int delta)
    {
        keys = Mathf.Max(keys + delta, minKeys);
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
        minSteps = 0;
        minKeys = 0;
        minCoins = 0;
        items = new();
        steps = 20;
        keys = 2;
        currentSledgeHammer = null;
        currentKey = null;
    }
}
