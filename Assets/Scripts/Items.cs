using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item
{
    public bool placed;

    public virtual void Setup(Floorplan floorplan) { }
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
        GameEvent.OnCollectItem?.Invoke(new(this));
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
        GameEvent.OnCollectItem?.Invoke(new(this));
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
        GameEvent.OnCollectItem?.Invoke(new(this));
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
        GameEvent.OnCollectItem?.Invoke(new(this));
        UIManager.ShowMessage($"found dice!!\n+{amount} dices",
            () => Player.dices += amount);
    }
}

public class SledgeHammer : Item
{
    public override void PickUp()
    {
        throw new System.NotImplementedException();
    }
}