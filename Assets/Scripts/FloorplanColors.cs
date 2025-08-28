using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Floorplan", menuName = "Floorplan/Floorplan Colors")]
public class FloorplanColors : ScriptableObject
{
    public Color blankColor;
    public Color restRoomColor;
    public Color hallwayColor;
    public Color storageRoomColor;
    public Color fancyRoomColor;
    public Color shopColor;
    public Color mysteryRoomColor;
    public Color cursedRoomColor;

    public Color GetColor(FloorCategory category)
    {
        return category switch
        {
            FloorCategory.RestRoom => restRoomColor,
            FloorCategory.Hallway => hallwayColor,
            FloorCategory.StorageRoom => storageRoomColor,
            FloorCategory.FancyRoom => fancyRoomColor,
            FloorCategory.Shop => shopColor,
            FloorCategory.MysteryRoom => mysteryRoomColor,
            FloorCategory.CursedRoom => cursedRoomColor,
            _ => blankColor
        };
    }
}
