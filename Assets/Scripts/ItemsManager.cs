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

    private void OnFloorplanDrafted(Vector2Int coordinate, Floorplan floorplan)
    {
        if (floorplan.Name == "Entrance Hall") return;
        AddFloorplanItems(floorplan);
    }

    public static void AddFloorplanItems(Floorplan floorplan)
    {
        //for items, common means you get nothing
        RarityPicker<Item> possibleItems = new(.6f, .3f, .1f, 0);
        possibleItems.allowEmptyResult = true;
        //blue rooms are most likely to contain items
        if (NumberUtil.ContainsBytes((int)floorplan.Category, (int)FloorCategory.BlueRoom))
        {
            float cutRate = possibleItems.commonRate / 2f;
            float distributeRate = cutRate / 3f;
            possibleItems.ChangeRarities(
                possibleItems.commonRate - cutRate,
                possibleItems.uncommonRate + distributeRate,
                possibleItems.rareRate + distributeRate,
                0);
        }

        switch (floorplan.Category)
        {
            default:
                possibleItems.AddToPool(new Food(), Rarity.Uncommon);
                possibleItems.AddToPool(new Coin(), Rarity.Uncommon);
                possibleItems.AddToPool(new Key(), Rarity.Uncommon);
                possibleItems.AddToPool(new Dice(), Rarity.Rare);
                break;
        }

        floorplan.AddItemToFloorplan(possibleItems.PickRandom());
    }
}

public abstract class Item
{
    public abstract void Initialize();
}

public class Food : Item
{
    public int? stepsAmount; //null equals random
    public Food(int? amountSteps = null) => stepsAmount = amountSteps;

    public override void Initialize()
    {
        int amount = stepsAmount ?? Random.Range(2, 7);
        //Debug.Log($"found food!!\n{Player.steps} + {amount}");
        UIManager.ShowMessage($"found food!!\n+{amount} steps",
            () => Player.ChangeSteps(amount));
    }
}

public class Coin : Item
{
    public int? coinsAmount; //null equals random
    public Coin(int? amountCoin = null) => coinsAmount = amountCoin;

    public override void Initialize()
    {
        int amount = coinsAmount ?? Random.Range(1, 4);
        //Debug.Log($"found coins!!\n{Player.coins} + {amount}");
        UIManager.ShowMessage($"found coins!!\n+{amount} coins",
            () => Player.ChangeCoins(amount));
    }
}

public class Key : Item
{
    public int? keyAmount; //null equals random
    public Key(int? amountKey = null) => keyAmount = amountKey;

    public override void Initialize()
    {
        int amount = keyAmount ?? Random.Range(1, 3);
        //Debug.Log($"found keys!!\n{Player.keys} + {amount}");
        UIManager.ShowMessage($"found keys!!\n+{amount} keys",
            () => Player.ChangeKeys(amount));
    }
}

public class Dice : Item
{
    public int? diceAmount; //null equals random
    public Dice(int? amountDice = null) => diceAmount = amountDice;

    public override void Initialize()
    {
        int amount = diceAmount ?? Random.Range(1, 3);
        //Debug.Log($"found dice!!\n{Player.dices} + {amount}");
        UIManager.ShowMessage($"found dice!!\n+{amount} dices",
            () => Player.dices += amount);
    }
}