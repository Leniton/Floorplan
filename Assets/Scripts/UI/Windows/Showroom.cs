using System;
using System.Collections;
using System.Collections.Generic;
using Lenix.NumberUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Showroom : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform buyFloorplanContainer;
    [SerializeField] private Button renovationPackButton;
    [SerializeField] private Button suppliesPackButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text rerollAmountText;
    [SerializeField] private TMP_Text renovationPick;

    [Header("Windows")] 
    [SerializeField] private DraftManager addFloorplan;
    [SerializeField] private DraftManager pickFloorplan;
    [SerializeField] private ShopWindow shopWindow;

    private List<PurchaseData> renovationPack = new();
    private List<PurchaseData> suppliesPack = new();
    
    private void Awake()
    {
        SetupSuppliesShop();
        SetupRenovationsShop();
        pickFloorplan.Setup(5, RunData.playerDeck, _ => pickFloorplan.CloseWindow());

        GameEvent.onDrawRooms += DeckBias;
        addFloorplan.Setup(3, RunData.allRooms, _ => addFloorplan.DraftRoom());
        addFloorplan.OnDraftRoom += OnPickFloorplanToAdd;
        
        renovationPackButton.onClick.AddListener(OpenRenovationShop);
        suppliesPackButton.onClick.AddListener(OpenSuppliesShop);
        continueButton.onClick.AddListener(ContinueGame);
    }

    private void DeckBias(DrawRoomEvent evt)
    {
        if(RunData.playerDeck.preferredCategory == 0) return;
        evt.IncreaseChanceOfDrawing(floorplan => 
            NumberUtil.ContainsAnyBits((int)floorplan.Category, (int)RunData.playerDeck.preferredCategory), 
            .2f);
    }

    private void OnPickFloorplanToAdd(Room floorplan)
    {
        addFloorplan.CloseWindow();
        RunData.playerDeck.deck.Add(floorplan);
    }

    private void SetupRenovationsShop()
    {
        RarityPicker<PurchaseData> possibleRenovations = new();
        var renovation = RenovationUtils.KeyHolder();
        possibleRenovations.AddToPool(new()
        {
            name = renovation.name,
            description = renovation.description,
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.KeyHolder()),
            pattern = renovation.overlayPattern
        }, Rarity.Common);
        renovation = RenovationUtils.SecretVault();
        possibleRenovations.AddToPool(new()
        {
            name = renovation.name,
            description = renovation.description,
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.SecretVault()),
            pattern = renovation.overlayPattern
        }, Rarity.Common);
        renovation = RenovationUtils.MiniFridge();
        possibleRenovations.AddToPool(new()
        {
            name = renovation.name,
            description = renovation.description,
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.MiniFridge()),
            pattern = renovation.overlayPattern
        }, Rarity.Common);
        renovation = RenovationUtils.PlayTable();
        possibleRenovations.AddToPool(new()
        {
            name = renovation.name,
            description = renovation.description,
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.PlayTable()),
            pattern = renovation.overlayPattern
        }, Rarity.Common);
        renovation = RenovationUtils.Wallpaper();
        possibleRenovations.AddToPool(new()
        {
            name = renovation.name,
            description = renovation.description,
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.Wallpaper())
        }, Rarity.Common);
        RoomCategory paintCategory = Helpers.RandomCategory();
        Renovation paint = RenovationUtils.Paint(paintCategory);
        possibleRenovations.AddToPool(new()
        {
            name = paint.name,
            description = paint.description,
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.Paint(paintCategory))
        }, Rarity.Common);
        renovation = RenovationUtils.WallMirror();
        possibleRenovations.AddToPool(new()
        {
            name = renovation.name,
            description = renovation.description,
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.WallMirror())
        }, Rarity.Common);
        renovation = RenovationUtils.NewDoor();
        possibleRenovations.AddToPool(new()
        {
            name = renovation.name,
            description = renovation.description,
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.NewDoor())
        }, Rarity.Common);
        renovation = RenovationUtils.Demolition();
        possibleRenovations.AddToPool(new()
        {
            name = renovation.name,
            description = renovation.description,
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.Demolition())
        }, Rarity.Common);
        renovation = RenovationUtils.SealedDoor();
        possibleRenovations.AddToPool(new()
        {
            name = renovation.name,
            description = renovation.description,
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.SealedDoor())
        }, Rarity.Common);
        
        List<PurchaseData> renovations = new(3);
        for (int i = 0; i < renovations.Capacity; i++)
        {
            renovations.Add(possibleRenovations.PickRandom(removeFromPool: true));
        }
        renovationPack = renovations;
    }
    
    private void SetupSuppliesShop()
    {
        List<PurchaseData> possibleSupplies = new();
        var apple = ItemUtilities.Apple;
        apple.amount = 0;
        possibleSupplies.Add(new()
        {
            name = "Apple",
            description = "Gain +3 steps",
            cost = 3,
            amount = 10,
            OnBuy = () =>
            {
                apple.amount++;
                if (apple.amount > 1) return;
                AddSupply(apple);
            }
        });
        var key = new Key(0);
        possibleSupplies.Add(new()
        {
            name = "Key",
            description = "Used to draft powerful floorplans",
            cost = 3,
            amount = 10,
            OnBuy = () =>
            {
                key.amount++;
                if (key.keyAmount > 1) return;
                AddSupply(key);
            }
        });
        var dice = new Dice(0);
        possibleSupplies.Add(new()
        {
            name = "Dice",
            description = "Used to reroll drawn floorplans",
            cost = 5,
            amount = 5,
            OnBuy = () =>
            {
                dice.amount++;
                if (dice.diceAmount > 1) return;
                AddSupply(dice);
            }
        });
        ColorKey colorKey = new();
        possibleSupplies.Add(new()
        {
            cost = 8,
            amount = 2,
            name = colorKey.Name,
            description = $"Guarantee you draw {Helpers.CategoryName(colorKey.floorCategory)}s",
            OnBuy = () => AddSupply(new ColorKey(colorKey.floorCategory))
        });
        possibleSupplies.Add(new()
        {
            name = "SledgeHammer",
            description = "Move towards an closed connection to open it.",
            cost = 10,
            amount = 1,
            OnBuy = () => AddSupply(new SledgeHammer())
        });
        suppliesPack = possibleSupplies;
    }

    private void OpenRenovationShop()
    {
        ShopWindow.OpenShop("Renovations", renovationPack);
    }

    private void OpenSuppliesShop()
    {
        ShopWindow.OpenShop("Supplies Shop", suppliesPack);
    }

    private void UseRenovation(Renovation renovation)
    {
        GameEvent.onDrawRooms -= DeckBias;
        renovationPick.text = renovation.description;
        if (renovation.condition != null)
            GameEvent.onDrawRooms += CheckCondition;
        pickFloorplan.DraftRoom();
        pickFloorplan.OnDraftRoom += ApplyRenovation;

        void ApplyRenovation(Room floorplan)
        {
            GameEvent.onDrawRooms += DeckBias;
            pickFloorplan.CloseWindow();
            pickFloorplan.OnDraftRoom -= ApplyRenovation;

            var original = floorplan.FindOriginal(RunData.playerDeck.deck);
            if (original != null) renovation.Activate(original);
            GameEvent.onDrawRooms -= CheckCondition;
        }

        void CheckCondition(DrawRoomEvent evt)
        {
            evt.IncreaseChanceOfDrawing(renovation.condition, 1);
        }
    }

    private void AddSupply(Item item)
    {
        GameEvent.onGameStart += AddItemToEntranceHall;

        void AddItemToEntranceHall(Event evt)
        {
            item.AddItemToRoom(GameManager.EntranceHall);
            GameEvent.onGameStart -= AddItemToEntranceHall;
        }
    }

    private void ContinueGame()
    {
        GameEvent.onDrawRooms -= DeckBias;
        SceneManager.LoadScene(1);
    }
}
