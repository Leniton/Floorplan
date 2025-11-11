using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PointsManager
{
    //average: 25/35
    private const int baseRequirement = 40;
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
        foreach (var pair in GameManager.roomDict)
        {
            var floorplan = pair.Value;
            if (!GridManager.instance.ValidCoordinate(pair.Key)) continue;
            finalValue += floorplan.CalculatePoints();
        }

        return finalValue;
    }

    public static List<BonusData> GetHouseBonuses()
    {
        #region BonusFields
        //rest room bonus
        int restBonusId = -1;
        bool restBonusActive = false;
        //hallway bonus
        int hallwayBonusId = -1;
        #endregion
        
        List<BonusData> bonuses = new();
        FullHouseBonus();
        foreach (var pair in GameManager.roomDict)
        {
            RestBonus(pair);
            HallwayBonus(pair);
        }
        
        return bonuses;
        
        void FullHouseBonus()
        {
            int size = GridManager.xSize * GridManager.ySize;
            for (int i = 0; i < size; i++)
                if (!GameManager.roomDict.ContainsKey(new(i % GridManager.xSize, i / GridManager.xSize))) 
                    return;
            bonuses.Add(new("Full House", 10));
        }

        void RestBonus(KeyValuePair<Vector2Int, Room> pair)
        {
            if(!pair.Value.IsOfCategory(RoomCategory.RestRoom)) return;
            restBonusActive = !restBonusActive;
            if (restBonusActive) return;
            if (restBonusId < 0)
            {
                restBonusId = bonuses.Count;
                bonuses.Add(new("Rest Bonus", 0));
            }
            bonuses[restBonusId].amount += 5;
        }

        void HallwayBonus(KeyValuePair<Vector2Int, Room> pair)
        {
            if (hallwayBonusId < 0)
            {
                hallwayBonusId = bonuses.Count;
                bonuses.Add(new("Hallway Bonus", 0));
            }
            var room = pair.Value;
            for (int i = 0; i < room.connectedRooms.Count; i++)
            {
                if(!room.connectedRooms[i].IsOfCategory(RoomCategory.Hallway)) continue;
                bonuses[hallwayBonusId].amount++;
            }
        }
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