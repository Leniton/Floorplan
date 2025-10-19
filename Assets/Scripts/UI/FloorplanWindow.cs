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

    private Room currentRoom;
    private List<TMP_Text> itemsTexts = new();
    private List<TMP_Text> pointsTexts = new();

    private void Awake()
    {
        floorplanDetails.onPickedFloorplan += OnClickFloorplan;
        GameEvent.onCollectItem += _ => SetupItems();
    }

    public void SetupWindow(Room room)
    {
        currentRoom = room;
        SetupData();
    }

    private void SetupData()
    {
        floorplanDetails.Setup(currentRoom);
        SetupItems();
        SetupPoints();
    }

    private void SetupItems()
    {
        int requiredTexts = currentRoom.items.Count;
        itemsTexts.EnsureEnoughInstances(textPrefab, requiredTexts, itemsContent);

        for (int i = 0;i < requiredTexts; i++)
            itemsTexts[i].text = $"{currentRoom.items[i].Name}";
    }

    private void SetupPoints()
    {
        basePointsText.text = $"Base: {currentRoom.basePoints}";
        int requiredTexts = currentRoom.pointBonus.Count + currentRoom.multBonus.Count;
        pointsTexts.EnsureEnoughInstances(textPrefab , requiredTexts, pointsContent);

        int id = 0;
        foreach (var bonus in currentRoom.pointBonus)
        {
            int points = bonus.Value.Invoke();
            pointsTexts[id].text = $"{bonus.Key} => {(points > 0 ? "+" : string.Empty) + $"{points}"}";
            id++;
        }
        foreach (var mult in currentRoom.multBonus)
        {
            pointsTexts[id].text = $"{mult.Key} => {mult.Value.Invoke()}x";
            id++;
        }
    }

    private void OnClickFloorplan(Room room)
    {
        Glossary.OpenGlossary(room);
    }
}
