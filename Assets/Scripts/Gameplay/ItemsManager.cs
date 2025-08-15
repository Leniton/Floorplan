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
        RarityPicker<Item> possibleItems = new(.25f, .1f, .05f, 0);
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
            default://blue rooms
                possibleItems.AddToPool(new Coin(), Rarity.Common);
                possibleItems.AddToPool(new Food(), Rarity.Common);
                possibleItems.AddToPool(new Key(), Rarity.Common);
                possibleItems.AddToPool(new Dice(), Rarity.Uncommon);
                possibleItems.AddToPool(new SledgeHammer(), Rarity.Rare);
                break;
        }

        return possibleItems;
    }

    public static void AddFloorplanItems(Floorplan floorplan, bool forceItem = false)
    {
        RarityPicker<Item> possibleItems = GetPossibleFloorplanItems(floorplan);
        //for items, legend means you get nothing
        possibleItems.allowEmptyResult = true;
        float nothingRate = possibleItems.commonRate + possibleItems.uncommonRate + possibleItems.rareRate;
        nothingRate = forceItem ? 0 : 1 - nothingRate;
        possibleItems.legendRate = nothingRate;
        
        //blue rooms are most likely to contain items
        if (floorplan.IsOfCategory(FloorCategory.BlueRoom))
        {
            float cutRate = possibleItems.legendRate / 2f;
            float distributeRate = cutRate / 2f;//to be 3 when rare items are introduced
            possibleItems.ChangeRarities(
                possibleItems.commonRate + distributeRate,
                possibleItems.uncommonRate + distributeRate,
                possibleItems.rareRate + distributeRate,//no rare items yet
                possibleItems.legendRate - cutRate);
        }

        possibleItems.PickRandom()?.Place(floorplan);
    }
}