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

    private List<ItemButton> roomItems = new();
    private List<ItemButton> playerItems = new();

    private void Awake()
    {
        GameEvent.onCollectItem += _ => UpdateItems();
        floorplanDetails.onPickedFloorplan += OnClickRoom;
        autoPickupToggle.onValueChanged.AddListener(on => GameSettings.current.autoCollectItems = on);
        openDetailsButton.onClick.AddListener(OpenDetails);
        CloseBag();
    }

    private void OpenDetails() => UIManager.ShowDetails(Helpers.CurrentRoom());

    private void UpdateItems()
    {
        SetupFloorplanItems(Helpers.CurrentRoom());
        SetupPlayerItems();
    }

    private void SetupFloorplanItems(Room room)
    {
        int requiredItems = room.items.Count;
        roomItems.EnsureEnoughInstances(itemPrefab, requiredItems, floorplanContainer);
        for (int i = 0; i < requiredItems; i++)
        {
            Item item = room.items[i];
            ItemButton button = roomItems[i];
            button.onClick = null;
            button.Setup(item);
            button.onClick += () => room.PickupItem(item);
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

    private void OnClickRoom(Room room)
    {
        Glossary.OpenGlossary(room);
    }

    public void OpenBag()
    {
        gameObject.SetActive(true);
        Room room = Helpers.CurrentRoom();
        floorplanDetails.Setup(room);
        autoPickupToggle.isOn = GameSettings.current.autoCollectItems;
        UpdateItems();
    }

    public void CloseBag()
    {
        gameObject.SetActive(false);
    }
}
