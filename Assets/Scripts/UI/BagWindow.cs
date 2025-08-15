using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagWindow : MonoBehaviour
{
    [SerializeField] private Toggle autoPickupToggle;
    [SerializeField] private Button openDetailsButton;
    [SerializeField] private FloorplanDetails floorplanDetails;
    [SerializeField] private ItemButton itemPrefab;

    private Floorplan currentFloorplan => Helpers.CurrentFloorplan();

    public void OpenBag()
    {

    }
}
