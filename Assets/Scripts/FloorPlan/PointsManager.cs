using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsManager : MonoBehaviour
{
    //average: 25/35
    
    public static int GetTotalPoints()
    {
        int finalValue = 0;
        foreach (var floorplan in GameManager.floorplanDict.Values)
            finalValue += floorplan.CalculatePoints();

        return finalValue;
    }
}
