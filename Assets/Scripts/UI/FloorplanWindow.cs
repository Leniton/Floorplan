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
        if (pointsTexts.Count < requiredTexts)
        {
            int diff = requiredTexts - itemsTexts.Count;
            for (int i = 0; i < diff; i++)
                itemsTexts.Add(Instantiate(textPrefab, itemsContent));
        }

        for (int i = 0;i < requiredTexts; i++)
        {
            itemsTexts[i].gameObject.SetActive(true);
            itemsTexts[i].text = $"{currentFloorplan.items[i].Name}";
        }
        for (int i = requiredTexts; i < itemsTexts.Count; i++)
            itemsTexts[i].gameObject.SetActive(false);
    }

    private void SetupPoints()
    {
        basePointsText.text = $"Base: {currentFloorplan.basePoints}";
        int requiredTexts = currentFloorplan.pointBonus.Count + currentFloorplan.multBonus.Count;
        if (pointsTexts.Count < requiredTexts)
        {
            int diff = requiredTexts - pointsTexts.Count;
            for (int i = 0; i < diff; i++)
                pointsTexts.Add(Instantiate(textPrefab, pointsContent));
        }

        int id = 0;
        foreach (var bonus in currentFloorplan.pointBonus)
        {
            int points = bonus.Value.Invoke();
            pointsTexts[id].gameObject.SetActive(true);
            pointsTexts[id].text = $"{bonus.Key} => {(points > 0 ? "+" : string.Empty) + $"{points}"}";
            id++;
        }
        foreach (var mult in currentFloorplan.multBonus)
        {
            pointsTexts[id].gameObject.SetActive(true);
            pointsTexts[id].text = $"{mult.Key} => {mult.Value.Invoke()}x";
            id++;
        }
        for (int i = id; i < pointsTexts.Count; i++)
            pointsTexts[i].gameObject.SetActive(false);
    }
}
