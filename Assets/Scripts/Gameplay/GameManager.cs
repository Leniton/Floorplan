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
    [SerializeField] private MovementManager movement;
    [SerializeField] private DeckView deckView;
    [SerializeField] private Floorplan entrance;
    [SerializeField] private Image currentImage;
    [SerializeField] private Button finishButton;
    [SerializeField] private HoverMenu hoverMenu;
    [SerializeField] private MinimapManager minimap;

    private static Dictionary<Floorplan, FloorplanUI> UIFloorplans;
    public static Dictionary<Vector2Int, Floorplan> floorplanDict;
    public static List<Floorplan> DraftPool;
    public static Floorplan EntranceHall;

    private static FloorplanUI floorplanPrefab;
    private Vector2Int currentDraftPosition;

    private void Start()
    {
        movement.OnDirectionChanged += UpdateMovementDirection;
        Player.ResetPlayer();
        UIFloorplans = new();
        floorplanDict = new();
        movement.OnMove += OnMoveSlot;
        draftManager.OnDraftFloorplan += OnChooseFloorplan;
        gridManager.OnStartMove += TriggerFloorplanExitEvent;
        gridManager.OnMove += TriggerFloorplanEnterEvent;
        finishButton.onClick.AddListener(FinishRun);
        hoverMenu.SetupOptions(new()
        {
            new()
            {
                icon = GameAssets.books[18],
                color = new(.6f, .6f, .6f),
                onPick = () => Glossary.OpenGlossary()
            },
            new()
            {
                icon = GameAssets.books[98],
                onPick = deckView.Open
            },
            null,
        });

        Checklist loadedAssets = new(0);
        loadedAssets.onCompleted += Setup;

        loadedAssets.AddStep();
        draftManager.Setup(3, RunData.playerDeck, pool =>
        {
            DraftPool = pool;
            loadedAssets.FinishStep();
        });

        loadedAssets.AddStep();
        gridManager.onDoneLoading += loadedAssets.FinishStep;
        loadedAssets.AddStep();
        AAComponent<FloorplanUI>.LoadComponent("FloorplanUI", prefab =>
        {
            floorplanPrefab = prefab;
            loadedAssets.FinishStep();
        });
        loadedAssets.AddStep();
        AAAsset<FloorplanColors>.LoadAsset("DefaultFloorplanColors", colors =>
        {
            GameSettings.current.floorplanColors = colors;
            loadedAssets.FinishStep();
        });
    }

    private void Setup()
    {
        deckView.Close();
        draftManager.CloseWindow();
        gridManager.onClick += InspectCurrentFloorplan;
        gridManager.SetInteractive(false);
        //add entrance hall
        GridManager.instance = gridManager;
        currentDraftPosition = gridManager.currentPosition;
        EntranceHall = entrance.CreateInstance(Vector2Int.left);
        OnChooseFloorplan(EntranceHall);
        minimap.OpenMinimap();
        MessageWindow.ShowMessage($"Current objective:\n\n <b>{PointsManager.currentRequirement} points", 
            () => GameEvent.onGameStart?.Invoke(new()));
    }

    private void InspectCurrentFloorplan(Vector2Int coordinate)
    {
        UIManager.ShowCurrentFloorplan();
    }

    private void UpdateMovementDirection(Vector2Int direction)
    {
        FloorplanUI floorplan = UIFloorplans[Helpers.CurrentFloorplan()];
        floorplan.HighlightDirection(direction);
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
        else //Draft a Floorplan
        {
            currentDraftPosition = targetedSlot;
            draftManager.DraftFloorplan(direction, currentDraftPosition, (GridManager.instance.currentPosition + direction).y);
        }
    }

    private void OnChooseFloorplan(Floorplan floorplan)
    {
        if (Player.keys < floorplan.keyCost)
        {
            MessageWindow.ShowMessage("You don't have enough keys!!");
            return;
        }
        draftManager.CloseWindow();
        Player.ChangeKeys(-floorplan.keyCost);
        draftManager.RemoveFloorplanFromPool(floorplan);

        PlaceFloorplan(floorplan, currentDraftPosition);
    }

    public static void PlaceFloorplan(Floorplan floorplan, Vector2Int coordinate)
    {
        FloorplanUI instance = Instantiate(floorplanPrefab, GridManager.instance.GetSlotRect(coordinate));
        instance.Setup(floorplan);
        UIFloorplans[floorplan] = instance;
        RectTransform floorplanRect = (RectTransform)instance.transform;
        floorplanRect.anchoredPosition = Vector2.zero;
        floorplanRect.anchorMin = Vector2.zero;
        floorplanRect.anchorMax = Vector2.one;
        floorplanRect.sizeDelta = Vector2.zero;

        //Apply floorplan effect
        EffectsManager.AddFloorplanEffect(floorplan);
        //Apply renovation effect
        floorplan.renovation?.activationEffect?.Invoke(floorplan);
        floorplanDict[coordinate] = floorplan;
        floorplan.coordinate = coordinate;
        floorplan.onDrafted?.Invoke(new(coordinate));
        GameEvent.onDraftedFloorplan?.Invoke(new(coordinate, floorplan));

        //connect floorplan
        for (int i = 0; i < floorplan.connections.Length; i++)
        {
            if (!floorplan.connections[i]) continue;
            Vector2Int direction = Floorplan.IDToDirection(i);
            Vector2Int slot = coordinate + direction;
            //Debug.Log($"{floorplan.Name} is open at {direction}[{slot}]");
            if (!floorplanDict.TryGetValue(slot, out var targetFloorplan)) continue;
            //Debug.Log($"there's a floorplan on {slot}({targetFloorplan.Name})");
            if (!targetFloorplan.connections[Floorplan.DirectionToID(-direction)]) continue;
            //Debug.Log($"{floorplan.Name} is connected to {targetFloorplan.Name}");

            Helpers.ConnectFloorplans(targetFloorplan, floorplan);
        }
    }

    private void TriggerFloorplanExitEvent(Vector2Int origin, Vector2Int goal)
    {
        //Debug.Log($"exit {floorplanDict[origin]}");
        gridManager.GetSlot(origin).interactable = false;
        Player.ChangeSteps(-1);
        Floorplan floorplan = floorplanDict[origin];
        floorplan.onExit?.Invoke(new());
        GameEvent.OnExitFloorplan?.Invoke(new(origin, floorplan));
    }

    private void TriggerFloorplanEnterEvent(Vector2Int coordinate)
    {
        gridManager.GetSlot(coordinate).interactable = true;
        Floorplan floorplan = floorplanDict[coordinate];
        //Debug.Log($"entered {floorplan.Name}({coordinate})\n{GridManager.instance.currentPosition}");
        floorplan.onEnter?.Invoke(new());
        GameEvent.OnEnterFloorplan?.Invoke(new(coordinate, floorplan));
    }

    private void FinishRun()
    {
        int finalPoints = PointsManager.GetTotalPoints();
        int targetScene = 2;
        if (finalPoints >= PointsManager.currentRequirement)
        {
            //win, progress
            PointsManager.Progress();
        }
        else
        {
            //lose, reset
            PointsManager.Reset();
            targetScene = 0;
        }
        floorplanDict.Clear();
        GameEvent.ResetListeners();
        SceneManager.LoadScene(targetScene);
    }
}
