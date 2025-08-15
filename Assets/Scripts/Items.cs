using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item
{
    public string Name;

    public virtual void Place(Floorplan floorplan) 
    {
        floorplan.AddItem(this);
    }
    public abstract void PickUp();
}

public class Food : Item
{
    public int stepsAmount; //null equals random
    public Food(int? amountSteps = null) 
    { 
        stepsAmount = amountSteps ?? Random.Range(2, 6);
        Name = stepsAmount switch
        {
            2 => "Cherry",
            3 => "Apple",
            4 => "Banana",
            5 => "Orange",
            _ => "Food",
        };
    }

    public override void PickUp()
    {
        int amount = stepsAmount;
        //Debug.Log($"found food!!\n{Player.steps} + {amount}");
        GameEvent.OnCollectItem?.Invoke(new(this));
        UIManager.ShowMessage($"found {Name}!!\n+{amount} steps",
            () => Player.ChangeSteps(amount));
    }
}

public class Coin : Item
{
    public int coinsAmount; //null equals random
    public Coin(int? amount = null)
    {
        coinsAmount = amount ?? Random.Range(1, 4);
        Name = coinsAmount > 1 ? $"Coins ({coinsAmount})" : "Coin";
    }

    public override void PickUp()
    {
        //Debug.Log($"found coins!!\n{Player.coins} + {amount}");
        GameEvent.OnCollectItem?.Invoke(new(this));
        UIManager.ShowMessage($"found {Name}",
            () => Player.ChangeCoins(coinsAmount));
    }
}

public class Key : Item
{
    public int keyAmount; //null equals random
    public Key(int? amount = null)
    {
        keyAmount = amount ?? Random.Range(1, 3);
        Name = keyAmount > 1 ? $"Keys ({keyAmount})" : "Key";
    }

    public override void PickUp()
    {
        //Debug.Log($"found keys!!\n{Player.keys} + {amount}");
        GameEvent.OnCollectItem?.Invoke(new(this));
        UIManager.ShowMessage($"found {Name}",
            () => Player.ChangeKeys(keyAmount));
    }
}

public class Dice : Item
{
    public int diceAmount; //null equals random
    public Dice(int? amount = null)
    {
        diceAmount = amount ?? Random.Range(1, 3);
        Name = diceAmount > 1 ? $"Dices ({diceAmount})" : "Dice";
    }

    public override void PickUp()
    {
        //Debug.Log($"found dice!!\n{Player.dices} + {amount}");
        GameEvent.OnCollectItem?.Invoke(new(this));
        UIManager.ShowMessage($"found {Name}",
            () => Player.dices += diceAmount);
    }
}

public abstract class ToggleItem : Item
{
    public bool active {  get; protected set; }
    public virtual void Toggle() => active = !active;
}

public class SledgeHammer : ToggleItem
{
    public SledgeHammer() => Name = "Sledge Hammer";

    public override void PickUp()
    {
        //if (!Player.activeSledgeHammer)
        //{
        //    UIManager.ShowMessage($"Found a {Name}!!",
        //        () => Player.activeSledgeHammer = true);
        //    return;
        //}

        //Player.items.Add(this);
        UIManager.ShowMessage($"Found a {Name}!!", () => Player.items.Add(this));
    }
}

public abstract class PlaceableItem : Item
{
    public bool placed;
    protected bool firstPlaced;

    public PlaceableItem(bool alreadyCanPlace = false) => firstPlaced = alreadyCanPlace;

    public override void Place(Floorplan floorplan)
    {
        base.Place(floorplan);
        if(!firstPlaced) PlaceOnFloorplan(floorplan);
        firstPlaced = true;
    }

    protected virtual void PlaceOnFloorplan(Floorplan floorplan)
    {
        placed = true;
    }

    public override void PickUp()
    {
        placed = false;
    }
}