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
        foreach (var floorplan in GameManager.floorplanDict.Values)
            SetupFloorplan(floorplan);
        GameEvent.onDraftedFloorplan += PlaceFloorplan;
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
        if (!GameManager.floorplanDict.TryGetValue(coordinates, out var floorplan)) return;
        UIManager.ShowDetails(floorplan);
    }

    private void PlaceFloorplan(FloorplanEvent evt)
    {
        SetupFloorplan(evt.Floorplan);
    }

    private void SetupFloorplan(Floorplan floorplan)
    {
        Button slotButton = minimapGrid.GetSlot(floorplan.coordinate);
        FloorplanUI instance = Instantiate(floorplanPrefab, slotButton.transform);
        instance.Setup(floorplan);
        RectTransform floorplanRect = (RectTransform)instance.transform;
        floorplanRect.anchoredPosition = Vector2.zero;
        floorplanRect.anchorMin = Vector2.zero;
        floorplanRect.anchorMax = Vector2.one;
        floorplanRect.sizeDelta = Vector2.zero;

        slotButton.onClick.AddListener(() => UIManager.ShowDetails(floorplan));
    }

    public void OpenMinimap()
    {
        minimapContainer.SetActive(true);
        int currentPoints = PointsManager.GetTotalPoints();
        totalPointsSlider.maxValue = PointsManager.currentRequirement;
        totalPointsSlider.value = currentPoints;
        totalPoints.text = $"{currentPoints}/{PointsManager.currentRequirement}";
        StartCoroutine(DelayedUpdate());
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
