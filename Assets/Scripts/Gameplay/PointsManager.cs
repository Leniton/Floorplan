using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PointsManager
{
    //average: 25/35
    private const int baseRequirement = 40;
    private const int requirementGrowth = 20;
    public static int currentRequirement { get; private set; }

    public static void ChangeRequirement(int newRequirement)
    {
        currentRequirement = newRequirement;
    }

    private static int pointBonus;

    public static void Progress()
    {
        currentRequirement += requirementGrowth;
    }

    public static void Reset()
    {
        currentRequirement = baseRequirement;
        pointBonus = 0;
    }
    public static void ResetBonus()
    {
        pointBonus = 0;
    }
    
    public static int GetTotalPoints()
    {
        int finalValue = 0;
        foreach (var pair in GameManager.roomDict)
        {
            var floorplan = pair.Value;
            if (!GridManager.instance.ValidCoordinate(pair.Key)) continue;
            finalValue += floorplan.CalculatePoints();
        }

        return finalValue + pointBonus;
    }

    public static void AddPoints(int points)
    {
        Debug.Log($"Adding {points} points");
        pointBonus += points;
    }
}

public class BonusData
{
    public string name;
    public int amount;

    public BonusData(string _name, int _amount)
    {
        name = _name;
        amount = _amount;
    }
}