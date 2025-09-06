using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private FloorplanWindow details;
    [SerializeField] private BagWindow currentDetails;

    private static UIManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static void ShowDetails(Floorplan floorplan)
    {
        instance.details.SetupWindow(floorplan);
        instance.details.gameObject.SetActive(true);
    }

    public static void ShowCurrentFloorplan()
    {
        instance.currentDetails.OpenBag();
    }
}
