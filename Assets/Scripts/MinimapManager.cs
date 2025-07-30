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

    private void Awake()
    {
        GameEvent.onDraftedFloorplan += PlaceFloorplan;
    }

    private void Start()
    {
        CloseMinimap();        
    }

    private void PlaceFloorplan(GenericFloorplanEvent evt)
    {
        Button slotButton = minimapGrid.GetSlot(evt.Coordinates).GetComponent<Button>();
        FloorplanUI instance = Instantiate(floorplanPrefab, slotButton.transform);
        instance.Setup(evt.Floorplan);
        RectTransform floorplanRect = (RectTransform)instance.transform;
        floorplanRect.anchoredPosition = Vector2.zero;
        floorplanRect.anchorMin = Vector2.zero;
        floorplanRect.anchorMax = Vector2.one;
        floorplanRect.sizeDelta = Vector2.zero;

        slotButton.onClick.AddListener(() => UIManager.ShowDetails(evt.Floorplan));
    }

    public void OpenMinimap()
    {
        minimapContainer.SetActive(true);
        playerPosition.SetParent(minimapGrid.GetSlot(gameGrid.currentPosition), false);
        int currentPoints = PointsManager.GetTotalPoints();
        totalPointsSlider.maxValue = PointsManager.currentRequirement;
        totalPointsSlider.value = currentPoints;
        totalPoints.text = $"{currentPoints}/{PointsManager.currentRequirement}";
    }

    public void CloseMinimap()
    {
        minimapContainer.SetActive(false);
    }
}
