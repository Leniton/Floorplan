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
            overlayPattern = null,
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
            overlayPattern = null,
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
            overlayPattern = null,
            activationEffect = ItemUtilities.Soda.AddItemToFloorplan
        };
    }
    /// <summary>
    /// +5 base points
    /// </summary>
    public static Renovation Wallpaper()
    {
        return new()
        {
            persistent = true,
            name = "Wallpaper",
            description = "+5 base points",
            overlayPattern = null,
            activationEffect = floorplan => floorplan.basePoints += 5
        };
    }
    /// <summary>
    /// Add category to floorplan
    /// </summary>
    public static Renovation Paint(FloorCategory category)
    {
        return new()
        {
            name = "Paint",
            description = $"Floorplan is also a {Helpers.CategoryName(category)}",
            overlayPattern = null,
            condition = floorplan => !floorplan.IsOfCategory(category),
            activationEffect = floorplan => floorplan.Category |= category
        };
    }
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