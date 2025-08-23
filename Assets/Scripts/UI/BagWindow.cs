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

    private void Awake()
    {
        autoPickupToggle.onValueChanged.AddListener(on => GameSettings.current.autoCollectItems = on);
        openDetailsButton.onClick.AddListener(OpenDetails);
        CloseBag();
    }

    private void OpenDetails() => UIManager.ShowDetails(Helpers.CurrentFloorplan());

    private void UpdateItems()
    {
        SetupFloorplanItems(Helpers.CurrentFloorplan());
        SetupPlayerItems();
    }

    private void SetupFloorplanItems(Floorplan floorplan)
    {
        int requiredItems = floorplan.items.Count;
        floorplanItems.EnsureEnoughInstances(itemPrefab, requiredItems, floorplanContainer);
        for (int i = 0; i < requiredItems; i++)
        {
            Item item = floorplan.items[i];
            ItemButton button = floorplanItems[i];
            button.onClick = null;
            button.Setup(item);
            button.onClick += () => floorplan.PickupItem(item);
            button.onClick += OpenBag;
        }
    }

    private void SetupPlayerItems()
    {
        int requiredItems = Player.items.Count;
        playerItems.EnsureEnoughInstances(itemPrefab, requiredItems, playerContainer);
        for (int i = 0; i < requiredItems; i++)
        {
            Item item = Player.items[i];
            ItemButton button = playerItems[i];
            button.onClick = null;
            button.Setup(item);
            button.onClick += item.Activate;
            button.onClick += OpenBag;
        }
    }

    public void OpenBag()
    {
        gameObject.SetActive(true);
        Floorplan floorplan = Helpers.CurrentFloorplan();
        floorplanDetails.Setup(floorplan);
        autoPickupToggle.isOn = GameSettings.current.autoCollectItems;
        UpdateItems();
    }

    public void CloseBag()
    {
        gameObject.SetActive(false);
    }
}
