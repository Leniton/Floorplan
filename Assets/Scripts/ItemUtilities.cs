using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemUtilities
{
    public static Food Soda() => new() { Name = "Soda", stepsAmount = 7 };
    public static Food Meal() => new() { Name = "Meal", stepsAmount = 10 };
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

    public static Decoration Statue() => new() { Name = "Statue", bonus = 7 };
}
