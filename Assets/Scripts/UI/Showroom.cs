using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Button rerollButton;
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
        pickFloorplan.Setup(5, RunData.playerDeck, pickFloorplan.CloseWindow);

        addFloorplan.Setup(3, RunData.allFloorplans, () => addFloorplan.DraftFloorplan());
        addFloorplan.OnDraftFloorplan += OnPickFloorplanToAdd;
        
        renovationPackButton.onClick.AddListener(OpenRenovationShop);
        suppliesPackButton.onClick.AddListener(OpenSuppliesShop);
        rerollButton.onClick.AddListener(RerollAddFloorplans);
        continueButton.onClick.AddListener(ContinueGame);
    }

    private void RerollAddFloorplans()
    {
        addFloorplan.RedrawFloorplans();
    }

    private void OnPickFloorplanToAdd(Floorplan floorplan)
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
        FloorCategory paintCategory = Helpers.RandomCategory();
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
        int amountApple = 0;
        var apple = new Food(0);
        possibleSupplies.Add(new()
        {
            name = "Apple",
            description = "Gain +3 steps",
            cost = 3,
            amount = 10,
            OnBuy = () =>
            {
                apple.stepsAmount += 3;
                amountApple++;
                apple.Name = $"Apple({amountApple})";
                if (amountApple > 1) return;
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
                key.keyAmount++;
                key.Name = $"Key({key.keyAmount})";
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
                dice.diceAmount++;
                dice.Name = $"Dice({dice.diceAmount})";
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
        renovationPick.text = renovation.description;
        if (renovation.condition != null)
            GameEvent.onDrawFloorplans += CheckCondition;
        pickFloorplan.DraftFloorplan();
        pickFloorplan.OnDraftFloorplan += ApplyRenovation;

        void ApplyRenovation(Floorplan floorplan)
        {
            pickFloorplan.CloseWindow();
            pickFloorplan.OnDraftFloorplan -= ApplyRenovation;

            var original = floorplan.FindOriginal(RunData.playerDeck.deck);
            if (original != null) renovation.Activate(original);
            GameEvent.onDrawFloorplans -= CheckCondition;
        }

        void CheckCondition(DrawFloorplanEvent evt)
        {
            evt.IncreaseChanceOfDrawing(renovation.condition, 1);
        }
    }

    private void AddSupply(Item item)
    {
        GameEvent.onGameStart += AddItemToEntranceHall;

        void AddItemToEntranceHall(Event evt)
        {
            item.AddItemToFloorplan(GameManager.EntranceHall);
            GameEvent.onGameStart -= AddItemToEntranceHall;
        }
    }

    private void ContinueGame()
    {
        SceneManager.LoadScene(1);
    }
}
