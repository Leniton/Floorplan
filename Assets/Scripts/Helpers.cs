using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static bool ConnectedToFloorplan(this Floorplan targetFloorplan, Floorplan firstFloorplan,
        Floorplan secondFloorplan, out Floorplan other)
    {
        other = null;
        if(firstFloorplan != targetFloorplan && secondFloorplan != targetFloorplan) return false;
        other = firstFloorplan == targetFloorplan ? secondFloorplan : firstFloorplan;
        return true;
    }
}
