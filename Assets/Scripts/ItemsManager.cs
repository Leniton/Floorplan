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
        //for items, common means you get nothing
        RarityPicker<Item> possibleItems = new(.6f, .3f, .1f, 0);
        possibleItems.allowEmptyResult = true;
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

        GameEvent.OnEnterFloorplan += OnEnterFloorplan;
        void OnEnterFloorplan(Vector2Int newCoordinate, Floorplan targetFloorplan)
        {
            if (targetFloorplan != floorplan) return;
            Item item = possibleItems.PickRandom();
            item?.Initialize();
            GameEvent.OnEnterFloorplan -= OnEnterFloorplan;
        }
    }
}

public abstract class Item
{
    public abstract void Initialize();
}

public class Food : Item
{
    public override void Initialize()
    {
        int amount = Random.Range(2, 7);
        Debug.Log($"found food!!\n{Player.steps} + {amount}");
        Player.ChangeSteps(amount);
    }
}

public class Coin : Item
{
    public override void Initialize()
    {
        int amount = Random.Range(1, 4);
        Debug.Log($"found coins!!\n{Player.coins} + {amount}");
        Player.ChangeCoins(amount);
    }
}

public class Key : Item
{
    public override void Initialize()
    {
        int amount = Random.Range(1, 3);
        Debug.Log($"found keys!!\n{Player.keys} + {amount}");
        Player.ChangeCrayons(amount);
    }
}

public class Dice : Item
{
    public override void Initialize()
    {
        int amount = Random.Range(1, 3);
        Debug.Log($"found dice!!\n{Player.dices} + {amount}");
        Player.dices += amount;
    }
}