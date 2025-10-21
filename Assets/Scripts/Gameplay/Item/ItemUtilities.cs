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
    public static Food Soda => new(7) { Name = "Soda" };
    /// <summary>
    /// +10 steps
    /// </summary>
    public static Food Meal() => new(10) { Name = "Meal" };
    /// <summary>
    /// +1 step for each room drafted
    /// </summary>
    public static Food Snack()
    {
        Food snack = new(GameManager.roomDict.Count) { Name = "Snack" };

        GameEvent.onDraftedRoom += AddPoint;
        GameEvent.onCollectItem += OnConsume;

        void AddPoint(RoomEvent evt) => snack.stepsAmount++;
        void OnConsume(ItemEvent evt)
        {
            if (evt.item != snack) return;
            GameEvent.onDraftedRoom -= AddPoint;
            GameEvent.onCollectItem -= OnConsume;
        }

        return snack;
    }
    /// <summary>
    /// +2 steps for each Dead end drafted
    /// </summary>
    public static Food EnergyBar()
    {
        Food snack = new(0) { Name = "Energy bar" };

        foreach (var room in GameManager.roomDict.Values)
        {
            if (room.Type != RoomType.DeadEnd) continue;
            snack.stepsAmount += 2;
        }

        GameEvent.onDraftedRoom += AddPoint;
        GameEvent.onCollectItem += OnConsume;

        bool IsDeadEnd(RoomEvent evt) => evt.Room.Type == RoomType.DeadEnd;
        void AddPoint(RoomEvent evt)
        {
            if (!IsDeadEnd(evt)) return;
            snack.stepsAmount += 2;
        }
        void OnConsume(ItemEvent evt)
        {
            if (evt.item != snack) return;
            GameEvent.onDraftedRoom -= AddPoint;
            GameEvent.onCollectItem -= OnConsume;
        }

        return snack;
    }
    #endregion

    #region Decoration
    /// <summary>
    /// +2 points
    /// </summary>
    public static Decoration Rock(bool placed = false) => new(2, placed);
    /// <summary>
    /// +3 points
    /// </summary>
    public static Decoration Toy(bool placed = false) => new(3, placed);
    /// <summary>
    /// +4 points
    /// </summary>
    public static Decoration Couch(bool placed = false) => new(4, placed);
    /// <summary>
    /// +5 points
    /// </summary>
    public static Decoration TV(bool placed = false) => new(5, placed);
    /// <summary>
    /// +7 points
    /// </summary>
    public static Decoration Statue(bool placed = false) => new(activate: placed) { Name = "Statue", bonus = 7 };
    public static Decoration Treasure(bool placed = false) => new(activate: placed) { Name = "Treasure", bonus = 30 };
    #endregion
}
