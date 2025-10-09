using Lenix.NumberUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Helpers
{
    #region General helpers
    /// <summary>
    /// Create nedded instances and adds to the list; also disables extra instances and enables the required ones
    /// </summary>
    public static void EnsureEnoughInstances<T>(this List<T> list, T prefab, int requiredInstances, Transform parent = null,
        Action<T> setupAction = null) where T : MonoBehaviour
    {
        if (list.Count < requiredInstances)
        {
            int diff = requiredInstances - list.Count;
            for (int i = 0; i < diff; i++)
            {
                T instance = GameObject.Instantiate(prefab, parent);
                setupAction?.Invoke(instance);
                list.Add(instance);
            }
        }
        for (int i = 0; i < list.Count; i++)
            list[i].gameObject.SetActive(i < requiredInstances);
    }
    #endregion

    #region Floorplan helpers

    /// <returns>Random room category</returns>
    public static FloorCategory RandomCategory() => (FloorCategory)Mathf.Pow(2, Random.Range(0, 7));
    public static string CategoryName(FloorCategory category) => category switch
    {
        FloorCategory.RestRoom => "Rest Room",
        FloorCategory.Hallway => "Hallway",
        FloorCategory.StorageRoom => "Storage Room",
        FloorCategory.FancyRoom => "Fancy Room",
        FloorCategory.Shop => "Shop",
        FloorCategory.MysteryRoom => "Mystery Room",
        FloorCategory.CursedRoom => "Cursed Room",
        _ => $"{category.ToString()}",
    };
    public static bool ConnectedToFloorplan(this Floorplan targetFloorplan, FloorplanConnectedEvent evt, out Floorplan other)
    {
        other = null;
        if(evt.baseFloorplan != targetFloorplan && evt.connectedFloorplan != targetFloorplan) return false;
        other = evt.baseFloorplan == targetFloorplan ? evt.connectedFloorplan : evt.baseFloorplan;
        return true;
    }
    public static Floorplan FindOriginal(this Floorplan floorplan, List<Floorplan> pool)
    {
        Floorplan originalFloorplan = floorplan;
        while (!pool.Contains(originalFloorplan))
        {
            if (ReferenceEquals(originalFloorplan, null))
            {
                Debug.LogWarning("Original floorplan not found!!");
                break;
            }
            originalFloorplan = originalFloorplan.original;
        }
        return originalFloorplan;
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

    public static RarityPicker<Func<Item>> ItemPool(this Floorplan floorplan)
    {
        RarityPicker<Func<Item>> possibleItems = new(.25f, .1f, .05f, 0);
        var categories = NumberUtil.SeparateBits((int)floorplan.Category);
        for (int i = 0; i < categories.Length; i++)
        {
            var category = (FloorCategory)categories[i];
            switch (category)
            {
                case FloorCategory.Shop:
                    possibleItems.AddToPool(() => new Coin(), Rarity.Common);
                    possibleItems.AddToPool(() => new Coin(5), Rarity.Uncommon);
                    possibleItems.AddToPool(() => new Decoration(), Rarity.Uncommon);
                    break;
                case FloorCategory.Hallway:
                    possibleItems.AddToPool(() => new Coin(), Rarity.Common);
                    possibleItems.AddToPool(() => new Key(), Rarity.Common);
                    possibleItems.AddToPool(() => new Dice(), Rarity.Uncommon);
                    possibleItems.AddToPool(() => new Decoration(), Rarity.Uncommon);
                    break;
                case FloorCategory.RestRoom:
                    possibleItems.AddToPool(() => new Food(), Rarity.Common);
                    possibleItems.AddToPool(() => new Key(), Rarity.Common);
                    possibleItems.AddToPool(() => new Dice(), Rarity.Uncommon);
                    break;
                case FloorCategory.MysteryRoom:
                    possibleItems.AddToPool(() => new Key(), Rarity.Common);
                    possibleItems.AddToPool(() => new Dice(), Rarity.Uncommon);
                    possibleItems.AddToPool(() => new CategoryWallpaper(), Rarity.Uncommon);
                    possibleItems.AddToPool(() => new SledgeHammer(), Rarity.Rare);
                    possibleItems.AddToPool(() => new Battery(), Rarity.Rare);
                    break;
                case FloorCategory.FancyRoom:
                    possibleItems.AddToPool(() => new Food(), Rarity.Common);
                    possibleItems.AddToPool(() => new Key(), Rarity.Common);
                    possibleItems.AddToPool(() => new Dice(), Rarity.Uncommon);
                    break;
                default: //storage rooms
                    possibleItems.AddToPool(() => new Coin(), Rarity.Common);
                    possibleItems.AddToPool(() => new Food(), Rarity.Common);
                    possibleItems.AddToPool(() => new Key(), Rarity.Common);
                    possibleItems.AddToPool(() => new Decoration(), Rarity.Common);
                    possibleItems.AddToPool(() => new Dice(), Rarity.Uncommon);
                    possibleItems.AddToPool(() => ItemUtilities.Soda, Rarity.Uncommon);
                    possibleItems.AddToPool(() => new ColorKey(), Rarity.Uncommon);
                    possibleItems.AddToPool(() => new SledgeHammer(), Rarity.Rare);
                    possibleItems.AddToPool(() => new Battery(), Rarity.Rare);
                    break;
            }
        }

        return possibleItems;
    }

    /// <summary>
    /// Base check for if a floorplan will have an item
    /// </summary>
    /// <param name="forceItem">Guarantees an item will be added</param>
    public static void AddFloorplanItems(Floorplan floorplan, bool forceItem = false)
    {
        RarityPicker<Func<Item>> possibleItems = floorplan.ItemPool();
        //for items, legend means you get nothing
        possibleItems.allowEmptyResult = true;
        float nothingRate = possibleItems.commonRate + possibleItems.uncommonRate + possibleItems.rareRate;
        nothingRate = forceItem ? 0 : 1 - nothingRate;
        possibleItems.legendRate = nothingRate;

        possibleItems.PickRandom()?.Invoke()?.AddItemToFloorplan(floorplan);
    }

    public static void AddItemToFloorplan(this Item item, Floorplan floorplan) => item.Place(floorplan);

    public static void OpenConnection(this Floorplan floorplan, int connectionID = -1)
    {
        if (connectionID < 0)
        {
            //open first found closed connection
            for (connectionID = 0; connectionID < floorplan.connections.Length-1; connectionID++)
                if (!floorplan.connections[connectionID]) break;
        }
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

    public static void CloseConnection(this Floorplan floorplan, int connectionID = -1)
    {
        if(floorplan.Type == FloorType.DeadEnd) return;
        if (connectionID < 0)
        {
            //close first found open connection
            for (connectionID = 0; connectionID < floorplan.connections.Length-1; connectionID++)
                if (floorplan.connections[connectionID]) break;
        }
        if (!floorplan.connections[connectionID]) return;//already closed
        floorplan.connections[connectionID] = false;
        floorplan.UpdateFloorplanType();
        floorplan.OnChanged?.Invoke();
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

    public static bool IsOfCategory(this Floorplan floorplan, FloorCategory category) =>
        NumberUtil.ContainsBytes((int)floorplan.Category, (int)category);

    public static void IncreaseChanceOfDrawing(this DrawFloorplanEvent evt, Func<Floorplan, bool> condition, float chance =  .4f,
        Func<DrawFloorplanEvent, Floorplan> spareRoomMethod = null)
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
            if (!condition.Invoke(targetFloorplan)) continue;
            bool alreadyDrawn = false;
            for (int f = 0; f < evt.drawnFloorplans.Length; f++)
            {
                if (!ReferenceEquals(evt.drawnFloorplans[f], targetFloorplan)) continue;
                alreadyDrawn = true;
                break;
            }
            if (alreadyDrawn) continue;
            picker.AddToPool(targetFloorplan, targetFloorplan.Rarity);
            possiblesFloorplans++;
        }

        if (possiblesFloorplans <= 0 && spareRoomMethod == null) return;
        
        if (possiblesFloorplans <= 0) picker.AddToPool(spareRoomMethod.Invoke(evt), Rarity.Common);
        float r = Random.Range(0f, 1f);
        if (r <= chance && !condition.Invoke(evt.drawnFloorplans[^1]))
        {
            evt.drawnFloorplans[^1] = picker.PickRandom(picker.commonRate, true);
            possiblesFloorplans--;
            //Debug.Log($"chance hit: changed to {evt.drawnFloorplans[^1].Name}");
        }

        if (possiblesFloorplans <= 0) picker.AddToPool(spareRoomMethod.Invoke(evt), Rarity.Common);
        for (int i = evt.drawnFloorplans.Length - 2; i >= 0; i--)
        {
            r = Random.Range(0f, 1f);
            if (r > chance || condition.Invoke(evt.drawnFloorplans[i])) continue;
            evt.drawnFloorplans[i] = picker.PickRandom(removeFromPool: true);
            //Debug.Log($"chance hit: changed to {evt.drawnFloorplans[i].Name}");
            possiblesFloorplans--;
            if (possiblesFloorplans > 0 || spareRoomMethod == null) continue;
            picker.AddToPool(spareRoomMethod.Invoke(evt), Rarity.Common);
        }
    }

    public static Func<DrawFloorplanEvent, Floorplan> CategorySpareRoom(FloorCategory category) => evt => CreateSpareRoom(new() { category }, evt.possibleFloorTypes);

    public static Floorplan CreateSpareRoom(List<FloorCategory> possibleCategories = null, List<FloorType> possibleTypes = null)
    {
        if (possibleTypes is not { Count: > 0 })
        {
            possibleTypes = new()
            {
                FloorType.DeadEnd,
                FloorType.Ankle,
                FloorType.Straw,
                FloorType.TPiece,
                FloorType.Crossroad,
            };
        }

        if (possibleCategories is not { Count: > 0 })
        {
            possibleCategories = new()
            {
                FloorCategory.CursedRoom,
                FloorCategory.FancyRoom,
                FloorCategory.Hallway,
                FloorCategory.MysteryRoom,
                FloorCategory.RestRoom,
                FloorCategory.Shop,
                FloorCategory.StorageRoom,
            };
        }

        var spareOriginal = ScriptableObject.CreateInstance<Floorplan>();
        spareOriginal.Name = "Spare Room";
        spareOriginal.Alias = "Spare Room";
        spareOriginal.Rarity = Rarity.Common;
        spareOriginal.basePoints = 1;
        spareOriginal.Type = possibleTypes[Random.Range(0, possibleTypes.Count)];
        spareOriginal.Category = possibleCategories[Random.Range(0, possibleCategories.Count)];

        var spareRoom = spareOriginal.CreateInstance(Vector2Int.up);
        switch (spareRoom.Category)
        {
            case FloorCategory.FancyRoom:
                spareRoom.basePoints = 15 - (3 * spareRoom.DoorCount);
                spareRoom.Description = "-";
                break;
            case FloorCategory.CursedRoom:
                spareRoom.basePoints = 25 - (5 * spareRoom.DoorCount);
                spareRoom.Description = $"When you draft this floorplan, lose {11 - (2 * spareRoom.DoorCount)} steps";
                break;
            case FloorCategory.RestRoom:
                spareRoom.Description = $"The first time you enter this floorplan, gain {15 - (3 * spareRoom.DoorCount)} steps";
                break;
            case FloorCategory.Shop:
                spareRoom.Description = $"{12 - (2 * spareRoom.DoorCount)} coins";
                break;
            case FloorCategory.StorageRoom:
                spareRoom.Description = $"{5 - spareRoom.DoorCount} items";
                break;
            case FloorCategory.Hallway:
                int pointBonus = 5 - spareRoom.DoorCount;
                spareRoom.Description = $"<b>Connected Rooms</b> gain {pointBonus} point{(pointBonus > 1 ? "s" : "")}";
                break;
            case FloorCategory.MysteryRoom:
                spareRoom.Description = $"Multiply this floorplan points by {6 - spareRoom.DoorCount}";
                break;
        }
        return spareRoom;
    }

    public static Floorplan CreateFloorplan(string name, string description, int basePoints, 
        FloorType type, FloorCategory category, int keyCost = 0,
        string alias = null, Rarity rarity = Rarity.Common, Vector2Int? entrance = null,
        Action<CoordinateEvent> onDraftEffect = null)
    {
        var original = ScriptableObject.CreateInstance<Floorplan>();
        original.name = name.Replace(" ", "");
        original.Name = name;
        original.Description = description;
        original.Type = type;
        original.Category = category;
        original.basePoints = basePoints;
        original.keyCost = keyCost;
        
        original.Alias = alias ?? name;
        original.Rarity = rarity;

        var instance = original.CreateInstance(entrance ?? Vector2Int.up);
        if (onDraftEffect != null) instance.EveryTime().FloorplanIsDrafted().Do(onDraftEffect);
        return instance;
    }

    public static List<FloorType> GetPossibleFloorType(Vector2Int coordinate, out List<Vector2Int> possibleSlots)
    {
        //get possible sides
        possibleSlots = new();
        //up
        Vector2Int targetCoordinate = coordinate + Vector2Int.up;
        if (GridManager.instance?.ValidCoordinate(targetCoordinate) ?? true) possibleSlots.Add(Vector2Int.up);
        //down
        targetCoordinate = coordinate + Vector2Int.down;
        if (GridManager.instance?.ValidCoordinate(targetCoordinate) ?? true) possibleSlots.Add(Vector2Int.down);
        //left
        targetCoordinate = coordinate + Vector2Int.left;
        if (GridManager.instance?.ValidCoordinate(targetCoordinate) ?? true) possibleSlots.Add(Vector2Int.left);
        //right
        targetCoordinate = coordinate + Vector2Int.right;
        if (GridManager.instance?.ValidCoordinate(targetCoordinate) ?? true) possibleSlots.Add(Vector2Int.right);
        
        List<FloorType> possibleTypes = new()
        {
            FloorType.DeadEnd,
            FloorType.Ankle,
            FloorType.Straw,
            FloorType.TPiece,
            FloorType.Crossroad,
        };

        if (possibleSlots.Count < 4) possibleTypes.Remove(FloorType.Crossroad);
        if (possibleSlots.Count < 3) possibleTypes.Remove(FloorType.TPiece);
        if (possibleSlots.Count == 2)
        {
            if (!possibleSlots.Contains(-possibleSlots[0]))
                possibleTypes.Remove(FloorType.Straw);
        }
        else possibleTypes.Remove(FloorType.Ankle);

        return possibleTypes;
    }
    public static void CorrectRotation(this Floorplan floorplan, List<Vector2Int> possibleSlots)
    {
        if (floorplan.Type != FloorType.Ankle && floorplan.Type != FloorType.TPiece) return;

        bool invalidConnection;
        do
        {
            invalidConnection = false;
            for (int i = 0; i < floorplan.connections.Length; i++)
            {
                if (!floorplan.connections[i]) continue;
                if (possibleSlots.Contains(Floorplan.IDToDirection(i))) continue;
                invalidConnection = true;
                break;
            }
            if (invalidConnection) floorplan.Rotate();
        } while (invalidConnection);
    }

    public static void GetHouseData(out List<Vector2Int> occupied, out List<Vector2Int> empty)
    {
        occupied = new();
        empty = new();
        int houseSize = GridManager.xSize * GridManager.ySize;
        for (int i = 0; i < houseSize; i++)
        {
            int x = i % GridManager.xSize;
            int y = i / GridManager.xSize;
            Vector2Int coordinate = new(x, y);
            if (!GridManager.instance?.ValidCoordinate(coordinate) ?? true) continue;
            if (GameManager.floorplanDict.ContainsKey(coordinate)) occupied.Add(coordinate);
            else empty.Add(coordinate);
        }
    }
    #endregion
}
