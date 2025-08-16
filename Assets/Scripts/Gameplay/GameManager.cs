using AddressableAsyncInstances;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private DraftManager draftManager;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Player player;
    [SerializeField] private Floorplan entrance;
    [SerializeField] private Image currentImage;
    [SerializeField] private Button finishButton;

    public static Dictionary<Vector2Int, Floorplan> floorplanDict;

    private FloorplanUI floorplanPrefab;
    private Vector2Int currentDraftPosition;

    private void Start()
    {
        GameSettings.current = new();
        floorplanDict = new();
        player.OnMove += OnMoveSlot;
        draftManager.OnDraftFloorplan += PlaceFloorplan;
        gridManager.OnStartMove += TriggerFloorplanExitEvent;
        gridManager.OnMove += TriggerFloorplanEnterEvent;
        finishButton.onClick.AddListener(FinishRun);

        Checklist loadedAssets = new(1);
        loadedAssets.onCompleted += Setup;
        gridManager.onDoneLoading += loadedAssets.FinishStep;
        loadedAssets.AddStep();
        AAComponent<FloorplanUI>.LoadComponent("FloorplanUI", prefab =>
        {
            floorplanPrefab = prefab;
            loadedAssets.FinishStep();
        });
    }

    private void Setup()
    {
        //add entrance hall
        GridManager.instance = gridManager;
        currentDraftPosition = gridManager.currentPosition;
        Floorplan entranceHall = entrance.CreateInstance(Vector2Int.left);
        PlaceFloorplan(entranceHall);
        UIManager.ShowMessage($"Current objective:\n\n <b>{PointsManager.currentRequirement} points");
    }

    private void OnMoveSlot(Vector2Int direction)
    {
        bool forceEntrance = Player.activeSledgeHammer;
        Floorplan current = Helpers.CurrentFloorplan();
        Vector2Int targetedSlot = gridManager.currentPosition + direction;
        if (!gridManager.ValidCoordinate(targetedSlot)) return;

        if (!current.connections[Floorplan.DirectionToID(direction)] && !forceEntrance) return;
        if (!current.connections[Floorplan.DirectionToID(direction)] && forceEntrance)
        {
            //add connection
            current.OpenConnection(Floorplan.DirectionToID(direction));
            if (Player.activeSledgeHammer) Player.ConsumeSledgeHammer();
        }

        if (floorplanDict.TryGetValue(targetedSlot, out var targetFloorplan))
        {
            if (Player.steps <= 0) return;
            //check if floorplan is connected to this one
            if (!targetFloorplan.connections[Floorplan.DirectionToID(-direction)])
            {

                if (!Player.activeSledgeHammer && !forceEntrance) return;
                //Add connection to other floorplans
                targetFloorplan.OpenConnection(Floorplan.DirectionToID(-direction));
                if (Player.activeSledgeHammer) Player.ConsumeSledgeHammer();
            }

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

        EffectsManager.AddFloorplanEffect(floorplan);
        floorplanDict[currentDraftPosition] = floorplan;
        floorplan.coordinate = currentDraftPosition;
        floorplan.onDrafted?.Invoke(new(currentDraftPosition));
        GameEvent.onDraftedFloorplan?.Invoke(new(currentDraftPosition, floorplan));
        
        //connect floorplan
        for (int i = 0; i < floorplan.connections.Length; i++)
        {
            if(!floorplan.connections[i]) continue;
            Vector2Int direction = Floorplan.IDToDirection(i);
            Vector2Int slot = currentDraftPosition + direction;
            //Debug.Log($"{floorplan.Name} is open at {direction}[{slot}]");
            if (!floorplanDict.TryGetValue(slot, out var targetFloorplan)) continue;
            //Debug.Log($"there's a floorplan on {slot}({targetFloorplan.Name})");
            if (!targetFloorplan.connections[Floorplan.DirectionToID(-direction)]) continue;
            //Debug.Log($"{floorplan.Name} is connected to {targetFloorplan.Name}");
            
            //the floorplan who's already there first
            Helpers.ConnectFloorplans(targetFloorplan, floorplan);
            //targetFloorplan.connectedFloorplans.Add(floorplan);
            //targetFloorplan.onConnectToFloorplan?.Invoke(new(targetFloorplan, floorplan, currentDraftPosition));
            //floorplan.connectedFloorplans.Add(targetFloorplan);
            //floorplan.onConnectToFloorplan?.Invoke(new(floorplan, targetFloorplan, slot));
            //GameEvent.onConnectFloorplans?.Invoke(new(floorplan, targetFloorplan, slot));
        }
    }

    private void TriggerFloorplanExitEvent(Vector2Int origin, Vector2Int goal)
    {
        //Debug.Log($"exit {floorplanDict[origin]}");
        Player.ChangeSteps(-1);
        Floorplan floorplan = floorplanDict[origin];
        floorplan.onExit?.Invoke(new());
        GameEvent.OnExitFloorplan?.Invoke(new(origin, floorplan));
    }

    private void TriggerFloorplanEnterEvent(Vector2Int coordinate)
    {
        Floorplan floorplan = floorplanDict[coordinate];
        //Debug.Log($"entered {floorplan.Name}({coordinate})\n{GridManager.instance.currentPosition}");
        floorplan.onEnter?.Invoke(new());
        GameEvent.OnEnterFloorplan?.Invoke(new(coordinate, floorplan));
        StartCoroutine(CheckSteps());
    }

    private IEnumerator CheckSteps()
    {
        if(Player.steps > 0) yield break;

        yield return null;
        FinishRun();
    }

    private void FinishRun()
    {
        int finalPoints = PointsManager.GetTotalPoints();
        if (finalPoints >= PointsManager.currentRequirement)
        {
            //win, progress
            PointsManager.Progress();
        }
        else
        {
            //lose, reset
            PointsManager.Reset();
        }
        GameEvent.ResetListeners();
        SceneManager.LoadScene(0);
    }
}
