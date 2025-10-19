using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Deck", menuName = "Floorplan/Player Deck")]
public class PlayerDeck : ScriptableObject
{
    public RoomCategory preferredCategory = 0;
    public List<Room> deck = new();
}
