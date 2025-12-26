using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    [SerializeField] private GridManager gameGrid;
    [Header("Self")]
    [SerializeField] private GameObject minimapContainer;
    [SerializeField] private GridManager minimapGrid;
    [SerializeField] private FloorplanUI floorplanPrefab;
    [SerializeField] private RectTransform playerPosition;
    [Header("points")]
    [SerializeField] private TMP_Text totalPoints;
    [SerializeField] private Slider totalPointsSlider;
    [SerializeField] private ValueSlider totalPointsValue;
    [Header("PeekFloorplan")]
    [SerializeField] private HoverMenu peekButtons;
    [SerializeField] private Sprite arrowIcon;
    [SerializeField] private Color buttonColor = Color.white;
    

    private void Awake()
    {
        minimapGrid.onDoneLoading += SetupGridMirror;
    }

    private void SetupGridMirror()
    {
        totalPointsValue.UpdateMaxValue(PointsManager.currentRequirement);
        totalPointsValue.SetValue(0);
        foreach (var floorplan in GameManager.roomDict.Values)
            SetupRoom(floorplan);
        GameEvent.onDraftedRoom += PlaceRoom;
    }

    private void Start()
    {
        peekButtons.SetupOptions(new()
        {
            //up
            new()
            {
                icon = arrowIcon,
                buttonColor = buttonColor,
                onPick = () => PeekAt(Vector2Int.up),
            },
            //left
            new()
            {
                icon = arrowIcon,
                buttonColor = buttonColor,
                onPick = () => PeekAt(Vector2Int.left),
            },
            //down
            new()
            {
                icon = arrowIcon,
                buttonColor = buttonColor,
                onPick = () => PeekAt(Vector2Int.down),
            },
            //right
            new()
            {
                icon = arrowIcon,
                buttonColor = buttonColor,
                onPick = () => PeekAt(Vector2Int.right),
            },
        }, UIManager.ShowCurrentFloorplan);

        CloseMinimap();
    }

    private void PeekAt(Vector2Int direction)
    {
        Vector2Int coordinates = gameGrid.currentPosition + direction;
        if (!gameGrid.ValidCoordinate(coordinates)) return;
        if (!GameManager.roomDict.TryGetValue(coordinates, out var room)) return;
        UIManager.ShowDetails(room);
    }

    private void PlaceRoom(RoomEvent evt)
    {
        SetupRoom(evt.Room);
    }

    private void SetupRoom(Room room)
    {
        room.OnChanged += CalculatePoints;
        Button slotButton = minimapGrid.GetSlot(room.coordinate);
        FloorplanUI instance = Instantiate(floorplanPrefab, slotButton.transform);
        instance.Setup(room);
        RectTransform roomRect = (RectTransform)instance.transform;
        roomRect.anchoredPosition = Vector2.zero;
        roomRect.anchorMin = Vector2.zero;
        roomRect.anchorMax = Vector2.one;
        roomRect.sizeDelta = Vector2.zero;

        slotButton.onClick.AddListener(() => UIManager.ShowDetails(room));
    }

    public void OpenMinimap()
    {
        minimapContainer.SetActive(true);
        CalculatePoints();
        StartCoroutine(DelayedUpdate());
    }

    private void CalculatePoints()
    {
        int currentPoints = PointsManager.GetTotalPoints();
        totalPointsValue.ChangeToValue(currentPoints);
        //totalPointsSlider.maxValue = PointsManager.currentRequirement;
        //totalPointsSlider.value = currentPoints;
        //totalPoints.text = $"{currentPoints}/{PointsManager.currentRequirement}";
    }

    private IEnumerator DelayedUpdate()
    {
        yield return null;
        playerPosition.anchoredPosition = minimapGrid.GetSlotRect(gameGrid.currentPosition).anchoredPosition;
    }

    public void CloseMinimap()
    {
        minimapContainer.SetActive(false);
    }
}
