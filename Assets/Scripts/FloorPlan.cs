using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Floorplan", menuName = "Floorplan")]
public class FloorPlan : ScriptableObject
{
    public string Name;
    public string Description;

    public Color Color = Color.white;

    public FloorType Type;
    public int Entrances => Mathf.Abs((int)Type);
}
public enum FloorType
{
    DeadEnd = 1,
    Straw = 2,
    Ankle = -2,
    TPiece = 3,
    Crossroad = 4,
}