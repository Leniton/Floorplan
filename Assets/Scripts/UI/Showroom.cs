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

    [Header("Windows")] 
    [SerializeField] private DraftManager draftManager;
    [SerializeField] private ShopWindow shopWindow;

    private List<PurchaseData> renovationPack = new();
    private List<PurchaseData> suppliesPack = new();
    
    private void Awake()
    {
        SetupSuppliesShop();
        SetupRenovationsShop();
        draftManager.Setup(5, RunData.playerDeck);
        draftManager.CloseWindow();
        
        renovationPackButton.onClick.AddListener(OpenRenovationShop);
        suppliesPackButton.onClick.AddListener(OpenSuppliesShop);
        continueButton.onClick.AddListener(ContinueGame);
    }

    private void SetupRenovationsShop()
    {
        RarityPicker<PurchaseData> possibleRenovations = new();
        possibleRenovations.AddToPool(new()
        {
            name = "Key holder",
            description = "Add 2 keys to a floorplan",
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.KeyHolder())
        }, Rarity.Common);
        possibleRenovations.AddToPool(new()
        {
            name = "Secret vault",
            description = "Add 5 coins to a floorplan",
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.SecretVault())
        }, Rarity.Common);
        possibleRenovations.AddToPool(new()
        {
            name = "Mini Fridge",
            description = "Add a <b>Soda</b> to a floorplan",
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.MiniFridge())
        }, Rarity.Common);
        possibleRenovations.AddToPool(new()
        {
            name = "Wallpaper",
            description = "+5 base points to floorplan",
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.Wallpaper())
        }, Rarity.Common);
        possibleRenovations.AddToPool(new()
        {
            name = "Play table",
            description = "Add 2 <b>Dices</b> to floorplan",
            cost = 3,
            amount = 1,
            OnBuy = () => UseRenovation(RenovationUtils.PlayTable())
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
        possibleSupplies.Add(new()
        {
            name = "Apple",
            description = "Gain +3 steps",
            cost = 3,
            amount = 10,
            OnBuy = () => AddSupply(ItemUtilities.Apple)
        });
        possibleSupplies.Add(new()
        {
            name = "Key",
            description = "Used to draft powerful floorplans",
            cost = 3,
            amount = 10,
            OnBuy = () => AddSupply(new Key(1))
        });
        possibleSupplies.Add(new()
        {
            name = "Dice",
            description = "Used to reroll drawn floorplans",
            cost = 5,
            amount = 5,
            OnBuy = () => AddSupply(new Dice(1))
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
        draftManager.DraftFloorplan(Vector2Int.up, null);
        draftManager.OnDraftFloorplan += ApplyRenovation;

        void ApplyRenovation(Floorplan floorplan)
        {
            var original = floorplan.FindOriginal(RunData.playerDeck.deck);
            if (original != null) renovation.Activate(original);
            draftManager.OnDraftFloorplan -= ApplyRenovation;
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
