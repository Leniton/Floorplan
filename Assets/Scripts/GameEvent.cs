using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvent
{
    public static Action<DrawFloorplanEvent> onDrawFloorplans;
    public static Action<FloorplanEvent> onDraftedFloorplan;
    public static Action<FloorplanConnectedEvent> onConnectFloorplans;
    public static Action<FloorplanEvent> OnExitFloorplan;
    public static Action<FloorplanEvent> OnEnterFloorplan;
    public static Action<ItemEvent> OnCollectItem;

    public static void ResetListeners()
    {
        onDrawFloorplans = null;
        onDraftedFloorplan = null;
        onConnectFloorplans = null;
        OnExitFloorplan = null;
        OnEnterFloorplan = null;
        OnCollectItem = null;
    }
}

public class Event { }
public class CoordinateEvent : Event
{
    public Vector2Int Coordinates;
    public CoordinateEvent(Vector2Int coordinates) => Coordinates = coordinates;
}
public class FloorplanEvent : CoordinateEvent
{
    public Floorplan Floorplan;

    public FloorplanEvent(Vector2Int coordinates, Floorplan floorplan) : base(coordinates)
    {
        Floorplan = floorplan;
    }
}
public class FloorplanConnectedEvent : FloorplanEvent
{
    public Floorplan baseFloorplan;
    public Floorplan connectedFloorplan => Floorplan;

    public FloorplanConnectedEvent(Floorplan baseFloorplan, Floorplan connectedFloorplan, Vector2Int connectedCoordinates) :
        base(connectedCoordinates, connectedFloorplan)
    {
        this.baseFloorplan = baseFloorplan;
    }
}
public class ItemEvent : Event
{
    public Item item;
    public ItemEvent(Item _item) => item = _item;
}