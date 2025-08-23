using Lenix.NumberUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public static class Helpers
{
    #region General helpers
    /// <summary>
    /// Create nedded instances and adds to the list; also disables extra instances and enables the required ones
    /// </summary>
    public static void EnsureEnoughInstances<T>(this List<T> list, T prefab, int requiredInstances, Transform parent = null) where T : MonoBehaviour
    {
        if (list.Count < requiredInstances)
        {
            int diff = requiredInstances - list.Count;
            for (int i = 0; i < diff; i++)
                list.Add(GameObject.Instantiate(prefab, parent));
        }
        for (int i = 0; i < list.Count; i++)
            list[i].gameObject.SetActive(i < requiredInstances);
    }
    #endregion

    #region Floorplan helpers
    public static bool ConnectedToFloorplan(this Floorplan targetFloorplan, FloorplanConnectedEvent evt, out Floorplan other)
    {
        other = null;
        if(evt.baseFloorplan != targetFloorplan && evt.connectedFloorplan != targetFloorplan) return false;
        other = evt.baseFloorplan == targetFloorplan ? evt.connectedFloorplan : evt.baseFloorplan;
        return true;
    }

    public static Floorplan DraftedFrom(this Floorplan floorplan)
    {
        Vector2Int draftedCoordinates = floorplan.coordinate + Floorplan.IDToDirection(floorplan.entranceId);
        GameManager.floorplanDict.TryGetValue(draftedCoordinates, out var draftedFloorplan);
        return floorplan;
    }
    
    public static Floorplan CurrentFloorplan()
    {
        GameManager.floorplanDict.TryGetValue(GridManager.instance.currentPosition, out var current);
        return current;
    }

    public static void ConnectFloorplans(Floorplan baseFloorplan, Floorplan connectedFloorplan)
    {
        //the floorplan who's already there first
        //Debug.Log($"connecting {baseFloorplan.Name} to {connectedFloorplan.Name}");
        baseFloorplan.connectedFloorplans.Add(connectedFloorplan);
        baseFloorplan.onConnectToFloorplan?.Invoke(new(baseFloorplan, connectedFloorplan, connectedFloorplan.coordinate));
        connectedFloorplan.connectedFloorplans.Add(baseFloorplan);
        connectedFloorplan.onConnectToFloorplan?.Invoke(new(connectedFloorplan, baseFloorplan, baseFloorplan.coordinate));
        GameEvent.onConnectFloorplans?.Invoke(new(connectedFloorplan, baseFloorplan, baseFloorplan.coordinate));
    }

    public static RarityPicker<Item> ItemPool(this Floorplan floorplan)
    {
        RarityPicker<Item> possibleItems = new(.25f, .1f, .05f, 0);
        switch (floorplan.Category)
        {
            case FloorCategory.Shop:
                possibleItems.AddToPool(new Coin(), Rarity.Common);
                possibleItems.AddToPool(new Coin(5), Rarity.Uncommon);
                break;
            case FloorCategory.Hallway:
                possibleItems.AddToPool(new Coin(), Rarity.Common);
                possibleItems.AddToPool(new Decoration(), Rarity.Uncommon);
                break;
            case FloorCategory.RestRoom:
                possibleItems.AddToPool(new Food(), Rarity.Common);
                possibleItems.AddToPool(new Key(), Rarity.Common);
                possibleItems.AddToPool(new Dice(), Rarity.Uncommon);
                possibleItems.AddToPool(new Decoration(), Rarity.Uncommon);
                break;
            case FloorCategory.BlackRooms:
                possibleItems.AddToPool(new Key(), Rarity.Common);
                possibleItems.AddToPool(new Dice(), Rarity.Uncommon);
                possibleItems.AddToPool(new SledgeHammer(), Rarity.Rare);
                possibleItems.AddToPool(new Battery(), Rarity.Rare);
                break;
            case FloorCategory.WhiteRoom:
                possibleItems.AddToPool(new Food(), Rarity.Common);
                possibleItems.AddToPool(new Key(), Rarity.Common);
                possibleItems.AddToPool(new Dice(), Rarity.Uncommon);
                break;
            default://blue rooms
                possibleItems.AddToPool(new Coin(), Rarity.Common);
                possibleItems.AddToPool(new Food(), Rarity.Common);
                possibleItems.AddToPool(new Key(), Rarity.Common);
                possibleItems.AddToPool(new Dice(), Rarity.Uncommon);
                possibleItems.AddToPool(new Decoration(), Rarity.Uncommon);
                possibleItems.AddToPool(new SledgeHammer(), Rarity.Rare);
                possibleItems.AddToPool(new Battery(), Rarity.Rare);
                break;
        }

        return possibleItems;
    }

    /// <summary>
    /// Base check for if a floorplan will have an item
    /// </summary>
    /// <param name="forceItem">Guarantees an item will be added</param>
    public static void AddFloorplanItems(Floorplan floorplan, bool forceItem = false)
    {
        RarityPicker<Item> possibleItems = floorplan.ItemPool();
        //for items, legend means you get nothing
        possibleItems.allowEmptyResult = true;
        float nothingRate = possibleItems.commonRate + possibleItems.uncommonRate + possibleItems.rareRate;
        nothingRate = forceItem ? 0 : 1 - nothingRate;
        possibleItems.legendRate = nothingRate;

        //blue rooms are most likely to contain items
        if (floorplan.IsOfCategory(FloorCategory.BlueRoom))
        {
            float cutRate = possibleItems.legendRate / 2f;
            float distributeRate = cutRate / 2f;//to be 3 when rare items are introduced
            possibleItems.ChangeRarities(
                possibleItems.commonRate + distributeRate,
                possibleItems.uncommonRate + distributeRate,
                possibleItems.rareRate + distributeRate,//no rare items yet
                possibleItems.legendRate - cutRate);
        }

        possibleItems.PickRandom()?.AddItemToFloorplan(floorplan);
    }

    public static void AddItemToFloorplan(this Item item, Floorplan floorplan) => item.Place(floorplan);

    public static void OpenConnection(this Floorplan floorplan, int connectionID)
    {
        if (floorplan.connections[connectionID]) return;//already open
        //Debug.Log($"create opening on {floorplan.name}");
        floorplan.connections[connectionID] = true;
        floorplan.UpdateFloorplanType();
        floorplan.OnChanged?.Invoke();
        if (!GameManager.floorplanDict.TryGetValue(floorplan.coordinate + Floorplan.IDToDirection(connectionID), out var targetFloorplan)) return;
        if (!targetFloorplan.connections[(connectionID + 2) % 4]) return;
        //Debug.Log($"{floorplan.Name} now connected to {targetFloorplan.Name}");
        ConnectFloorplans(floorplan, targetFloorplan);
    }

    private static void UpdateFloorplanType(this Floorplan floorplan)
    {
        var connections = floorplan.connections;
        //change floorplan type
        int connectionCount = 0;
        for (int i = 0; i < connections.Length; i++)
            if (connections[i]) connectionCount++;

        if (connectionCount == 4) floorplan.Type = FloorType.Crossroad;
        else if (connectionCount == 3) floorplan.Type = FloorType.TPiece;
        else if (connections[(floorplan.entranceId + 2) % 4]) floorplan.Type = FloorType.Straw;
        else if (connectionCount > 1) floorplan.Type = FloorType.Ankle;
        else floorplan.Type = FloorType.DeadEnd;
    }

    public static bool IsOfCategory(this Floorplan floorplan, FloorCategory category) => NumberUtil.ContainsBytes((int)floorplan.Category, (int)category);

    public static void IncreaseChanceOfDrawing(this DrawFloorplanEvent evt, Func<Floorplan, bool> condition, float chance = .4f)
    {
        int possiblesFloorplans = 0;
        RarityPicker<Floorplan> picker = new(
            evt.floorplanPicker.commonRate,
            evt.floorplanPicker.uncommonRate,
            evt.floorplanPicker.rareRate,
            evt.floorplanPicker.legendRate);

        for (int i = 0; i < evt.possibleFloorplans.Count; i++)
        {
            Floorplan targetFloorplan = evt.possibleFloorplans[i];
            if (!condition.Invoke(evt.possibleFloorplans[i])) continue;
            bool alreadyDrawn = false;
            for (int f = 0; f < evt.drawnFloorplans.Length; f++)
            {
                if (!ReferenceEquals(evt.drawnFloorplans[f], targetFloorplan)) continue;
                alreadyDrawn = true;
                break;
            }
            if (alreadyDrawn) continue;
            picker.AddToPool(evt.possibleFloorplans[i], evt.possibleFloorplans[i].Rarity);
            possiblesFloorplans++;
        }

        if (possiblesFloorplans <= 0) return; //there's no floorplan that fits the criteria

        float r = Random.Range(0f, 1f);
        if (r <= chance && !condition.Invoke(evt.possibleFloorplans[^1])) 
        {
            evt.drawnFloorplans[^1] = picker.PickRandom(picker.commonRate, true);
            possiblesFloorplans--;
            //Debug.Log($"chance hit: changed to {evt.drawnFloorplans[^1].Name}");
        }
        if (possiblesFloorplans <= 0) return;
        for (int i = evt.drawnFloorplans.Length - 2; i >= 0; i--)
        {
            r = Random.Range(0f, 1f);
            if (r > chance || condition.Invoke(evt.possibleFloorplans[^1])) continue;
            evt.drawnFloorplans[i] = picker.PickRandom(removeFromPool: true);
            //Debug.Log($"chance hit: changed to {evt.drawnFloorplans[i].Name}");
            possiblesFloorplans--;
            if(possiblesFloorplans <= 0) break;
        }
    }
    #endregion
}
