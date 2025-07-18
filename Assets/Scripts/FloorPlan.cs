using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Floorplan", menuName = "Floorplan")]
public class Floorplan : ScriptableObject
{
    public string Name;
    public string Description;

    public Color Color = Color.white;

    public FloorType Type;
    public int Entrances => Mathf.Abs((int)Type);

    public bool[] openEntrances;

    public Floorplan CreateInstance(Vector2Int firsEntrance)
    {
        Floorplan floorplan = new Floorplan();
        floorplan.Name = Name;
        floorplan.Description = Description;
        floorplan.Color = Color;
        floorplan.Type = Type;
        floorplan.openEntrances = new bool[4];

        return floorplan;
    }

    public void Rotate()
    {

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