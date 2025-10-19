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

    public Color GetColor(RoomCategory category)
    {
        return category switch
        {
            RoomCategory.RestRoom => restRoomColor,
            RoomCategory.Hallway => hallwayColor,
            RoomCategory.StorageRoom => storageRoomColor,
            RoomCategory.FancyRoom => fancyRoomColor,
            RoomCategory.Shop => shopColor,
            RoomCategory.MysteryRoom => mysteryRoomColor,
            RoomCategory.CursedRoom => cursedRoomColor,
            _ => blankColor
        };
    }
}
