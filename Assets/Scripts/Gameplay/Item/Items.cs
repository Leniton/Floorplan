using System;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class Item
{
    public virtual string Name { get; set; }

    public virtual void Place(Room room) 
    {
        room.AddItem(this);
    }
    public virtual void PickUp() => GameEvent.onCollectItem?.Invoke(new(this));
    public abstract void Activate();
}

public abstract class StackableItem : Item
{
    public int amount = 1;
    public string itemName;
    
    public override string Name
    {
        get => $"{itemName}{(amount > 1 ? $"s ({amount})" : string.Empty)}";
        set => itemName = value;
    }
}

public class Food : StackableItem
{
    public override string Name => $"{itemName}{(amount > 1 ? $" ({amount})" : string.Empty)}";
    private static RarityPicker<int> foodRarity;
    public int stepsAmount;
    public int stepGain => stepsAmount * amount;
    public Food(int? amountSteps = null) 
    {
        if (foodRarity == null)
        {
            foodRarity = new(.2f, .4f, .3f, .1f);
            foodRarity.AddToPool(2, Rarity.Common);
            foodRarity.AddToPool(3, Rarity.Uncommon);
            foodRarity.AddToPool(4, Rarity.Rare);
            foodRarity.AddToPool(5, Rarity.Legend);
        }
        stepsAmount = amountSteps ?? foodRarity.PickRandom();
        Name = stepsAmount switch
        {
            2 => "Cherry",
            3 => "Apple",
            4 => "Banana",
            5 => "Orange",
            _ => $"+{stepGain} Food",
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
        Player.ChangeSteps(stepGain);
    }
}

public class Coin : StackableItem
{
    public int coinsAmount => amount;
    public Coin(int? _amount = null)
    {
        Name = "Coin";
        amount = _amount ?? Random.Range(1, 4);
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

public class Key : StackableItem
{
    public int keyAmount => amount;
    public Key(int? _amount = null)
    {
        Name = "Key";
        amount = _amount ?? Random.Range(1, 3);
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

public class Dice : StackableItem
{
    public int diceAmount => amount;
    public Dice(int? _amount = null)
    {
        Name = "Dice";
        amount = _amount ?? Random.Range(1, 3);
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
    public RoomCategory floorCategory { get; protected set; }

    public ColorKey(RoomCategory? category = null)
    {
        floorCategory = category ?? Helpers.RandomCategory();//will be 8 with addition of red rooms
        Name = $"{Helpers.CategoryName(floorCategory).Replace(" Room", string.Empty)} key";
    }

    public override void Activate()
    {
        if (active != Player.activeKey) return;
        active = !active;
        Player.currentKey = active ? this : null;
        if (active) GameEvent.onDrawRooms += GuaranteeCategory;
        else GameEvent.onDrawRooms -= GuaranteeCategory;
    }

    private void GuaranteeCategory(DrawRoomEvent evt)
    {
        evt.IncreaseChanceOfDrawing(CheckCategory, 1, Helpers.CategorySpareRoom(floorCategory));
        new Effect(null, 1).AnyRoomIsDrafted().Do(_ =>
        {
            GameEvent.onDrawRooms -= GuaranteeCategory;
            Player.currentKey = null;
            Player.items.Remove(this);
        });
    }

    private bool CheckCategory(Room floorplan) => floorplan.IsOfCategory(floorCategory);
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
    public bool placed => !ReferenceEquals(currentRoom, null);
    protected bool firstPlaced;
    protected Room currentRoom;

    public PlaceableItem(bool alreadyCanPlace = false) => firstPlaced = alreadyCanPlace;

    public override void Place(Room room)
    {
        if (firstPlaced) PlaceOnRoom(room);
        base.Place(room);
        firstPlaced = true;
    }

    protected virtual void PlaceOnRoom(Room room)
    {
        currentRoom = room;
        Player.items.Remove(this);
    }

    public override void PickUp()
    {
        bool wasPlaced = placed;
        currentRoom = null;
        Player.items.Add(this);
        if (wasPlaced) return;
        base.PickUp();
    }

    public override void Activate() => Place(Helpers.CurrentRoom());
}

public class CategoryWallpaper : Item
{
    public RoomCategory roomCategory { get; protected set; }

    public CategoryWallpaper(RoomCategory? category = null)
    {
        roomCategory = category ?? Helpers.RandomCategory();
        Name = $"{Helpers.CategoryName(roomCategory).Replace(" Room", string.Empty)} Decor";
    }

    public override void PickUp()
    {
        Player.items.Add(this);
        base.PickUp();
    }

    public override void Activate()
    {
        var room = Helpers.CurrentRoom();
        if (room.IsOfCategory(roomCategory))
        {
            MessageWindow.ShowMessage($"Floorplan is already a {Helpers.CategoryName(roomCategory)}!");
            return;
        }
        room.AddCategory(roomCategory);
        switch (roomCategory)
        {
            case RoomCategory.RestRoom:
                room.TheFirstTime().PlayerExitRoom().Do(_ => Player.ChangeSteps(room.CalculatePoints()));
                break;
            case RoomCategory.Hallway:
                for (int i = 0; i < room.connectedRooms.Count; i++)
                    room.connectedRooms[i].AddBonus(room.Alias, Bonus);
                room.EveryTime().RoomConnected().AddPointBonusToThatRoom(Bonus);
                int Bonus() => 1;
                break;
            case RoomCategory.StorageRoom:
                Helpers.AddRoomItems(room, true);
                break;
            case RoomCategory.Shop:
                int points = room.CalculatePoints();
                if (points > 0)
                    new Coin(points).AddItemToRoom(room);
                break;
            case RoomCategory.FancyRoom:
                room.basePoints += 3;
                room.OnChanged?.Invoke();
                break;
            case RoomCategory.MysteryRoom:
                room.AddMultiplier(Name, () => 2);
                break;
            case RoomCategory.CursedRoom:
                Player.ChangeSteps(-5);
                room.AddBonus(Name, () => 5);
                break;
        }
        Player.items.Remove(this);
    }
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
            2 => "Shiny Rock",
            3 => "Toy",
            4 => "Couch",
            5 => "TV",
            _ => $"Decoration (+{bonus})",
        };
    }

    protected override void PlaceOnRoom(Room room)
    {
        if (placed) PickUp();
        base.PlaceOnRoom(room);
        bonusKey = currentRoom.AddBonus(Name, PointBonus);
    }

    public override void PickUp()
    {
        currentRoom?.RemoveBonus(bonusKey);
        base.PickUp();
    }

    private int PointBonus() => bonus;
}

public class Battery : PlaceableItem
{
    private int mult;
    private string multKey;

    public Battery(int multiplier = 2, bool activate = false) : base(activate)
    {
        mult = multiplier;
        Name = $"V{mult} Battery";
    }

    protected override void PlaceOnRoom(Room floorplan)
    {
        if (placed) PickUp();
        base.PlaceOnRoom(floorplan);
        multKey = currentRoom.AddMultiplier(Name, Multiplier);
    }

    public override void PickUp()
    {
        currentRoom?.RemoveMultiplier(multKey);
        base.PickUp();
    }

    private int Multiplier() => mult;
}