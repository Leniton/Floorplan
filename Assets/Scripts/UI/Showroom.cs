using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    }

    private void SetupRenovationsShop()
    {
        
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
            OnBuy = () => ItemUtilities.Apple.PickUp()
        });
        possibleSupplies.Add(new()
        {
            name = "Key",
            description = "Used to draft powerful floorplans",
            cost = 3,
            amount = 10,
            OnBuy = () => new Key(1).PickUp()
        });
        possibleSupplies.Add(new()
        {
            name = "Dice",
            description = "Used to reroll drawn floorplans",
            cost = 5,
            amount = 5,
            OnBuy = () => new Dice(1).PickUp()
        });
        ColorKey colorKey = new();
        possibleSupplies.Add(new()
        {
            cost = 8,
            amount = 2,
            name = colorKey.Name,
            description = "Guarantee you draw rooms of the same category",
            OnBuy = () => new ColorKey(colorKey.floorCategory).PickUp()
        });
        possibleSupplies.Add(new()
        {
            name = "SledgeHammer",
            description = "Move towards an closed connection to open it.",
            cost = 10,
            amount = 1,
            OnBuy = () => new SledgeHammer().PickUp()
        });
        suppliesPack = possibleSupplies;
    }
}
