using AddressableAsyncInstances;
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
    [SerializeField] private Room entrance;
    [SerializeField] private Image currentImage;
    [SerializeField] private Button finishButton;
    [SerializeField] private HoverMenu hoverMenu;
    [SerializeField] private MinimapManager minimap;
    [SerializeField] private HouseStatsWindow houseStats;

    private static Dictionary<Room, FloorplanUI> UIRooms;
    public static Dictionary<Vector2Int, Room> roomDict;
    public static List<Room> DraftPool;
    public static Room EntranceHall;

    private static FloorplanUI roomPrefab;
    private Vector2Int currentDraftPosition;

    private void Start()
    {
        movement.OnDirectionChanged += UpdateMovementDirection;
        Player.ResetPlayer();
        UIRooms = new();
        roomDict = new();
        movement.OnMove += OnMoveSlot;
        draftManager.OnDraftRoom += OnChooseRoom;
        gridManager.OnStartMove += TriggerRoomExitEvent;
        gridManager.OnMove += TriggerRoomEnterEvent;
        finishButton.onClick.AddListener(FinishRun);
        hoverMenu.SetupOptions(new()
        {
            new()
            {
                icon = GameAssets.books[98],
                onPick = deckView.Open
            },
            new()
            {
                icon = GameAssets.books[18],
                color = new(.6f, .6f, .6f),
                onPick = () => Glossary.OpenGlossary()
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
            roomPrefab = prefab;
            loadedAssets.FinishStep();
        });
        loadedAssets.AddStep();
        AAAsset<FloorplanColors>.LoadAsset("DefaultFloorplanColors", colors =>
        {
            GameSettings.current.roomColors = colors;
            loadedAssets.FinishStep();
        });
    }

    private void Setup()
    {
        draftManager.CloseWindow();
        gridManager.onClick += InspectCurrentFloorplan;
        gridManager.SetInteractive(false);
        //add entrance hall
        GridManager.instance = gridManager;
        currentDraftPosition = gridManager.currentPosition;
        EntranceHall = entrance.CreateInstance(Vector2Int.left);
        OnChooseRoom(EntranceHall);
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
        FloorplanUI floorplan = UIRooms[Helpers.CurrentRoom()];
        floorplan.HighlightDirection(direction);
    }

    private void OnMoveSlot(Vector2Int direction)
    {
        bool forceEntrance = Player.activeSledgeHammer;
        Room current = Helpers.CurrentRoom();
        Vector2Int targetedSlot = gridManager.currentPosition + direction;
        if (!gridManager.ValidCoordinate(targetedSlot)) return;

        if (!current.connections[Room.DirectionToID(direction)] && !forceEntrance) return;
        if (!current.connections[Room.DirectionToID(direction)] && forceEntrance)
        {
            //add connection
            current.OpenConnection(Room.DirectionToID(direction));
            if (Player.activeSledgeHammer) Player.ConsumeSledgeHammer();
        }

        if (roomDict.TryGetValue(targetedSlot, out var targetFloorplan))
        {
            if (Player.steps <= Player.minSteps) return;
            //check if floorplan is connected to this one
            if (!targetFloorplan.connections[Room.DirectionToID(-direction)])
            {

                if (!Player.activeSledgeHammer && !forceEntrance) return;
                //Add connection to other floorplans
                targetFloorplan.OpenConnection(Room.DirectionToID(-direction));
                if (Player.activeSledgeHammer) Player.ConsumeSledgeHammer();
            }

            //slot enter event
            gridManager.ShiftSelection(direction);
        }
        else //Draft a Floorplan
        {
            currentDraftPosition = targetedSlot;
            draftManager.DraftRoom(direction, currentDraftPosition, (GridManager.instance.currentPosition + direction).y);
        }
    }

    private void OnChooseRoom(Room room)
    {
        if ((Player.keys - room.keyCost) < Player.minKeys)
        {
            MessageWindow.ShowMessage("You don't have enough keys!!");
            return;
        }
        draftManager.CloseWindow();
        Player.ChangeKeys(-room.keyCost);
        draftManager.RemoveRoomFromPool(room);

        PlaceRoom(room, currentDraftPosition);
    }

    public static void PlaceRoom(Room room, Vector2Int coordinate)
    {
        FloorplanUI instance = Instantiate(roomPrefab, GridManager.instance.GetSlotRect(coordinate));
        instance.Setup(room);
        UIRooms[room] = instance;
        RectTransform roomRect = (RectTransform)instance.transform;
        roomRect.anchoredPosition = Vector2.zero;
        roomRect.anchorMin = Vector2.zero;
        roomRect.anchorMax = Vector2.one;
        roomRect.sizeDelta = Vector2.zero;

        //Apply floorplan effect
        EffectsManager.AddRoomEffect(room);
        //Apply renovation effect
        room.renovation?.activationEffect?.Invoke(room);
        roomDict[coordinate] = room;
        room.coordinate = coordinate;
        room.onDrafted?.Invoke(new(coordinate));
        GameEvent.onDraftedRoom?.Invoke(new(coordinate, room));

        //connect floorplan
        for (int i = 0; i < room.connections.Length; i++)
        {
            if (!room.connections[i]) continue;
            Vector2Int direction = Room.IDToDirection(i);
            Vector2Int slot = coordinate + direction;
            //Debug.Log($"{floorplan.Name} is open at {direction}[{slot}]");
            if (!roomDict.TryGetValue(slot, out var targetRoom)) continue;
            //Debug.Log($"there's a floorplan on {slot}({targetFloorplan.Name})");
            if (!targetRoom.connections[Room.DirectionToID(-direction)]) continue;
            //Debug.Log($"{floorplan.Name} is connected to {targetFloorplan.Name}");

            Helpers.ConnectRooms(targetRoom, room);
        }
    }

    private void TriggerRoomExitEvent(Vector2Int origin, Vector2Int goal)
    {
        //Debug.Log($"exit {floorplanDict[origin]}");
        gridManager.GetSlot(origin).interactable = false;
        Player.ChangeSteps(-1);
        Room room = roomDict[origin];
        room.onExit?.Invoke(new());
        GameEvent.onExitRoom?.Invoke(new(origin, room));
    }

    private void TriggerRoomEnterEvent(Vector2Int coordinate)
    {
        gridManager.GetSlot(coordinate).interactable = true;
        Room room = roomDict[coordinate];
        //Debug.Log($"entered {floorplan.Name}({coordinate})\n{GridManager.instance.currentPosition}");
        room.onEnter?.Invoke(new());
        GameEvent.onEnterRoom?.Invoke(new(coordinate, room));
    }

    private void FinishRun()
    {
        houseStats.ShowStatsAndEnd();
    }
}
