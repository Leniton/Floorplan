using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagWindow : MonoBehaviour
{
    [SerializeField] private Toggle autoPickupToggle;
    [SerializeField] private Button openDetailsButton;
    [SerializeField] private FloorplanDetails floorplanDetails;
    [Header("Items")]
    [SerializeField] private ItemButton itemPrefab;
    [SerializeField] private Transform floorplanContainer;
    [SerializeField] private Transform playerContainer;

    private List<ItemButton> floorplanItems = new();
    private List<ItemButton> playerItems = new();

    public void OpenBag()
    {
        Floorplan floorplan = Helpers.CurrentFloorplan();
        floorplanDetails.Setup(floorplan);
        autoPickupToggle.isOn = GameSettings.current.autoCollectItems;
        UpdateItems();
    }

    private void UpdateItems()
    {
        SetupFloorplanItems(Helpers.CurrentFloorplan());
        SetupPlayerItems();
    }

    private void SetupFloorplanItems(Floorplan floorplan)
    {
        int requiredItems = floorplan.items.Count;
        floorplanItems.EnsureEnoughInstances(itemPrefab, requiredItems, floorplanContainer);
        for (int i = 0; i < requiredItems; i++) floorplanItems[i].Setup(floorplan.items[i]);
    }

    private void SetupPlayerItems()
    {
        int requiredItems = Player.items.Count;
        playerItems.EnsureEnoughInstances(itemPrefab, requiredItems, playerContainer);
        for (int i = 0; i < requiredItems; i++) playerItems[i].Setup(Player.items[i]);
    }
}
