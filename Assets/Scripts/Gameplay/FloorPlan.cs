using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lenix.NumberUtilities;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "new Room", menuName = "Floorplan/Room")]
public class Room : InfoReference
{
    public string Alias;

    public RoomCategory Category;

    public RoomType Type = RoomType.DeadEnd;
    public Rarity Rarity;
    public int keyCost = 0;
    public int basePoints = 1;
    public int DoorCount => Mathf.Abs((int)Type);

    [HideInInspector] public int entranceId = 0;
    [HideInInspector] public bool[] connections;
    
    public Vector2Int coordinate { get; set; }
    public List<Item> items { get; set; } = new();

    public Room original { get; private set; }

    public Action OnChanged;
    
    //Renovation parameters
    public Renovation renovation;

    [HideInInspector] public List<Room> connectedRooms;
    public Dictionary<string, Func<int>> pointBonus = new();
    public Dictionary<string, Func<int>> multBonus = new();

    public Action<CoordinateEvent> onDrafted = null;
    public Action<RoomConnectedEvent> onConnectToRoom = null;
    public Action<Event> onEnter = null;
    public Action<Event> onExit = null;
    public Action<CategoryChangeEvent> onCategoryChanged = null;

    public Room CreateInstance(Vector2Int entranceDirection)
    {
        Room room = CreateInstance<Room>();
        room.original = this;
        room.name = name;
        room.Name = Name;
        room.Alias = Alias;
        room.Description = Description;
        room.Category = Category;
        room.Type = Type;
        room.Rarity = Rarity;
        room.keyCost = keyCost;
        room.basePoints = basePoints;
        room.references = references;
        if (connections is { Length: > 0 })
        {
            //instance floorplan; copy floorplan connections and determine its type
            room.connections = new[]
            {
                connections[0],
                connections[1],
                connections[2],
                connections[3],
            };
        }
        else
        {
            //default floorplan; connections based on type
            room.connections = new[]
            {
                true,
                Type != RoomType.DeadEnd && Type != RoomType.Straw,
                Type != RoomType.DeadEnd && Type != RoomType.Ankle,
                Type == RoomType.Crossroad,
            };
        }
        room.connectedRooms = new(Mathf.Abs((int)room.Type));
        room.ChangeEntrance(entranceDirection);
        room.Setup();
        room.renovation = renovation;
        
        return room;
    }

    private void Setup()
    {
        connectedRooms = new(Mathf.Abs((int)Type));

        //StringBuilder sb = new();
        //for (int i = 0; i < floorplan.connections.Length; i++)
        //    sb.Append($"{floorplan.connections[i]} | ");
        //Debug.Log(sb);

        while (!connections[entranceId])
            InternalRotation();

        onEnter += OnEnterFloorplan;
    }

    public void ChangeEntrance(Vector2Int entranceDirection) => entranceId = DirectionToID(entranceDirection);

    private void InternalRotation()
    {
        bool entranceValue = connections[entranceId];
        for (int i = 0; i < 3; i++)
        {
            int connection = (entranceId + i) % 4;
            //Debug.Log($"{connection} -> {openEntrances[connection]}");
            this.connections[connection] = this.connections[(connection + 1) % 4];
        }
        connections[(entranceId + 3) % 4] = entranceValue;
        //Debug.Log($"{currentEntrance} => {openEntrances[currentEntrance]}");
        //keep rotating if entrance is closed
        if (!connections[entranceId])
        {
            Rotate();
            return;
        }

        StringBuilder sb = new();
        for (int i = 0; i < connections.Length; i++)
            sb.Append($"{connections[i]} | ");
        //Debug.Log(sb);
    }

    public void Rotate()
    {
        if (Type != RoomType.TPiece && Type != RoomType.Ankle) return;
        InternalRotation();
        OnChanged?.Invoke();
    }

    /// <summary>
    /// Used by the items so they place themselves on the floorplan
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(Item item)
    {
        items.Add(item);
        if (Helpers.CurrentRoom() != this ||
            !GameSettings.current.autoCollectItems ||
            item is PlaceableItem and { placed: true }) return;
        MessageWindow.ShowMessage($"found {item.Name}", () => PickupItem(item));
    }

    private void OnEnterFloorplan(Event evt)
    {
        if (!GameSettings.current.autoCollectItems) return;
        int itemCount = items.Count;
        int skippedItems = 0;
        for (int i = 0; i < itemCount; i++)
        {
            Item item = items[i];
            if (item is PlaceableItem and { placed: true }) continue;
            MessageWindow.ShowMessage($"found {item.Name}", () => PickupItem(item));
        }
    }

    public void PickupItem(Item item)
    {
        if(!items.Contains(item)) return;
        items.Remove(item);
        item.PickUp();
    }

    public string AddBonus(string name, Func<int> bonus)
    {
        string key = name;
        int duplicateID = 1;
        while(pointBonus.ContainsKey(key)) key = $"{name} {++duplicateID}";
        pointBonus[key] = bonus;
        OnChanged?.Invoke();
        return key;
    }

    public void RemoveBonus(string name)
    {
        pointBonus.Remove(name);
        OnChanged?.Invoke();
    }

    public string AddMultiplier(string name, Func<int> bonus)
    {
        string key = name;
        int duplicateID = 1;
        while (multBonus.ContainsKey(key)) key = $"{name} {++duplicateID}";
        multBonus[key] = bonus;
        OnChanged?.Invoke();
        return key;
    }

    public void RemoveMultiplier(string name)
    {
        multBonus.Remove(name);
        OnChanged?.Invoke();
    }

    public int CalculatePoints()
    {
        int finalValue = basePoints;
        foreach(var point in pointBonus.Values)
            finalValue += point?.Invoke() ?? 0;
        foreach (var mult in multBonus.Values)
            finalValue *= mult?.Invoke() ?? 1;
        return finalValue;
    }

    public void AddCategory(RoomCategory category)
    {
        if(this.IsOfCategory(category)) return;
        Category |= category;
        OnChanged?.Invoke();
        onCategoryChanged?.Invoke(new(category, coordinate, this));
        GameEvent.onRoomCategoryChanged?.Invoke(new(category, coordinate, this));
    }

    public static int DirectionToID(Vector2Int direction)
    {
        if(direction == Vector2Int.up)
            return 0;
        if (direction == Vector2Int.right)
            return 1;
        if (direction == Vector2Int.down)
            return 2;
        if (direction == Vector2Int.left)
            return 3;

        return -1;
    }

    public static Vector2Int IDToDirection(int id)
    {
        switch (id)
        {
            case 0: return Vector2Int.up;
            case 1: return Vector2Int.right;
            case 2: return Vector2Int.down;
            case 3: return Vector2Int.left;
        }

        return Vector2Int.zero;
    }
}

public enum RoomType
{
    DeadEnd = 1,
    Straw = 2,
    Ankle = -2,
    TPiece = 3,
    Crossroad = 4,
}

[Flags]
public enum RoomCategory
{
    RestRoom = 1,
    Hallway = 2,
    StorageRoom = 4,
    FancyRoom = 8,
    Shop = 16,
    MysteryRoom = 32,
    CursedRoom = 64,
    Blank = 128 //added so Aquarium doesn't break Great Hall
}

public class CategoryChangeEvent : RoomEvent
{
    public RoomCategory category;

    public CategoryChangeEvent(RoomCategory newCategory, Vector2Int coordinates, Room floorplan) : base(
        coordinates, floorplan)
    {
        category = newCategory;
    }
}