using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvent
{
    public static Action<Event> onGameStart;
    /// <summary>
    /// Initial draw
    /// </summary>
    public static Action<DrawRoomEvent> onDrawRooms;
    /// <summary>
    /// Changed drawn
    /// </summary>
    public static Action<DrawRoomEvent> onDrawChange;
    /// <summary>
    /// Modifies drawn
    /// </summary>
    public static Action<DrawRoomEvent> onModifyDraw;
    public static Action<RoomEvent> onDraftedRoom;
    public static Action<RoomConnectedEvent> onConnectRooms;
    public static Action<RoomEvent> onExitRoom;
    public static Action<RoomEvent> onEnterRoom;
    public static Action<ItemEvent> onCollectItem;
    public static Action<CategoryChangeEvent> onRoomCategoryChanged;

    public static Action<ValueEvent> onStepsChanged;
    public static Action<ValueEvent> onKeysChanged;
    public static Action<ValueEvent> onCoinsChanged;

    public static void ResetListeners()
    {
        onDrawRooms = null;
        onDrawChange = null;
        onModifyDraw = null;
        onDraftedRoom = null;
        onConnectRooms = null;
        onExitRoom = null;
        onEnterRoom = null;
        onCollectItem = null;
        onRoomCategoryChanged = null;
    }
}

public class Event { }

public class ValueEvent : Event
{
    public int amount;
    public ValueEvent(int _amount) => amount = _amount;
}
public class CoordinateEvent : Event
{
    public Vector2Int Coordinates;
    public CoordinateEvent(Vector2Int coordinates) => Coordinates = coordinates;
}
public class RoomEvent : CoordinateEvent
{
    public Room Room;

    public RoomEvent(Vector2Int coordinates, Room room) : base(coordinates)
    {
        Room = room;
    }
}
public class RoomConnectedEvent : RoomEvent
{
    public Room baseRoom;
    public Room connectedRoom => Room;

    public RoomConnectedEvent(Room baseRoom, Room connectedRoom, Vector2Int connectedCoordinates) :
        base(connectedCoordinates, connectedRoom)
    {
        this.baseRoom = baseRoom;
    }
}
public class ItemEvent : Event
{
    public Item item;
    public ItemEvent(Item _item) => item = _item;
}