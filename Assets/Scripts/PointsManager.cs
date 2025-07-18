using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsManager : MonoBehaviour
{
    [SerializeField] private DraftManager draftManager;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Player player;
    [SerializeField] private FloorplanUI floorplanPrefab;

    Dictionary<Vector2Int, Floorplan> floorplanDict;

    private Vector2Int currentDraftPosition;

    private void Start()
    {
        floorplanDict = new();
        player.OnMove += OnMoveSlot;
        draftManager.OnDraftFloorplan += PlaceFloorplan;
    }

    private void OnMoveSlot(Vector2Int direction)
    {
        Vector2Int targetedSlot = gridManager.currentPosition + direction;
        if (floorplanDict.ContainsKey(targetedSlot))
        {
            //slot enter event
            gridManager.ShiftSelection(direction);
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
    }
}
