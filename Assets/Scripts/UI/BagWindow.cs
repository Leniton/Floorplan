using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagWindow : MonoBehaviour
{
    [SerializeField] private FloorplanDetails floorplanDetails;
    [SerializeField] private ItemButton itemPrefab;

    private Floorplan currentFloorplan => Helpers.CurrentFloorplan();

    public void OpenBag()
    {

    }
}
