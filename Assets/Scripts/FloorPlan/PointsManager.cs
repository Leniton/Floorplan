using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PointsManager
{
    //average: 25/35
    private const int baseRequirement = 20;
    private const int requirementGrowth = 20;
    public static int currentRequirement { get; private set; } = 20;

    public static void ChangeRequirement(int newRequirement)
    {
        currentRequirement = newRequirement;
    }

    public static void Progress()
    {
        currentRequirement += requirementGrowth;
    }

    public static void Reset()
    {
        currentRequirement = baseRequirement;
    }
    
    public static int GetTotalPoints()
    {
        int finalValue = 0;
        foreach (var floorplan in GameManager.floorplanDict.Values)
            finalValue += floorplan.CalculatePoints();

        return finalValue;
    }
}
