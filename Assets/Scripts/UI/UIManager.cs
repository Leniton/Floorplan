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
    [SerializeField] private Button playerItemsButton;
    [SerializeField] private PlayerItemsView playerItemsView;

    private static UIManager instance;

    private void Awake()
    {
        instance = this;
        playerItemsButton?.onClick.AddListener(playerItemsView.Open);
        playerItemsView?.Close();
    }

    public static void ShowDetails(Room room)
    {
        instance.details.SetupWindow(room);
        instance.details.gameObject.SetActive(true);
    }

    public static void ShowCurrentFloorplan()
    {
        instance.currentDetails.OpenBag();
    }
}
