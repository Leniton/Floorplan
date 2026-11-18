using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PointsManager
{
    //average: 25/35
    private const int baseRequirement = 40;
    private const int requirementGrowth = 20;

    public const int deadline = 3;
    
    public static int currentRequirement { get; private set; }
    public static int day { get; private set; }

    public static void ChangeRequirement(int newRequirement)
    {
        currentRequirement = newRequirement;
    }

    private static int basePoints;
    private static int pointBonus;

    public static void AddDay() => day++;
    public static void Progress()
    {
        currentRequirement += requirementGrowth;
    }

    public static void Reset()
    {
        day = 0;
        currentRequirement = baseRequirement;
        basePoints = 0;
        pointBonus = 0;
    }
    public static void ResetBonus()
    {
        pointBonus = 0;
    }
    
    public static int GetTotalPoints()
    {
        int finalValue = GetHousePoints();
        return finalValue + basePoints + pointBonus;
    }

    private static int GetHousePoints()
    {
        int finalValue = 0;
        foreach (var pair in GameManager.roomDict)
        {
            var floorplan = pair.Value;
            if (!GridManager.instance.ValidCoordinate(pair.Key)) continue;
            finalValue += floorplan.CalculatePoints();
        }
        return finalValue;
    }

    public static void AddPoints(int points)
    {
        Debug.Log($"Adding {points} points");
        pointBonus += points;
    }

    public static void SavePoints(int? points = null)
    {
        basePoints = points ?? GetHousePoints();
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