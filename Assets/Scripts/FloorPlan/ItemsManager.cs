using Lenix.NumberUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    private void Awake()
    {
        GameEvent.onDraftedFloorplan += OnFloorplanDrafted;
    }

    private void OnFloorplanDrafted(FloorplanEvent evt)
    {
        if (evt.Floorplan.Name == "Entrance Hall") return;
        AddFloorplanItems(evt.Floorplan);
    }

    public static RarityPicker<Item> GetPossibleFloorplanItems(Floorplan floorplan)
    {
        RarityPicker<Item> possibleItems = new(.3f, .1f, 0, 0);
        switch (floorplan.Category)
        {
            case FloorCategory.Shop:
                possibleItems.AddToPool(new Coin(), Rarity.Common);
                break;
            case FloorCategory.Hallway:
                possibleItems.AddToPool(new Coin(), Rarity.Common);
                possibleItems.AddToPool(new Coin(5), Rarity.Uncommon);
                break;
            case FloorCategory.RestRoom:
                possibleItems.AddToPool(new Food(), Rarity.Common);
                possibleItems.AddToPool(new Key(), Rarity.Common);
                possibleItems.AddToPool(new Dice(), Rarity.Uncommon);
                break;
            case FloorCategory.BlackRooms:
                possibleItems.AddToPool(new Key(), Rarity.Common);
                possibleItems.AddToPool(new Dice(), Rarity.Uncommon);
                possibleItems.AddToPool(new SledgeHammer(), Rarity.Uncommon);
                break;
            case FloorCategory.WhiteRoom:
                possibleItems.AddToPool(new Food(), Rarity.Common);
                possibleItems.AddToPool(new Key(), Rarity.Common);
                possibleItems.AddToPool(new Dice(), Rarity.Uncommon);
                break;
            default:
                possibleItems.AddToPool(new Coin(), Rarity.Common);
                possibleItems.AddToPool(new Food(), Rarity.Common);
                possibleItems.AddToPool(new Key(), Rarity.Common);
                possibleItems.AddToPool(new Dice(), Rarity.Uncommon);
                possibleItems.AddToPool(new SledgeHammer(), Rarity.Uncommon);
                break;
        }

        return possibleItems;
    }

    public static void AddFloorplanItems(Floorplan floorplan)
    {
        RarityPicker<Item> possibleItems = GetPossibleFloorplanItems(floorplan);
        //for items, legend means you get nothing
        possibleItems.allowEmptyResult = true;
        float nothingRate = possibleItems.commonRate + possibleItems.uncommonRate + possibleItems.rareRate;
        nothingRate = 1 - nothingRate;
        possibleItems.legendRate = nothingRate;
        
        //blue rooms are most likely to contain items
        if (NumberUtil.ContainsBytes((int)floorplan.Category, (int)FloorCategory.BlueRoom))
        {
            float cutRate = possibleItems.legendRate / 2f;
            float distributeRate = cutRate / 2f;//to be 3 when rare items are introduced
            possibleItems.ChangeRarities(
                possibleItems.commonRate + distributeRate,
                possibleItems.uncommonRate + distributeRate,
                0,//no rare items yet
                possibleItems.legendRate - cutRate);
        }

        Item item = possibleItems.PickRandom();
        if(item == null) return;
        floorplan.AddItem(item);
    }
}