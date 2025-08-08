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

public abstract class Item
{
    public bool placed;
    public virtual void Setup(Floorplan floorplan){}
    public abstract void PickUp();
}

public class Food : Item
{
    public int stepsAmount; //null equals random
    public Food(int? amountSteps = null) => stepsAmount = amountSteps ?? Random.Range(2, 6);

    public override void PickUp()
    {
        int amount = stepsAmount;
        //Debug.Log($"found food!!\n{Player.steps} + {amount}");
        GameEvent.OnCollectItem?.Invoke(new (this));
        UIManager.ShowMessage($"found food!!\n+{amount} steps",
            () => Player.ChangeSteps(amount));
    }
}

public class Coin : Item
{
    public int? coinsAmount; //null equals random
    public Coin(int? amountCoin = null) => coinsAmount = amountCoin;

    public override void PickUp()
    {
        int amount = coinsAmount ?? Random.Range(1, 4);
        //Debug.Log($"found coins!!\n{Player.coins} + {amount}");
        GameEvent.OnCollectItem?.Invoke(new (this));
        UIManager.ShowMessage($"found coins!!\n+{amount} coins",
            () => Player.ChangeCoins(amount));
    }
}

public class Key : Item
{
    public int? keyAmount; //null equals random
    public Key(int? amountKey = null) => keyAmount = amountKey;

    public override void PickUp()
    {
        int amount = keyAmount ?? Random.Range(1, 3);
        //Debug.Log($"found keys!!\n{Player.keys} + {amount}");
        GameEvent.OnCollectItem?.Invoke(new (this));
        UIManager.ShowMessage($"found keys!!\n+{amount} keys",
            () => Player.ChangeKeys(amount));
    }
}

public class Dice : Item
{
    public int? diceAmount; //null equals random
    public Dice(int? amountDice = null) => diceAmount = amountDice;

    public override void PickUp()
    {
        int amount = diceAmount ?? Random.Range(1, 3);
        //Debug.Log($"found dice!!\n{Player.dices} + {amount}");
        GameEvent.OnCollectItem?.Invoke(new (this));
        UIManager.ShowMessage($"found dice!!\n+{amount} dices",
            () => Player.dices += amount);
    }
}