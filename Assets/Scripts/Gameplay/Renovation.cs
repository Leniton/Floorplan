using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RenovationUtils
{
    /// <summary>
    /// Add 2 keys to floorplan
    /// </summary>
    public static Renovation KeyHolder()
    {
        return new()
        {
            persistent = true,
            name = "Key Holder",
            description = "Add 2 keys to room",
            overlayPattern = GameAssets.patterns[9],
            activationEffect = new Key(2).AddItemToRoom
        };
    }
    /// <summary>
    /// Add 5 coins to floorplan
    /// </summary>
    public static Renovation SecretVault()
    {
        return new()
        {
            persistent = true,
            name = "Secret Vault",
            description = "Add 5 coins to room",
            overlayPattern = GameAssets.patterns[73],
            activationEffect = new Coin(5).AddItemToRoom
        };
    }
    /// <summary>
    /// Add a <b>Soda</b> to floorplan
    /// </summary>
    public static Renovation MiniFridge()
    {
        return new()
        {
            persistent = true,
            name = "Mini-Fridge",
            description = "Add a <b>Soda</b> to room",
            overlayPattern = GameAssets.patterns[54],
            activationEffect = ItemUtilities.Soda.AddItemToRoom
        };
    }
    /// <summary>
    /// Add 2 <b>Dices</b> to floorplan
    /// </summary>
    public static Renovation PlayTable()
    {
        return new()
        {
            persistent = true,
            name = "Play table",
            description = "Add 2 dices to room",
            overlayPattern = GameAssets.patterns[14],
            activationEffect = new Dice(2).AddItemToRoom
        };
    }
    /// <summary>
    /// +5 base points
    /// </summary>
    public static Renovation Wallpaper()
    {
        return new()
        {
            name = "Wallpaper",
            description = "+5 base points",
            activationEffect = room => room.basePoints += 5
        };
    }
    /// <summary>
    /// Add category to floorplan
    /// </summary>
    public static Renovation Paint(RoomCategory? category = null)
    {
        var roomCategory = category ?? Helpers.RandomCategory();
        var categoryName = Helpers.CategoryName(roomCategory);
        return new()
        {
            name = $"{categoryName} Paint",
            description = $"Room is also a {categoryName}",
            condition = room => !room.IsOfCategory(roomCategory),
            activationEffect = room => room.AddCategory(roomCategory)
        };
    }
    /// <summary>
    /// Adds copy of floorplan to pool
    /// </summary>
    public static Renovation WallMirror() => new()
    {
        name = "Wall Mirror",
        description = "Add a copy of the room to the draft pool",
        activationEffect = room => RunData.playerDeck.deck.Add(room.CreateInstance(Vector2Int.up))
    };
    /// <summary>
    /// Adds an opening to floorplan
    /// </summary>
    public static Renovation NewDoor() => new()
    {
        name = "New Door",
        description = "Adds an opening to room",
        condition = floorplan => floorplan.Type != RoomType.Crossroad,
        activationEffect = room => room.OpenConnection()
    };
    /// <summary>
    /// Remove floorplan from deck.
    /// </summary>
    /// <returns></returns>
    public static Renovation Demolition() => new()
    {
        name = "Demolition",
        description = "Remove room from deck.",
        activationEffect = room => RunData.playerDeck.deck.Remove(room.FindOriginal(RunData.playerDeck.deck))
    };

    /// <summary>
    /// Close a door. remove key cost.
    /// </summary>
    /// <returns></returns>
    public static Renovation SealedDoor() => new()
    {
        name = "Sealed Door",
        description = "Close a door. remove key cost.",
        condition = room => room.Type != RoomType.DeadEnd,
        activationEffect = room =>
        {
            room.CloseConnection();
            room.keyCost = 0;
        }
    };
}

public class Renovation
{
    public string name;
    public string description;
    public Sprite overlayPattern;
    public Action<Room> activationEffect;
    public bool persistent;
    public Func<Room, bool> condition;

    public Renovation()
    {
        if (persistent) condition = NoRenovations;
    }

    private static bool NoRenovations(Room room) => room.renovation != null;

    public void Activate(Room flooroomplan)
    {
        if (!persistent)
        {
            activationEffect?.Invoke(flooroomplan);
            return;
        }

        flooroomplan.renovation = this;
    }
}