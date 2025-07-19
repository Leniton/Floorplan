using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "new Floorplan", menuName = "Floorplan")]
public class Floorplan : ScriptableObject
{
    public string Name;
    public string Description;

    public Color Color = Color.white;

    public FloorType Type;
    public int DoorCount => Mathf.Abs((int)Type);

    public int entranceId = 0;
    public bool[] connections;

    public Floorplan CreateInstance(Vector2Int entranceDirection)
    {
        Floorplan floorplan = CreateInstance<Floorplan>();
        floorplan.Name = Name;
        floorplan.Description = Description;
        floorplan.Color = Color;
        floorplan.Type = Type;
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

        floorplan.entranceId = DirectionToID(entranceDirection);
        int entrance = floorplan.entranceId;
        int randomRotation = Random.Range(1, 3);
        for (int i = 0; i < randomRotation; i++)
        {
            floorplan.Rotate();
        }

        return floorplan;
    }

    public void Rotate()
    {
        if (Type != FloorType.TPiece && Type != FloorType.Ankle) return;

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

    public int DirectionToID(Vector2Int direction)
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

    public Vector2Int IDToDirection(int id)
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