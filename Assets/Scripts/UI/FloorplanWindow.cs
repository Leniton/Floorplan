using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloorplanWindow : MonoBehaviour
{
    [SerializeField] private FloorplanDetails floorplanDetails;
    [SerializeField] private TMP_Text textPrefab;
    [Header("Items")]
    [SerializeField] private RectTransform itemsContent;
    [Header("Points")]
    [SerializeField] private TMP_Text basePointsText;
    [SerializeField] private RectTransform pointsContent;

    private Floorplan currentFloorplan;
    private List<TMP_Text> itemsTexts = new();
    private List<TMP_Text> pointsTexts = new();

    private void Awake()
    {
        floorplanDetails.onPickedFloorplan += OnClickFloorplan;
        GameEvent.OnCollectItem += _ => SetupItems();
    }

    public void SetupWindow(Floorplan floorplan)
    {
        currentFloorplan = floorplan;
        SetupData();
    }

    private void SetupData()
    {
        floorplanDetails.Setup(currentFloorplan);
        SetupItems();
        SetupPoints();
    }

    private void SetupItems()
    {
        int requiredTexts = currentFloorplan.items.Count;
        itemsTexts.EnsureEnoughInstances(textPrefab, requiredTexts, itemsContent);

        for (int i = 0;i < requiredTexts; i++)
            itemsTexts[i].text = $"{currentFloorplan.items[i].Name}";
    }

    private void SetupPoints()
    {
        basePointsText.text = $"Base: {currentFloorplan.basePoints}";
        int requiredTexts = currentFloorplan.pointBonus.Count + currentFloorplan.multBonus.Count;
        pointsTexts.EnsureEnoughInstances(textPrefab , requiredTexts, pointsContent);

        int id = 0;
        foreach (var bonus in currentFloorplan.pointBonus)
        {
            int points = bonus.Value.Invoke();
            pointsTexts[id].text = $"{bonus.Key} => {(points > 0 ? "+" : string.Empty) + $"{points}"}";
            id++;
        }
        foreach (var mult in currentFloorplan.multBonus)
        {
            pointsTexts[id].text = $"{mult.Key} => {mult.Value.Invoke()}x";
            id++;
        }
    }

    private void OnClickFloorplan(Floorplan floorplan)
    {
        Glossary.OpenGlossary(floorplan);
    }
}
