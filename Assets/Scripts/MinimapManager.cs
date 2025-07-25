using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        GameEvent.onDraftedFloorplan += PlaceFloorplan;
    }

    private void Start()
    {
        CloseMinimap();        
    }

    private void PlaceFloorplan(Vector2Int coordinates, Floorplan floorplan)
    {
        Button slotButton = minimapGrid.GetSlot(coordinates).GetComponent<Button>();
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
        playerPosition.SetParent(minimapGrid.GetSlot(gameGrid.currentPosition), false);
    }

    public void CloseMinimap()
    {
        minimapContainer.SetActive(false);
    }
}
