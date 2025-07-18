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
    public int Entrances => Mathf.Abs((int)Type);

    public int currentEntrance = 0;
    public bool[] openEntrances;

    public Floorplan CreateInstance(Vector2Int entranceDirection)
    {
        Floorplan floorplan = CreateInstance<Floorplan>();
        floorplan.Name = Name;
        floorplan.Description = Description;
        floorplan.Color = Color;
        floorplan.Type = Type;
        floorplan.openEntrances = new bool[] 
        {
            true,
            Type != FloorType.DeadEnd && Type != FloorType.Straw,
            Type != FloorType.DeadEnd && Type != FloorType.Ankle,
            Type == FloorType.Crossroad,
        };

        StringBuilder sb = new();
        for (int i = 0; i < floorplan.openEntrances.Length; i++)
            sb.Append($"{floorplan.openEntrances[i]} | ");
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
        bool lastConnection = openEntrances[(currentEntrance + 3) % 4];
        for (int i = 1; i < 4; i++)
        {
            int connection = (currentEntrance + i) % 4;
            Debug.Log($"{connection} -> {openEntrances[connection]}");
            openEntrances[(connection + 3) % 4] = openEntrances[connection];
        }
        openEntrances[currentEntrance] = lastConnection;
        Debug.Log($"{currentEntrance} => {openEntrances[currentEntrance]}");
        //keep rotating if entrance is closed
        //if (!openEntrances[currentEntrance])
        //{
        //    Rotate();
        //    return;
        //}

        StringBuilder sb = new();
        for (int i = 0; i < openEntrances.Length; i++)
            sb.Append($"{openEntrances[i]} | ");
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