using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvent
{
    public static Action<DrawFloorplanEvent> onDrawFloorplans;
    public static Action<Vector2Int, Floorplan> onDraftedFloorplan;
    public static Action<Floorplan, Floorplan> onConnectFloorplans;
    public static Action<Vector2Int, Floorplan> OnExitFloorplan;
    public static Action<Vector2Int, Floorplan> OnEnterFloorplan;
    public static Action<Item> OnCollectItem;

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
