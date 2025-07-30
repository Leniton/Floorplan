using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvent
{
    public static Action<DrawFloorplanEvent> onDrawFloorplans;
    public static Action<GenericFloorplanEvent> onDraftedFloorplan;
    public static Action<FloorplanConnectedEvent> onConnectFloorplans;
    public static Action<GenericFloorplanEvent> OnExitFloorplan;
    public static Action<GenericFloorplanEvent> OnEnterFloorplan;
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
public class GenericFloorplanEvent : CoordinateEvent
{
    public Floorplan Floorplan;

    public GenericFloorplanEvent(Vector2Int coordinates, Floorplan floorplan) : base(coordinates)
    {
        Floorplan = floorplan;
    }
}
public class FloorplanConnectedEvent : Event
{
    public Floorplan baseFloorplan;
    public Floorplan connectedFloorplan;

    public FloorplanConnectedEvent(Floorplan baseFloorplan, Floorplan connectedFloorplan)
    {
        this.baseFloorplan = baseFloorplan;
        this.connectedFloorplan = connectedFloorplan;
    }
}
public class ItemEvent : Event
{
    public Item item;
    public ItemEvent(Item _item) => item = _item;
}