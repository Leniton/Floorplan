using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "new Floorplan", menuName = "Floorplan/Floorplan")]
public class Floorplan : ScriptableObject
{
    public string Name;
    [TextArea] public string Description;

    public Color Color = Color.white;
    public FloorCategory Category;

    public FloorType Type = FloorType.DeadEnd;
    public Rarity Rarity;
    public int keyCost = 0;
    public int basePoints = 1;
    public int DoorCount => Mathf.Abs((int)Type);

    [HideInInspector] public int entranceId = 0;
    [HideInInspector] public bool[] connections;
    
    public Vector2Int coordinate { get; set; }
    public List<Item> items { get; set; } = new();

    public Floorplan original { get; private set; }

    public Action OnChanged;

    [HideInInspector] public List<Floorplan> connectedFloorplans;
    public Dictionary<string, Func<int>> pointBonus = new();
    public Dictionary<string, Func<int>> multBonus = new();

    public Action<CoordinateEvent> onDrafted = null;
    public Action<FloorplanConnectedEvent> onConnectToFloorplan = null;
    public Action<Event> onEnter = null;
    public Action<Event> onExit = null;

    public Floorplan CreateInstance(Vector2Int entranceDirection)
    {
        Floorplan floorplan = CreateInstance<Floorplan>();
        floorplan.original = this;
        floorplan.name = name;
        floorplan.Name = Name;
        floorplan.Description = Description;
        floorplan.Color = Color;
        floorplan.Category = Category;
        floorplan.Type = Type;
        floorplan.Rarity = Rarity;
        floorplan.keyCost = keyCost;
        floorplan.basePoints = basePoints;
        if (connections is { Length: > 0 })
        {
            //instance floorplan; copy floorplan connections and determine its type
            floorplan.connections = new[]
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
            floorplan.connections = new[]
            {
                true,
                Type != FloorType.DeadEnd && Type != FloorType.Straw,
                Type != FloorType.DeadEnd && Type != FloorType.Ankle,
                Type == FloorType.Crossroad,
            };
        }
        floorplan.connectedFloorplans = new(Mathf.Abs((int)floorplan.Type));
        floorplan.ChangeEntrance(entranceDirection);
        floorplan.Setup();
        
        return floorplan;
    }

    private void Setup()
    {
        connectedFloorplans = new(Mathf.Abs((int)Type));

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
        if (Type != FloorType.TPiece && Type != FloorType.Ankle) return;
        InternalRotation();
    }

    /// <summary>
    /// Used by the items so they place themselves on the floorplan
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(Item item)
    {
        items.Add(item);
        if (Helpers.CurrentFloorplan() != this ||
            !GameSettings.current.autoCollectItems ||
            item is PlaceableItem and { placed: true }) return;
        UIManager.ShowMessage($"found {item.Name}", () => PickupItem(item));
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
            UIManager.ShowMessage($"found {item.Name}", () => PickupItem(item));
        }
    }

    public void PickupItem(Item item)
    {
        if(!items.Contains(item)) return;
        item.PickUp();
        items.Remove(item);
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
public enum FloorType
{
    DeadEnd = 1,
    Straw = 2,
    Ankle = -2,
    TPiece = 3,
    Crossroad = 4,
}

[Flags]
public enum FloorCategory
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