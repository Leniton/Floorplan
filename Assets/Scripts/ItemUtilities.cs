using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemUtilities
{
    #region Food
    /// <summary>
    /// +2 steps
    /// </summary>
    public static Food Cherry => new Food(2);
    /// <summary>
    /// +3 steps
    /// </summary>
    public static Food Apple => new Food(3);
    /// <summary>
    /// +4 steps
    /// </summary>
    public static Food Banana => new Food(4);
    /// <summary>
    /// +5 steps
    /// </summary>
    public static Food Orange => new Food(5);
    /// <summary>
    /// +7 steps
    /// </summary>
    public static Food Soda => new() { Name = "Soda", stepsAmount = 7 };
    /// <summary>
    /// +10 steps
    /// </summary>
    public static Food Meal() => new() { Name = "Meal", stepsAmount = 10 };
    /// <summary>
    /// +1 step for each floorplan drafted
    /// </summary>
    public static Food Snack()
    {
        Food snack = new() { Name = "Snack", stepsAmount = GameManager.floorplanDict.Count };

        GameEvent.onDraftedFloorplan += AddPoint;
        GameEvent.OnCollectItem += OnConsume;

        void AddPoint(FloorplanEvent evt) => snack.stepsAmount++;
        void OnConsume(ItemEvent evt)
        {
            if (evt.item != snack) return;
            GameEvent.onDraftedFloorplan -= AddPoint;
            GameEvent.OnCollectItem -= OnConsume;
        }

        return snack;
    }
    /// <summary>
    /// +2 steps for each Dead end drafted
    /// </summary>
    public static Food EnergyBar()
    {
        Food snack = new() { Name = "Energy bar", stepsAmount = 0 };

        foreach (var floorplan in GameManager.floorplanDict.Values)
        {
            if (floorplan.Type != FloorType.DeadEnd) continue;
            snack.stepsAmount += 2;
        }

        GameEvent.onDraftedFloorplan += AddPoint;
        GameEvent.OnCollectItem += OnConsume;

        bool IsDeadEnd(FloorplanEvent evt) => evt.Floorplan.Type == FloorType.DeadEnd;
        void AddPoint(FloorplanEvent evt)
        {
            if (!IsDeadEnd(evt)) return;
            snack.stepsAmount += 2;
        }
        void OnConsume(ItemEvent evt)
        {
            if (evt.item != snack) return;
            GameEvent.onDraftedFloorplan -= AddPoint;
            GameEvent.OnCollectItem -= OnConsume;
        }

        return snack;
    }
    #endregion

    #region Decoration
    public static Decoration Statue(bool placed = false) => new(activate: placed) { Name = "Statue", bonus = 7 };
    #endregion
}
