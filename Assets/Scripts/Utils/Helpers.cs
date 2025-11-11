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

    public static string SignedValue(this int value) => $"{(value > 0 ? "+" : string.Empty)}{value}";
    #endregion

    #region Floorplan helpers

    /// <returns>Random room category</returns>
    public static RoomCategory RandomCategory() => (RoomCategory)Mathf.Pow(2, Random.Range(0, 7));
    public static string CategoryName(RoomCategory category) => category switch
    {
        RoomCategory.RestRoom => "Rest Room",
        RoomCategory.Hallway => "Hallway",
        RoomCategory.StorageRoom => "Storage Room",
        RoomCategory.FancyRoom => "Fancy Room",
        RoomCategory.Shop => "Shop",
        RoomCategory.MysteryRoom => "Mystery Room",
        RoomCategory.CursedRoom => "Cursed Room",
        _ => $"{category.ToString()}",
    };
    public static bool ConnectedToRoom(this Room targetFloorplan, RoomConnectedEvent evt, out Room other)
    {
        other = null;
        if(evt.baseRoom != targetFloorplan && evt.connectedRoom != targetFloorplan) return false;
        other = evt.baseRoom == targetFloorplan ? evt.connectedRoom : evt.baseRoom;
        return true;
    }
    public static Room FindOriginal(this Room room, List<Room> pool)
    {
        Room originalRoom = room;
        while (!pool.Contains(originalRoom))
        {
            if (ReferenceEquals(originalRoom, null))
            {
                Debug.LogWarning("Original floorplan not found!!");
                break;
            }
            originalRoom = originalRoom.original;
        }
        return originalRoom;
    }
    
    public static Room CurrentRoom()
    {
        GameManager.roomDict.TryGetValue(GridManager.instance.currentPosition, out var current);
        return current;
    }

    public static void ConnectRooms(Room baseRoom, Room connectedRoom)
    {
        //the floorplan who's already there first
        //Debug.Log($"connecting {baseFloorplan.Name} to {connectedFloorplan.Name}");
        baseRoom.connectedRooms.Add(connectedRoom);
        baseRoom.onConnectToRoom?.Invoke(new(baseRoom, connectedRoom, connectedRoom.coordinate));
        connectedRoom.connectedRooms.Add(baseRoom);
        connectedRoom.onConnectToRoom?.Invoke(new(connectedRoom, baseRoom, baseRoom.coordinate));
        GameEvent.onConnectRooms?.Invoke(new(connectedRoom, baseRoom, baseRoom.coordinate));
    }

    public static RarityPicker<Func<Item>> ItemPool(this Room floorplan)
    {
        RarityPicker<Func<Item>> possibleItems = new(.25f, .1f, .05f, 0);
        var categories = NumberUtil.SeparateBits((int)floorplan.Category);
        
        possibleItems.AddToPool(() => new Coin(), Rarity.Common);
        possibleItems.AddToPool(() => new Food(), Rarity.Common);
        possibleItems.AddToPool(() => new Key(), Rarity.Common);
        for (int i = 0; i < categories.Length; i++)
        {
            var category = (RoomCategory)categories[i];
            switch (category)
            {
                case RoomCategory.Shop:
                    possibleItems.AddToPool(() => new Coin(5), Rarity.Uncommon);
                    possibleItems.AddToPool(() => new Decoration(), Rarity.Uncommon);
                    break;
                case RoomCategory.Hallway:
                    possibleItems.AddToPool(() => new Dice(), Rarity.Uncommon);
                    break;
                case RoomCategory.RestRoom:
                    possibleItems.AddToPool(() => new Dice(), Rarity.Uncommon);
                    break;
                case RoomCategory.MysteryRoom:
                    possibleItems.AddToPool(() => new Dice(), Rarity.Uncommon);
                    possibleItems.AddToPool(() => new CategoryWallpaper(), Rarity.Uncommon);
                    possibleItems.AddToPool(() => new SledgeHammer(), Rarity.Rare);
                    possibleItems.AddToPool(() => new Battery(), Rarity.Rare);
                    break;
                case RoomCategory.StorageRoom:
                    possibleItems.AddToPool(() => new Decoration(), Rarity.Common);
                    possibleItems.AddToPool(() => new Dice(), Rarity.Uncommon);
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
    public static void AddRoomItems(Room room, bool forceItem = false)
    {
        RarityPicker<Func<Item>> possibleItems = room.ItemPool();
        //for items, legend means you get nothing
        possibleItems.allowEmptyResult = true;
        float nothingRate = possibleItems.commonRate + possibleItems.uncommonRate + possibleItems.rareRate;
        nothingRate = forceItem ? 0 : 1 - nothingRate;
        possibleItems.legendRate = nothingRate;

        possibleItems.PickRandom()?.Invoke()?.AddItemToRoom(room);
    }

    public static void AddItemToRoom(this Item item, Room room) => item.Place(room);

    public static void OpenConnection(this Room room, int connectionID = -1)
    {
        if (connectionID < 0)
        {
            //open first found closed connection
            for (connectionID = 0; connectionID < room.connections.Length-1; connectionID++)
                if (!room.connections[connectionID]) break;
        }
        if (room.connections[connectionID]) return;//already open
        //Debug.Log($"create opening on {floorplan.name}");
        room.connections[connectionID] = true;
        room.UpdateRoomType();
        room.OnChanged?.Invoke();
        if (!GameManager.roomDict.TryGetValue(room.coordinate + Room.IDToDirection(connectionID), out var targetRoom)) return;
        if (!targetRoom.connections[(connectionID + 2) % 4]) return;
        //Debug.Log($"{floorplan.Name} now connected to {targetFloorplan.Name}");
        ConnectRooms(room, targetRoom);
    }

    public static void CloseConnection(this Room room, int connectionID = -1)
    {
        if(room.Type == RoomType.DeadEnd) return;
        if (connectionID < 0)
        {
            //close first found open connection
            for (connectionID = 0; connectionID < room.connections.Length-1; connectionID++)
                if (room.connections[connectionID]) break;
        }
        if (!room.connections[connectionID]) return;//already closed
        room.connections[connectionID] = false;
        room.UpdateRoomType();
        room.OnChanged?.Invoke();
    }

    private static void UpdateRoomType(this Room floorplan)
    {
        var connections = floorplan.connections;
        //change floorplan type
        int connectionCount = 0;
        for (int i = 0; i < connections.Length; i++)
            if (connections[i]) connectionCount++;

        if (connectionCount == 4) floorplan.Type = RoomType.Crossroad;
        else if (connectionCount == 3) floorplan.Type = RoomType.TPiece;
        else if (connections[(floorplan.entranceId + 2) % 4]) floorplan.Type = RoomType.Straw;
        else if (connectionCount > 1) floorplan.Type = RoomType.Ankle;
        else floorplan.Type = RoomType.DeadEnd;
    }

    public static bool IsOfCategory(this Room room, RoomCategory category) =>
        NumberUtil.ContainsBytes((int)room.Category, (int)category);

    public static void IncreaseChanceOfDrawing(this DrawRoomEvent evt, Func<Room, bool> condition, float chance =  .4f,
        Func<DrawRoomEvent, Room> spareRoomMethod = null)
    {
        int possiblesRooms = 0;
        RarityPicker<Room> picker = new(
            evt.roomPicker.commonRate,
            evt.roomPicker.uncommonRate,
            evt.roomPicker.rareRate,
            evt.roomPicker.legendRate);

        for (int i = 0; i < evt.possibleRooms.Count; i++)
        {
            Room targetRoom = evt.possibleRooms[i];
            if (!condition.Invoke(targetRoom)) continue;
            bool alreadyDrawn = false;
            for (int f = 0; f < evt.drawnRooms.Length; f++)
            {
                if (!ReferenceEquals(evt.drawnRooms[f], targetRoom)) continue;
                alreadyDrawn = true;
                break;
            }
            if (alreadyDrawn) continue;
            picker.AddToPool(targetRoom, targetRoom.Rarity);
            possiblesRooms++;
        }

        if (possiblesRooms <= 0 && spareRoomMethod == null) return;
        
        if (possiblesRooms <= 0) picker.AddToPool(spareRoomMethod.Invoke(evt), Rarity.Common);
        float r = Random.Range(0f, 1f);
        if (r <= chance && !condition.Invoke(evt.drawnRooms[^1]))
        {
            evt.drawnRooms[^1] = picker.PickRandom(picker.commonRate, true);
            possiblesRooms--;
            //Debug.Log($"chance hit: changed to {evt.drawnFloorplans[^1].Name}");
        }

        if (possiblesRooms <= 0) picker.AddToPool(spareRoomMethod.Invoke(evt), Rarity.Common);
        for (int i = evt.drawnRooms.Length - 2; i >= 0; i--)
        {
            r = Random.Range(0f, 1f);
            if (r > chance || condition.Invoke(evt.drawnRooms[i])) continue;
            evt.drawnRooms[i] = picker.PickRandom(removeFromPool: true);
            //Debug.Log($"chance hit: changed to {evt.drawnFloorplans[i].Name}");
            possiblesRooms--;
            if (possiblesRooms > 0 || spareRoomMethod == null) continue;
            picker.AddToPool(spareRoomMethod.Invoke(evt), Rarity.Common);
        }
    }

    public static Func<DrawRoomEvent, Room> CategorySpareRoom(RoomCategory category) => evt => CreateSpareRoom(new() { category }, evt.possibleFloorTypes);

    public static Room CreateSpareRoom(List<RoomCategory> possibleCategories = null, List<RoomType> possibleTypes = null)
    {
        if (possibleTypes is not { Count: > 0 })
        {
            possibleTypes = new()
            {
                RoomType.DeadEnd,
                RoomType.Ankle,
                RoomType.Straw,
                RoomType.TPiece,
                RoomType.Crossroad,
            };
        }

        if (possibleCategories is not { Count: > 0 })
        {
            possibleCategories = new()
            {
                RoomCategory.CursedRoom,
                RoomCategory.FancyRoom,
                RoomCategory.Hallway,
                RoomCategory.MysteryRoom,
                RoomCategory.RestRoom,
                RoomCategory.Shop,
                RoomCategory.StorageRoom,
            };
        }

        var spareOriginal = ScriptableObject.CreateInstance<Room>();
        spareOriginal.Name = "Spare Room";
        spareOriginal.Alias = "Spare Room";
        spareOriginal.Rarity = Rarity.Common;
        spareOriginal.basePoints = 1;
        spareOriginal.Type = possibleTypes[Random.Range(0, possibleTypes.Count)];
        spareOriginal.Category = possibleCategories[Random.Range(0, possibleCategories.Count)];

        var spareRoom = spareOriginal.CreateInstance(Vector2Int.up);
        switch (spareRoom.Category)
        {
            case RoomCategory.FancyRoom:
                spareRoom.basePoints = 15 - (3 * spareRoom.DoorCount);
                spareRoom.Description = "-";
                break;
            case RoomCategory.CursedRoom:
                spareRoom.basePoints = 25 - (5 * spareRoom.DoorCount);
                spareRoom.Description = $"When you draft this floorplan, lose {11 - (2 * spareRoom.DoorCount)} steps";
                break;
            case RoomCategory.RestRoom:
                spareRoom.Description = $"The first time you enter this floorplan, gain {15 - (3 * spareRoom.DoorCount)} steps";
                break;
            case RoomCategory.Shop:
                spareRoom.Description = $"{12 - (2 * spareRoom.DoorCount)} coins";
                break;
            case RoomCategory.StorageRoom:
                spareRoom.Description = $"{5 - spareRoom.DoorCount} items";
                break;
            case RoomCategory.Hallway:
                int pointBonus = 5 - spareRoom.DoorCount;
                spareRoom.Description = $"<b>Connected Rooms</b> gain {pointBonus} point{(pointBonus > 1 ? "s" : "")}";
                break;
            case RoomCategory.MysteryRoom:
                spareRoom.Description = $"Multiply this floorplan points by {6 - spareRoom.DoorCount}";
                break;
        }
        return spareRoom;
    }

    public static Room CreateRoom(string name, string description, int basePoints, 
        RoomType type, RoomCategory category, int keyCost = 0,
        string alias = null, Rarity rarity = Rarity.Common, Vector2Int? entrance = null,
        Action<CoordinateEvent> onDraftEffect = null)
    {
        var original = ScriptableObject.CreateInstance<Room>();
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
        if (onDraftEffect != null) instance.EveryTime().RoomIsDrafted().Do(onDraftEffect);
        return instance;
    }

    public static List<RoomType> GetPossibleRoomTypes(Vector2Int coordinate, out List<Vector2Int> possibleSlots)
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
        
        List<RoomType> possibleTypes = new()
        {
            RoomType.DeadEnd,
            RoomType.Ankle,
            RoomType.Straw,
            RoomType.TPiece,
            RoomType.Crossroad,
        };

        if (possibleSlots.Count < 4) possibleTypes.Remove(RoomType.Crossroad);
        if (possibleSlots.Count < 3) possibleTypes.Remove(RoomType.TPiece);
        if (possibleSlots.Count == 2 && !possibleSlots.Contains(-possibleSlots[0]))
            possibleTypes.Remove(RoomType.Straw);
        else if(possibleSlots.Count <= 1) possibleTypes.Remove(RoomType.Ankle);

        return possibleTypes;
    }
    public static void CorrectRotation(this Room room, List<Vector2Int> possibleSlots)
    {
        if (room.Type != RoomType.Ankle && room.Type != RoomType.TPiece) return;

        bool invalidConnection;
        do
        {
            invalidConnection = false;
            for (int i = 0; i < room.connections.Length; i++)
            {
                if (!room.connections[i]) continue;
                if (possibleSlots.Contains(Room.IDToDirection(i))) continue;
                invalidConnection = true;
                break;
            }
            if (invalidConnection) room.Rotate();
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
            if (GameManager.roomDict.ContainsKey(coordinate)) occupied.Add(coordinate);
            else empty.Add(coordinate);
        }
    }
    #endregion
}
