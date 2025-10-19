using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RunData
{
    public static PlayerDeck playerDeck = ScriptableObject.CreateInstance<PlayerDeck>();
    public static PlayerDeck allRooms = ScriptableObject.CreateInstance<PlayerDeck>();
}
