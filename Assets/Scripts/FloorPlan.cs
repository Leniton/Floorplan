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

    public int currentEntrance = 0;
    public bool[] connection;

    public Floorplan CreateInstance(Vector2Int entranceDirection)
    {
        Floorplan floorplan = CreateInstance<Floorplan>();
        floorplan.Name = Name;
        floorplan.Description = Description;
        floorplan.Color = Color;
        floorplan.Type = Type;
        floorplan.connection = new bool[] 
        {
            true,
            Type != FloorType.DeadEnd && Type != FloorType.Straw,
            Type != FloorType.DeadEnd && Type != FloorType.Ankle,
            Type == FloorType.Crossroad,
        };

        StringBuilder sb = new();
        for (int i = 0; i < floorplan.connection.Length; i++)
            sb.Append($"{floorplan.connection[i]} | ");
        Debug.Log(sb);

        floorplan.currentEntrance = DirectionToID(entranceDirection);
        int entrance = floorplan.currentEntrance;
        int randomRotation = Random.Range(0, 3);
        for (int i = 0; i < randomRotation; i++)
        {
            floorplan.Rotate();
        }

        return floorplan;
    }

    public void Rotate()
    {
        bool entranceValue = connection[currentEntrance];
        for (int i = 0; i < 3; i++)
        {
            int connection = (currentEntrance + i) % 4;
            //Debug.Log($"{connection} -> {openEntrances[connection]}");
            this.connection[connection] = this.connection[(connection + 1) % 4];
        }
        connection[(currentEntrance + 3) % 4] = entranceValue;
        //Debug.Log($"{currentEntrance} => {openEntrances[currentEntrance]}");
        //keep rotating if entrance is closed
        if (!connection[currentEntrance])
        {
            Rotate();
            return;
        }

        StringBuilder sb = new();
        for (int i = 0; i < connection.Length; i++)
            sb.Append($"{connection[i]} | ");
        Debug.Log(sb);
    }

    private int DirectionToID(Vector2Int direction)
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

    private Vector2Int IDToDirection(int id)
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