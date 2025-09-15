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
            description = "Add 2 keys to floorplan",
            overlayPattern = GameAssets.patterns[9],
            activationEffect = new Key(2).AddItemToFloorplan
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
            description = "Add 5 coins to floorplan",
            overlayPattern = GameAssets.patterns[73],
            activationEffect = new Coin(5).AddItemToFloorplan
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
            description = "Add a <b>Soda</b> to floorplan",
            overlayPattern = GameAssets.patterns[54],
            activationEffect = ItemUtilities.Soda.AddItemToFloorplan
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
            description = "Add 2 dices to floorplan",
            overlayPattern = GameAssets.patterns[14],
            activationEffect = new Dice(2).AddItemToFloorplan
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
            activationEffect = floorplan => floorplan.basePoints += 5
        };
    }
    /// <summary>
    /// Add category to floorplan
    /// </summary>
    public static Renovation Paint(FloorCategory? category = null)
    {
        var floorCategory = category ?? Helpers.RandomCategory();
        var categoryName = Helpers.CategoryName(floorCategory);
        return new()
        {
            name = $"{categoryName} Paint",
            description = $"Floorplan is also a {categoryName}",
            condition = floorplan => !floorplan.IsOfCategory(floorCategory),
            activationEffect = floorplan => floorplan.AddCategory(floorCategory)
        };
    }
    /// <summary>
    /// Adds copy of floorplan to pool
    /// </summary>
    public static Renovation WallMirror() => new()
    {
        name = "Wall Mirror",
        description = "Add a copy of the floorplan to the draft pool",
        activationEffect = floorplan => RunData.playerDeck.deck.Add(floorplan.CreateInstance(Vector2Int.up))
    };
    /// <summary>
    /// Adds an opening to floorplan
    /// </summary>
    public static Renovation NewDoor() => new()
    {
        name = "New Door",
        description = "Adds an opening to floorplan",
        condition = floorplan => floorplan.Type != FloorType.Crossroad,
        activationEffect = floorplan => floorplan.OpenConnection()
    };
}

public class Renovation
{
    public string name;
    public string description;
    public Sprite overlayPattern;
    public Action<Floorplan> activationEffect;
    public bool persistent;
    public Func<Floorplan, bool> condition;

    public Renovation()
    {
        if (persistent) condition = NoRenovations;
    }

    private static bool NoRenovations(Floorplan floorplan) => floorplan.renovation != null;

    public void Activate(Floorplan floorplan)
    {
        if (!persistent)
        {
            activationEffect?.Invoke(floorplan);
            return;
        }

        floorplan.renovation = this;
    }
}