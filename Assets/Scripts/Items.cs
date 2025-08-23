using UnityEngine;

public abstract class Item
{
    public string Name;

    public virtual void Place(Floorplan floorplan) 
    {
        floorplan.AddItem(this);
    }
    public virtual void PickUp() => GameEvent.OnCollectItem?.Invoke(new(this));
    public abstract void Activate();
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
        Activate();
        base.PickUp();
    }

    public override void Activate()
    {
        //UIManager.ShowMessage($"found {Name}!!\n+{amount} steps"
        Player.ChangeSteps(stepsAmount);
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
        Activate();
        base.PickUp();
    }

    public override void Activate()
    {
        //Debug.Log($"found coins!!\n{Player.coins} + {amount}");
        Player.ChangeCoins(coinsAmount);
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
        Activate();
        base.PickUp();
    }

    public override void Activate()
    {
        //Debug.Log($"found keys!!\n{Player.keys} + {amount}");
        Player.ChangeKeys(keyAmount);
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
        Activate();
        base.PickUp();
    }

    public override void Activate()
    {
        //Debug.Log($"found dice!!\n{Player.dices} + {amount}");
        Player.dices += diceAmount;
    }
}

public abstract class ToggleItem : Item
{
    public bool active {  get; protected set; }

    public override void PickUp()
    {
        Player.items.Add(this);
        base.PickUp();
    }

    public virtual void Toggle() => active = !active;
}

public class ColorKey : ToggleItem
{
    private FloorCategory floorCategory;

    public ColorKey(FloorCategory? category = null)
    {
        floorCategory = category ?? (FloorCategory)Mathf.Pow(2, Random.Range(0, 7));//will be 8 with addition of red rooms
        Name = floorCategory switch
        {
            FloorCategory.RestRoom => "Rest Room Key",
            FloorCategory.Hallway => "Hallway Key",
            FloorCategory.StorageRoom => "Blue Room Key",
            FloorCategory.FancyRoom => "White Room Key",
            FloorCategory.Shop => "Shop Key",
            FloorCategory.MysteryRoom => "Black Room Key",
            _ => $"{floorCategory.ToString()} Key",
        };
    }

    public override void Activate()
    {
        if (active != Player.activeKey) return;
        Player.activeKey = active = !active;
        if (active) GameEvent.onDrawFloorplans += GuaranteeCategory;
        else GameEvent.onDrawFloorplans -= GuaranteeCategory;
    }

    private void GuaranteeCategory(DrawFloorplanEvent evt)
    {
        evt.IncreaseChanceOfDrawing(CheckCategory, 1);
        new Effect(null, 1).AnyFloorplanIsDrafted().Do(_ =>
        {
            GameEvent.onDrawFloorplans -= GuaranteeCategory;
            Player.activeKey = active = !active;
            Player.items.Remove(this);
        });
    }

    private bool CheckCategory(Floorplan floorplan) => floorplan.IsOfCategory(floorCategory);
}

public class SledgeHammer : ToggleItem
{
    public SledgeHammer() => Name = "Sledge Hammer";

    public override void Activate()
    {
        if (active != Player.activeSledgeHammer) return;
        active = !active;
        if (!active) return;
        Player.ActivateSledgeHammer(this);

    }
}

public abstract class PlaceableItem : Item
{
    public bool placed => !ReferenceEquals(currentFloorplan, null);
    protected bool firstPlaced;
    protected Floorplan currentFloorplan;

    public PlaceableItem(bool alreadyCanPlace = false) => firstPlaced = alreadyCanPlace;

    public override void Place(Floorplan floorplan)
    {
        if(firstPlaced) PlaceOnFloorplan(floorplan);
        base.Place(floorplan);
        firstPlaced = true;
    }

    protected virtual void PlaceOnFloorplan(Floorplan floorplan)
    {
        currentFloorplan = floorplan;
        Player.items.Remove(this);
    }

    public override void PickUp()
    {
        currentFloorplan = null;
        Player.items.Add(this);
        base.PickUp();
    }

    public override void Activate() => Place(Helpers.CurrentFloorplan());
}

public class Decoration : PlaceableItem
{
    public int bonus;
    private string bonusKey;

    public Decoration(int? pointBonus = null, bool activate = false) : base(activate)
    {
        bonus = pointBonus ?? Random.Range(2, 6);
        Name = bonus switch
        {
            2 => "Rock",
            3 => "Toy",
            4 => "Couch",
            5 => "TV",
            _ => $"Decoration (+{bonus})",
        };
    }

    protected override void PlaceOnFloorplan(Floorplan floorplan)
    {
        if (placed) PickUp();
        base.PlaceOnFloorplan(floorplan);
        bonusKey = currentFloorplan.AddBonus(Name, PointBonus);
    }

    public override void PickUp()
    {
        currentFloorplan?.RemoveBonus(bonusKey);
        base.PickUp();
    }

    private int PointBonus() => bonus;
}

public class Battery : PlaceableItem
{
    private string multKey;

    public Battery(bool activate = false) : base(activate) => Name = "Battery";

    protected override void PlaceOnFloorplan(Floorplan floorplan)
    {
        if (placed) PickUp();
        base.PlaceOnFloorplan(floorplan);
        multKey = currentFloorplan.AddMultiplier(Name, Multiplier);
    }

    public override void PickUp()
    {
        currentFloorplan?.RemoveMultiplier(multKey);
        base.PickUp();
    }

    private int Multiplier() => 2;
}