using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "new Floorplan", menuName = "Floorplan")]
public class Floorplan : ScriptableObject
{
    public string Name;
    [TextArea] public string Description;

    public Color Color = Color.white;
    public FloorCategory Category;

    public FloorType Type = FloorType.DeadEnd;
    public Rarity Rarity;
    public int basePoints = 1;
    public int DoorCount => Mathf.Abs((int)Type);

    [HideInInspector] public int entranceId = 0;
    [HideInInspector] public bool[] connections;

    public Floorplan original { get; private set; }

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
        floorplan.basePoints = basePoints;
        floorplan.connections = new bool[] 
        {
            true,
            Type != FloorType.DeadEnd && Type != FloorType.Straw,
            Type != FloorType.DeadEnd && Type != FloorType.Ankle,
            Type == FloorType.Crossroad,
        };

        StringBuilder sb = new();
        for (int i = 0; i < floorplan.connections.Length; i++)
            sb.Append($"{floorplan.connections[i]} | ");
        //Debug.Log(sb);

        floorplan.ChangeEntrance(entranceDirection);
        while (!floorplan.connections[floorplan.entranceId])
            floorplan.InternalRotation();
        //int entrance = floorplan.entranceId;
        //int randomRotation = Random.Range(1, 3);
        //for (int i = 0; i < randomRotation; i++)
        //    floorplan.InternalRotation();

        return floorplan;
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
    BlueRoom = 4,
    WhiteRoom = 8,
    Shop = 16,
    BlackRooms = 32,
    RedRooms = 64
}