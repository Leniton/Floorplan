using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DraftManager : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject draftScreen;
    [SerializeField] private FloorplanDetails floorplanUI;
    [SerializeField] private List<Floorplan> draftPool;

    private int amountDrafted = 3;
    private List<FloorplanDetails> draftList;

    public event Action<Floorplan> OnDraftFloorplan;

    private void Awake()
    {
        draftList = new(amountDrafted);
        for (int i = 0; i < amountDrafted; i++)
        {
            FloorplanDetails instance = Instantiate(floorplanUI, draftScreen.transform);
            instance.onPickedFloorplan += PickFloorplan;
            draftList.Add(instance);
        }
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
        List<Floorplan> possibleFloors = new();
        for (int i = 0;i < draftPool.Count; i++)
        {
            if (!possibleTypes.Contains(draftPool[i].Type)) continue;
            Floorplan floorplan = draftPool[i].CreateInstance(-direction);
            int randomRotation = Random.Range(1, 3);
            for (int j = 0; j < randomRotation; j++) floorplan.Rotate();
            CorrectFloorplanRotation(floorplan, possibleSlots);
            possibleFloors.Add(floorplan);
        }

        for (int i = 0; i < amountDrafted; i++)
        {
            if (possibleFloors.Count <= 0) break;
            int id = Random.Range(0, possibleFloors.Count);
            Floorplan floorplan = possibleFloors[id];
            possibleFloors.RemoveAt(id);

            FloorplanDetails instance = draftList[i];
            instance.Setup(floorplan);
        }

        background.SetActive(true);
        draftScreen.SetActive(true);
    }

    public void CorrectFloorplanRotation(Floorplan floorplan, List<Vector2Int> possibleSlots)
    {
        if (floorplan.Type == FloorType.DeadEnd || floorplan.Type == FloorType.Straw) return;

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
}
