using Lenix.NumberUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

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

    public static bool IsOfCategory(this Floorplan floorplan, FloorCategory category) => NumberUtil.ContainsBytes((int)floorplan.Category, (int)category));

    public static void IncreaseChanceOfDrawing(this DrawFloorplanEvent evt, Func<Floorplan, bool> condition, float chance = .4f)
    {
        int possiblesFloorplans = 0;
        RarityPicker<Floorplan> picker = new(
            evt.floorplanPicker.commonRate,
            evt.floorplanPicker.uncommonRate,
            evt.floorplanPicker.rareRate,
            evt.floorplanPicker.legendRate);

        for (int i = 0; i < evt.possibleFloorplans.Count; i++)
        {
            Floorplan targetFloorplan = evt.possibleFloorplans[i];
            if (!condition.Invoke(evt.possibleFloorplans[i])) continue;
            bool alreadyDrawn = false;
            for (int f = 0; f < evt.drawnFloorplans.Length; f++)
            {
                if (!ReferenceEquals(evt.drawnFloorplans[f], targetFloorplan)) continue;
                alreadyDrawn = true;
                break;
            }
            if (alreadyDrawn) continue;
            picker.AddToPool(evt.possibleFloorplans[i], evt.possibleFloorplans[i].Rarity);
            possiblesFloorplans++;
        }

        if (possiblesFloorplans <= 0) return; //there's no floorplan that fits the criteria

        float r = Random.Range(0f, 1f);
        if (r <= chance && !condition.Invoke(evt.possibleFloorplans[^1])) 
        {
            evt.drawnFloorplans[^1] = picker.PickRandom(picker.commonRate, true);
            possiblesFloorplans--;
            //Debug.Log($"chance hit: changed to {evt.drawnFloorplans[^1].Name}");
        }
        if (possiblesFloorplans <= 0) return;
        for (int i = evt.drawnFloorplans.Length - 2; i >= 0; i--)
        {
            r = Random.Range(0f, 1f);
            if (r > chance || condition.Invoke(evt.possibleFloorplans[^1])) continue;
            evt.drawnFloorplans[i] = picker.PickRandom(removeFromPool: true);
            //Debug.Log($"chance hit: changed to {evt.drawnFloorplans[i].Name}");
            possiblesFloorplans--;
            if(possiblesFloorplans <= 0) break;
        }
    }
}
