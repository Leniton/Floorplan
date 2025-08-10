using System;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static bool ConnectedToFloorplan(this Floorplan targetFloorplan, FloorplanConnectedEvent evt, out Floorplan other)
    {
        other = null;
        if(evt.baseFloorplan != targetFloorplan && evt.connectedFloorplan != targetFloorplan) return false;
        other = evt.baseFloorplan == targetFloorplan ? evt.connectedFloorplan : evt.baseFloorplan;
        return true;
    }

    public static Floorplan DraftedFrom(this Floorplan floorplan)
    {
        Vector2Int draftedCoordinates = floorplan.coordinate + Floorplan.IDToDirection(floorplan.entranceId);
        GameManager.floorplanDict.TryGetValue(draftedCoordinates, out var draftedFloorplan);
        return floorplan;
    }
    
    public static Floorplan CurrentFloorplan()
    {
        GameManager.floorplanDict.TryGetValue(GridManager.instance.currentPosition, out var current);
        return current;
    }

    public static void ConnectFloorplans(Floorplan baseFloorplan, Floorplan connectedFloorplan)
    {
        //the floorplan who's already there first
        //Debug.Log($"connecting {baseFloorplan.Name} to {connectedFloorplan.Name}");
        baseFloorplan.connectedFloorplans.Add(connectedFloorplan);
        baseFloorplan.onConnectToFloorplan?.Invoke(new(baseFloorplan, connectedFloorplan, connectedFloorplan.coordinate));
        connectedFloorplan.connectedFloorplans.Add(baseFloorplan);
        connectedFloorplan.onConnectToFloorplan?.Invoke(new(connectedFloorplan, baseFloorplan, baseFloorplan.coordinate));
        GameEvent.onConnectFloorplans?.Invoke(new(connectedFloorplan, baseFloorplan, baseFloorplan.coordinate));
    }

    public static void OpenConnection(this Floorplan floorplan, int connectionID)
    {
        if (floorplan.connections[connectionID]) return;//already open
        //Debug.Log($"create opening on {floorplan.name}");
        floorplan.connections[connectionID] = true;
        floorplan.OnChanged?.Invoke();
        if (!GameManager.floorplanDict.TryGetValue(floorplan.coordinate + Floorplan.IDToDirection(connectionID), out var targetFloorplan)) return;
        if (!targetFloorplan.connections[(connectionID + 2) % 4]) return;
        //Debug.Log($"{floorplan.Name} now connected to {targetFloorplan.Name}");
        ConnectFloorplans(floorplan, targetFloorplan);
    }
}
