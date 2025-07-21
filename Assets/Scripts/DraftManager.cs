using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DraftManager : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject draftScreen;
    [SerializeField] private Button rerollButton;
    [SerializeField] private FloorplanDetails floorplanUI;
    [SerializeField] private List<Floorplan> allFloorplans;

    private int amountDrafted = 3;
    private List<FloorplanDetails> draftList;
    private List<Floorplan> draftPool;

    public event Action<Floorplan> OnDraftFloorplan;

    private TMP_Text rerollCount;
    private Vector2Int lastDraftDirection;
    private List<Vector2Int> lastPossibleSlots;

    private void Awake()
    {
        draftList = new(amountDrafted);
        for (int i = 0; i < amountDrafted; i++)
        {
            FloorplanDetails instance = Instantiate(floorplanUI, draftScreen.transform);
            instance.onPickedFloorplan += PickFloorplan;
            draftList.Add(instance);
        }

        draftPool = new(allFloorplans.Count);
        for (int i = 0; i < allFloorplans.Count; i++)
            draftPool.Add(allFloorplans[i].CreateInstance(Vector2Int.up));

        rerollButton.onClick.AddListener(RedrawFloorplans);
        rerollCount = rerollButton.GetComponentInChildren<TMP_Text>();
        background.SetActive(false);
        draftScreen.SetActive(false);
    }

    public void DraftFloorplan(Vector2Int direction, List<Vector2Int> possibleSlots)
    {
        List<FloorType> possibleTypes = new()
        {
            FloorType.DeadEnd,
            FloorType.Ankle,
            FloorType.Straw,
            FloorType.TPiece,
            FloorType.Crossroad,
        };

        if (possibleSlots.Count < 4) possibleTypes.Remove(FloorType.Crossroad);
        if (possibleSlots.Count < 3) possibleTypes.Remove(FloorType.TPiece);
        if (possibleSlots.Count < 2) possibleTypes.Remove(FloorType.Ankle);
        if (!possibleSlots.Contains(direction)) possibleTypes.Remove(FloorType.Straw);

        StringBuilder sb = new();
        for (int i = 0; i < possibleTypes.Count; i++)
            sb.Append($"{possibleTypes[i]} | ");
        //Debug.Log(sb.ToString());

        //pick possible ones
        RarityPicker<Floorplan> possibleFloors = new();
        for (int i = 0; i < draftPool.Count; i++)
        {
            if (!possibleTypes.Contains(draftPool[i].Type)) continue;
            Floorplan floorplan = draftPool[i];
            possibleFloors.AddToPool(floorplan, floorplan.Rarity);
        }

        lastDraftDirection = direction;
        lastPossibleSlots = possibleSlots;
        rerollButton.gameObject.SetActive(Player.dices > 0);
        rerollCount?.SetText($"{Player.dices}");

        DrawFloorplanEvent evt = new();
        evt.drawnFloorplans = new Floorplan[3];

        for (int i = 0; i < amountDrafted - 1; i++) AddToDraftList(i);
        //last one is rarer
        AddToDraftList(amountDrafted - 1, possibleFloors.commonRate);
        GameEvent.onDrawFloorplans?.Invoke(evt);

        void AddToDraftList(int id, float rarityOffset = 0)
        {
            Floorplan refFloorplan = possibleFloors.PickRandom(rarityOffset, true);
            Floorplan floorplan = refFloorplan.CreateInstance(-direction);
            int randomRotation = Random.Range(0, 3);
            for (int j = 0; j < randomRotation; j++) floorplan.Rotate();
            CorrectFloorplanRotation(floorplan, possibleSlots);
            evt.drawnFloorplans[id] = floorplan;
        }


        for (int i = 0; i < amountDrafted; i++)
        {
            FloorplanDetails instance = draftList[i];
            instance.Setup(evt.drawnFloorplans[i]);
        }

        background.SetActive(true);
        draftScreen.SetActive(true);
    }

    public void CorrectFloorplanRotation(Floorplan floorplan, List<Vector2Int> possibleSlots)
    {
        if (floorplan.Type != FloorType.Ankle && floorplan.Type != FloorType.TPiece) return;

        bool invalidConnection = false;
        do
        {
            invalidConnection = false;
            for (int i = 0; i < floorplan.connections.Length; i++)
            {
                if (!floorplan.connections[i]) continue;
                if (possibleSlots.Contains(Floorplan.IDToDirection(i))) continue;
                invalidConnection = true;
                break;
            }
            if (invalidConnection) floorplan.Rotate();
        } while (invalidConnection);
    }

    public void PickFloorplan(Floorplan floorplan)
    {
        background.SetActive(false);
        draftScreen.SetActive(false);
        draftPool.Remove(floorplan.original);
        OnDraftFloorplan?.Invoke(floorplan);
    }

    public void RotateFloorplans()
    {
        for (int i = 0; i < draftList.Count; i++)
        {
            draftList[i].floorplan.Rotate();
            draftList[i].Setup(draftList[i].floorplan);
        }
    }

    public void RedrawFloorplans()
    {
        Player.dices--;
        DraftFloorplan(lastDraftDirection, lastPossibleSlots);
    }
}

public class DrawFloorplanEvent
{
    public Floorplan[] drawnFloorplans;
}