using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsManager : MonoBehaviour
{
    [SerializeField] private DraftManager draftManager;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Player player;

    Dictionary<Vector2Int, Floorplan> floorplanDict;

    private void Start()
    {
        floorplanDict = new();
        player.OnMove += OnMoveSlot;
    }

    private void OnMoveSlot(Vector2Int coordinate)
    {
        if (floorplanDict.ContainsKey(coordinate))
        {
            //slot enter event
            gridManager.ShiftSelection(coordinate);
        }
        else
        {
            //draft plan?
            draftManager.DraftFloorplan();
        }
    }
}
