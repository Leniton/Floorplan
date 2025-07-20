using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointsManager : MonoBehaviour
{
    [SerializeField] private DraftManager draftManager;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Player player;
    [SerializeField] private FloorplanUI floorplanPrefab;
    [SerializeField] private Floorplan entrance;
    [SerializeField] private Image currentImage;

    private float currentAlpha;
    private Dictionary<Vector2Int, Floorplan> floorplanDict;

    private Vector2Int currentDraftPosition;

    public static event Action<Vector2Int, Floorplan> onDraftedFloorplan;

    private void Start()
    {
        floorplanDict = new();
        player.OnMove += OnMoveSlot;
        draftManager.OnDraftFloorplan += PlaceFloorplan;

        currentAlpha = currentImage.color.a;
        //add entrance hall
        currentDraftPosition = gridManager.currentPosition;
        Floorplan floorplan = entrance.CreateInstance(Vector2Int.up);
        draftManager.CorrectFloorplanRotation(floorplan, new() { Vector2Int.up, Vector2Int.left, Vector2Int.right });
        PlaceFloorplan(floorplan);
    }

    private void OnMoveSlot(Vector2Int direction)
    {
        Floorplan current = floorplanDict[gridManager.currentPosition];
        if (!current.connections[Floorplan.DirectionToID(direction)]) return;
        Vector2Int targetedSlot = gridManager.currentPosition + direction;
        if (floorplanDict.TryGetValue(targetedSlot, out var targetFloorplan))
        {
            if (Player.steps <= 0) return;
            //check if floorplan is connected to this one
            if (!targetFloorplan.connections[Floorplan.DirectionToID(-direction)]) return;
            //slot enter event
            gridManager.ShiftSelection(direction);
            Player.ChangeSteps(-1);
        }
        else
        {
            //get possible sides
            List<Vector2Int> possibleSlots = new();
            //up
            Vector2Int targetCoordinate = targetedSlot + Vector2Int.up;
            if (gridManager.ValidCoordinate(targetCoordinate)) possibleSlots.Add(Vector2Int.up);
            //down
            targetCoordinate = targetedSlot + Vector2Int.down;
            if (gridManager.ValidCoordinate(targetCoordinate)) possibleSlots.Add(Vector2Int.down);
            //left
            targetCoordinate = targetedSlot + Vector2Int.left;
            if (gridManager.ValidCoordinate(targetCoordinate)) possibleSlots.Add(Vector2Int.left);
            //right
            targetCoordinate = targetedSlot + Vector2Int.right;
            if (gridManager.ValidCoordinate(targetCoordinate)) possibleSlots.Add(Vector2Int.right);

            //draft plan?
            currentDraftPosition = targetedSlot;
            draftManager.DraftFloorplan(direction, possibleSlots);
        }
    }

    private void PlaceFloorplan(Floorplan floorplan)
    {
        FloorplanUI instance = Instantiate(floorplanPrefab, gridManager.GetSlot(currentDraftPosition));
        instance.Setup(floorplan);
        RectTransform floorplanRect = (RectTransform)instance.transform;
        floorplanRect.anchoredPosition = Vector2.zero;
        floorplanRect.anchorMin = Vector2.zero;
        floorplanRect.anchorMax = Vector2.one;
        floorplanRect.sizeDelta = Vector2.zero;

        floorplanDict[currentDraftPosition] = floorplan;
        onDraftedFloorplan?.Invoke(currentDraftPosition, floorplan);
    }
}
