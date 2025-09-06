using Lenix.NumberUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class EffectsManager
{
    public static void AddFloorplanEffect(Floorplan floorplan)
    {
        if (floorplan.Name == "Entrance Hall") return;
        Floorplan draftedFloorplan = floorplan.DraftedFrom();
        switch (floorplan.Name)
        {
            case "Attic":
                int atticItemCount = 6;
                for (int i = 0; i < atticItemCount; i++)
                    Helpers.AddFloorplanItems(floorplan, true);
                return;
            case "Bathroom":
                floorplan.TheFirstTime().PlayerEnterFloorplan().Do(_ =>
                {
                    int currentSteps = Player.steps;
                    int stepChange = 10 - (currentSteps % 10);
                    floorplan.AddMultiplier(floorplan.Alias, () => stepChange);
                    Player.ChangeSteps(stepChange);
                });
                break;
            case "Bedroom":
                floorplan.TheFirstTime().PlayerExitFloorplan()
                    .Do(_ => Player.ChangeSteps(floorplan.CalculatePoints()));
                break;
            case "Boiler Room":
                floorplan.EveryTime().FloorplanConnected().PowerThatFloorplan();
                break;
            case "Boudoir":
                floorplan.EveryTime().FloorplansAreDrawn().Where(DraftedFromHere).Do(evt =>
                    evt.IncreaseChanceOfDrawing(target => target.IsOfCategory(FloorCategory.RestRoom)));
                break;
            case "Bunk Room":
                //double draft
                floorplan.TheFirstTime().
                    FloorplanIsDrafted().Do(evt =>
                    {
                        GameManager.floorplanDict[floorplan.coordinate * -1] = floorplan;
                        floorplan.onDrafted?.Invoke(evt);
                        GameEvent.onDraftedFloorplan?.Invoke(new(evt.Coordinates,
                            GameManager.floorplanDict[evt.Coordinates]));
                    });
                //double connect
                bool retrigger = false;
                floorplan.EveryTime().FloorplanConnected().Where(_ => retrigger = !retrigger)
                    .Do(evt => Helpers.ConnectFloorplans(evt.baseFloorplan, evt.connectedFloorplan));
                break;
            case "Cassino":
                floorplan.TheFirstTime().PlayerEnterFloorplan().Do(evt =>
                {
                    int r = Random.Range(0, 100);
                    if (r < 70) // gotta lie to the player sometimes
                    {
                        Player.ChangeCoins(Player.coins);
                        UIManager.ShowMessage($"Luck is on your side, your coins doubled!!!");
                    }
                    else
                    {
                        Player.ChangeCoins(-(Player.coins / 2));
                        UIManager.ShowMessage($"That's too bad, you lost half your coins...");
                    }
                });
                break;
            case "Cloister":
                //contains a placed statue
                ItemUtilities.Statue(true).AddItemToFloorplan(floorplan);
                //bonus for each decoration
                floorplan.AddBonus(floorplan.Alias, DecorationBonus);
                int DecorationBonus()
                {
                    int bonus = 0;
                    for (int i = 0; i < floorplan.items.Count; i++)
                    {
                        Decoration decoration = floorplan.items[i] as Decoration;
                        if (decoration is null or { placed: false}) continue;
                        bonus += 2;
                    }
                    return bonus;
                }
                break;
            case "Commissary":
                PurchaseData bananaCommissary = new()
                {
                    cost = 3,
                    amount = 3,
                    name = "Banana",
                    description = "Gain +4 steps",
                    OnBuy = () => ItemUtilities.Banana.PickUp()
                };
                PurchaseData keyCommissary = new()
                {
                    cost = 5,
                    amount = 3,
                    name = "Key",
                    description = "Used to draft powerful floorplans",
                    OnBuy = () => new Key(1).PickUp()
                };
                PurchaseData rock = new()
                {
                    cost = 2,
                    amount = 2,
                    name = "Rock",
                    description = "Place on floorplans to add +2 points to it",
                    OnBuy = () => ItemUtilities.Rock().PickUp()
                };
                PurchaseData dice = new()
                {
                    cost = 8,
                    amount = 2,
                    name = "Dice",
                    description = "Used to reroll drawn floorplans",
                    OnBuy = () => new Dice(1).PickUp()
                };
                PurchaseData toy = new()
                {
                    cost = 6,
                    amount = 2,
                    name = "Toy",
                    description = "Place on floorplans to add +3 points to it",
                    OnBuy = () => ItemUtilities.Toy().PickUp()
                };
                PurchaseData battery = new()
                {
                    cost = 15,
                    amount = 1,
                    name = "Battery",
                    description = "Place on floorplans to <b>Power</b> them",
                    OnBuy = () => new Dice(1).PickUp()
                };
                PurchaseData couch = new()
                {
                    cost = 10,
                    amount = 1,
                    name = "Couch",
                    description = "Place on floorplans to add +4 points to it",
                    OnBuy = () =>ItemUtilities.Couch().PickUp()
                };

                RarityPicker<PurchaseData> picker = new();
                picker.AddToPool(bananaCommissary, Rarity.Common);
                picker.AddToPool(keyCommissary, Rarity.Common);
                picker.AddToPool(rock, Rarity.Common);
                picker.AddToPool(dice, Rarity.Uncommon);
                picker.AddToPool(toy, Rarity.Uncommon);
                picker.AddToPool(battery, Rarity.Rare);
                picker.AddToPool(couch, Rarity.Rare);

                List<PurchaseData> commissaryList = new(3);
                picker.ChangeRarities(1, 0, 0, 0);
                commissaryList.Add(picker.PickRandom());
                picker.ChangeRarities(0, 1, 0, 0);
                commissaryList.Add(picker.PickRandom());
                picker.ChangeRarities(0, 0, 1, 0);
                commissaryList.Add(picker.PickRandom());
                floorplan.TheFirstTime().FloorplanIsDrafted().SetupFloorplanShop(floorplan.Name, commissaryList);
                break;
            case "Conservatory":
                new ColorKey().AddItemToFloorplan(floorplan);
                break;
            case "Courtyard":
                new Key(3).AddItemToFloorplan(floorplan);
                break;
            case "Den":
                floorplan.TheFirstTime().FloorplanIsDrafted().AddItemToFloorplan(new Key(1));
                break;
            case "Dining Room":
                int stepsFromFood = 0;
                ItemUtilities.Meal().AddItemToFloorplan(floorplan);
                floorplan.AddBonus(floorplan.Alias, () => stepsFromFood);
                floorplan.EveryTime().ItemCollected().Where(evt => evt.item is Food).Do(evt => stepsFromFood += (evt.item as Food).stepsAmount);
                break;
            case "Dormitory":
                //gains extra points for each restRoom in the house
                int selfBonus = 0;
                floorplan.ForEveryFloorplan(IsOfCategory(FloorCategory.RestRoom), _ => selfBonus += 2);
                floorplan.AddBonus(floorplan.Alias, () => selfBonus);
                //when you connect a rest room, add a snack to this room
                floorplan.EveryTime().FloorplanConnected().
                    Where(IsOfCategory(FloorCategory.RestRoom)).
                    Do(evt => ItemUtilities.Snack().AddItemToFloorplan(evt.connectedFloorplan));
                break;
            case "Drawing Room":
                int startAmount = 0;
                floorplan.EveryTime().PlayerEnterFloorplan().Do(_ =>
                {
                    startAmount = Player.dices;
                    //Debug.Log($"entered room with {startAmount} dices");
                    Player.dices += 2;
                });
                floorplan.EveryTime().ItemCollected().
                    Where(_ => ReferenceEquals(floorplan, Helpers.CurrentFloorplan()), evt => evt.item is Dice).
                    Do(evt =>
                {
                    //Debug.Log($"Gained dice while on drawing room");
                    Dice dice = evt.item as Dice;
                    startAmount += dice.diceAmount;
                });
                floorplan.EveryTime().PlayerExitFloorplan().Do(_ => Player.dices = Mathf.Min(startAmount, Player.dices));
                break;
            case "Gallery":
                int visits = 0;
                floorplan.AddBonus(floorplan.Alias, () => visits);
                floorplan.EveryTime().PlayerEnterFloorplan().Where(_ => Player.coins > 0).Do(_ =>
                {
                    Player.ChangeCoins(-1);
                    visits++;
                });
                break;
            case "Gift Shop":
                int bonusPoints = 0;
                PurchaseData one = new()
                {
                    cost = 2,
                    amount = 9999,
                    name = "1 point",
                    description = "Adds 1 point to the Gift Shop",
                    OnBuy = () => AddPoints(1)
                };
                PurchaseData three = new()
                {
                    cost = 5,
                    amount = 9999,
                    name = "3 point bundle",
                    description = "Adds 3 points to the Gift Shop",
                    OnBuy = () => AddPoints(3)
                };
                PurchaseData five = new()
                {
                    cost = 8,
                    amount = 9999,
                    name = "5 point bundle",
                    description = "Adds 5 points to the Gift Shop",
                    OnBuy = () => AddPoints(5)
                };
                PurchaseData ten = new()
                {
                    cost = 14,
                    amount = 9999,
                    name = "10 point bundle",
                    description = "Adds 10 points to the Gift Shop",
                    OnBuy = () => AddPoints(10)
                };

                void AddPoints(int amount) => bonusPoints += amount;
                floorplan.AddBonus(floorplan.Alias, () => bonusPoints);
                List<PurchaseData> giftList = new () { one, three, five, ten };
                floorplan.TheFirstTime().FloorplanIsDrafted().SetupFloorplanShop(floorplan.Name, giftList);
                break;
            case "Great Hall":
                //extra points for each different type of room connected
                FloorCategory connectedCategories = 0;
                floorplan.AddBonus(floorplan.Alias, () => NumberUtil.SeparateBits((int)connectedCategories).Length * 2);
                floorplan.EveryTime().FloorplanConnected()
                    .Do(evt => connectedCategories |= evt.connectedFloorplan.Category);
                break;
            case "Guest Bedroom":
                //essentialy free to move in
                floorplan.EveryTime().PlayerEnterFloorplan().ChangePlayerSteps(2);

                //extra points for each connected rest room
                floorplan.EveryTime().FloorplanConnected().
                    Where(IsOfCategory(FloorCategory.RestRoom)).
                    AddPointsToFloorplan(5);
                break;
            case "Hallway Closet":
                int hallwayClosetItemCount = 2;
                if (!ReferenceEquals(draftedFloorplan, null) &&
                    draftedFloorplan.IsOfCategory(FloorCategory.Hallway))
                    hallwayClosetItemCount += 1;

                for (int i = 0; i < hallwayClosetItemCount; i++)
                    Helpers.AddFloorplanItems(floorplan, true);
                return;
            case "Hovel":
                //buff rest rooms
                floorplan.ForEveryFloorplan(IsOfCategory(FloorCategory.RestRoom), evt => evt.Floorplan.AddBonus(floorplan.Alias, () => 1));
                break;
            case "Kitchen":
                PurchaseData cherry = new()
                {
                    cost = 1,
                    amount = 6,
                    name = "Cherry",
                    description = "Gain +2 steps",
                    OnBuy = () => ItemUtilities.Cherry.PickUp()
                };
                PurchaseData apple = new()
                {
                    cost = 2,
                    amount = 5,
                    name = "Apple",
                    description = "Gain +3 steps",
                    OnBuy = () => ItemUtilities.Apple.PickUp()
                };
                PurchaseData banana = new()
                {
                    cost = 3,
                    amount = 4,
                    name = "Banana",
                    description = "Gain +4 steps",
                    OnBuy = () => ItemUtilities.Banana.PickUp()
                };
                PurchaseData orange = new()
                {
                    cost = 4,
                    amount = 3,
                    name = "Orange",
                    description = "Gain +5 steps",
                    OnBuy = () => ItemUtilities.Orange.PickUp()
                };
                PurchaseData soda = new()
                {
                    cost = 6,
                    amount = 3,
                    name = "Soda",
                    description = "Gain +7 steps",
                    OnBuy = () => ItemUtilities.Soda.PickUp()
                };
                PurchaseData snack = new()
                {
                    cost = 10,
                    amount = 1,
                    name = "Snack",
                    description = "Gain +1 step for each floorplan you drafted",
                    OnBuy = () => ItemUtilities.Snack().PickUp()
                };
                PurchaseData energyBar = new()
                {
                    cost = 10,
                    amount = 1,
                    name = "Energy Bar",
                    description = "Gain +2 steps for each <b>Dead end</b> you drafted",
                    OnBuy = () => ItemUtilities.EnergyBar().PickUp()
                };

                RarityPicker<PurchaseData> kitchenPicker = new();
                kitchenPicker.AddToPool(cherry, Rarity.Common);
                kitchenPicker.AddToPool(apple, Rarity.Common);
                kitchenPicker.AddToPool(banana, Rarity.Common);
                kitchenPicker.AddToPool(orange, Rarity.Uncommon);
                kitchenPicker.AddToPool(soda, Rarity.Uncommon);
                kitchenPicker.AddToPool(snack, Rarity.Rare);
                kitchenPicker.AddToPool(energyBar, Rarity.Rare);

                List<PurchaseData> kitchenList = new (3);
                kitchenPicker.ChangeRarities(1, 0, 0, 0);
                kitchenList.Add(kitchenPicker.PickRandom());
                kitchenPicker.ChangeRarities(0, 1, 0, 0);
                kitchenList.Add(kitchenPicker.PickRandom());
                kitchenPicker.ChangeRarities(0, 0, 1, 0);
                kitchenList.Add(kitchenPicker.PickRandom());
                floorplan.TheFirstTime().FloorplanIsDrafted().SetupFloorplanShop(floorplan.Name, kitchenList);
                break;
            case "Library":
                floorplan.EveryTime().FloorplansAreDrawn().Where(DraftedFromHere).Do(evt =>
                {
                    for (int i = 0; i < evt.drawnFloorplans.Length; i++)
                    {
                        var costLessFloorplan = evt.drawnFloorplans[i].
                            CreateInstance(Floorplan.IDToDirection(evt.drawnFloorplans[i].entranceId));
                        costLessFloorplan.keyCost = 0;
                        evt.drawnFloorplans[i] = costLessFloorplan;
                    }
                });
                break;
            case "Locksmith":
                PurchaseData key = new()
                {
                    cost = 5,
                    amount = 5,
                    name = "Key",
                    description = "Used to draft powerful floorplans",
                    OnBuy = () => new Key(1).PickUp()
                };
                PurchaseData keyBundle = new()
                {
                    cost = 12,
                    amount = 3,
                    name = "Key bundle",
                    description = "A 3 key bundle",
                    OnBuy = () => new Key(3).PickUp()
                };
                ColorKey keyColor = new();
                PurchaseData colorKey = new()
                {
                    cost = 8,
                    amount = 1,
                    name = keyColor.Name,
                    description = "Guarantee you draw rooms of the same category",
                    OnBuy = () => new ColorKey(keyColor.floorCategory).PickUp()
                };
                PurchaseData sledgeHammer = new()
                {
                    cost = 10,
                    amount = 1,
                    name = "SledgeHammer",
                    description = "Move towards an closed connection to open it.",
                    OnBuy = () => new SledgeHammer().PickUp()
                };

                List<PurchaseData> locksmithList = new() { key, keyBundle, colorKey, sledgeHammer };
                floorplan.TheFirstTime().FloorplanIsDrafted().SetupFloorplanShop(floorplan.Name, locksmithList);
                break;
            case "Master Bedroom":
                //buffs all rest room for each rest room
                int otherBonus = 0;
                floorplan.ForEveryFloorplan(IsOfCategory(FloorCategory.RestRoom), evt =>
                {
                    otherBonus += 2;
                    evt.Floorplan.AddBonus(floorplan.Alias, () => otherBonus);
                });
                break;
            case "Mail Room":
                const int draftsNeeded = 3;
                int count = 0;
                floorplan.TheNext_Times(draftsNeeded).AnyFloorplanIsDrafted().Where(IsNot(floorplan)).Do(_ =>
                {
                    count++;
                    if (count < draftsNeeded) return;
                    UIManager.ShowMessage("Your package has been delivered!!");
                    RarityPicker<Item> picker = floorplan.ItemPool();
                    //add uncommon item
                    picker.ChangeRarities(0, 1, 0, 0);
                    picker.PickRandom().AddItemToFloorplan(floorplan);
                    //add rare item
                    picker.ChangeRarities(0, 0, 1, 0);
                    picker.PickRandom().AddItemToFloorplan(floorplan);
                });
                break;
            case "Pantry":
                new Coin().AddItemToFloorplan(floorplan);
                new Food().AddItemToFloorplan(floorplan);
                break;
            case "Pump Room":
                floorplan.EveryTime().FloorplanConnected().Do(evt =>
                {
                    evt.connectedFloorplan.AddBonus(floorplan.Alias, () =>
                    {
                        int bonus = 0;
                        for (int i = 0; i < floorplan.connectedFloorplans.Count; i++)
                        {
                            var current = floorplan.connectedFloorplans[i];
                            if(ReferenceEquals(current, evt.connectedFloorplan)) continue;
                            bonus += current.basePoints;
                        }
                        return bonus;
                    });
                });
                break;
            case "Secret Passage":
                //add connection to drafted floorplans
                floorplan.EveryTime().FloorplansAreDrawn().Where(DraftedFromHere).Do(evt =>
                {
                    for (int i = 0; i < evt.drawnFloorplans.Length; i++)
                    {
                        Floorplan drawnFloorplan = evt.drawnFloorplans[i];
                        int maxConnections = Mathf.Abs((int)evt.possibleFloorTypes[^1]);
                        int openConnections = 0;
                        int closedConnectionID = -1;
                        for (int c = 0; c < maxConnections; c++)
                        {
                            if (drawnFloorplan.connections[c]) openConnections++;
                            else if (closedConnectionID < 0)
                            {
                                //Debug.Log($"{drawnFloorplan.Name} closed at {Floorplan.IDToDirection(c)}({drawnFloorplan.connections[c]})");
                                closedConnectionID = c;
                            }
                        }

                        //if there's no closed connections, add item to floorplan
                        if (openConnections >= maxConnections)
                        {
                            //Debug.Log($"{drawnFloorplan.Name} already has 4 connections, adding item");
                            Helpers.AddFloorplanItems(drawnFloorplan, true);
                            continue;
                        }
                        //otherwise open a connection on floorplan
                        Floorplan newFloorplan = drawnFloorplan.CreateInstance(Floorplan.IDToDirection(drawnFloorplan.entranceId));
                        newFloorplan.OpenConnection(closedConnectionID);
                        evt.drawnFloorplans[i] = newFloorplan;
                        //Debug.Log($"open connection on {newFloorplan} => {Floorplan.IDToDirection(closedConnectionID)}({newFloorplan.connections[closedConnectionID]})");
                    }
                });

                //if there's already a floorplan on a openConnection, connect to it
                floorplan.TheFirstTime().FloorplanIsDrafted().Do(evt =>
                {
                    for (int i = 0; i < floorplan.connections.Length; i++)
                    {
                        if (i == floorplan.entranceId || !floorplan.connections[i]) continue;
                        Vector2Int connection = Floorplan.IDToDirection(i);
                        if (!GameManager.floorplanDict.TryGetValue(floorplan.coordinate + connection, out var otherFloorplan)) continue;
                        int connectionID = Floorplan.DirectionToID(-connection);
                        if (otherFloorplan.connections[connectionID]) continue;
                        //Debug.Log($"{otherFloorplan.Name} may connect with {floorplan.Name}");
                        otherFloorplan.OpenConnection(connectionID);
                    }
                });
                break;
            case "Terrace":
                RarityPicker<Item> terraceItems = floorplan.ItemPool();
                terraceItems.ChangeRarities(0,1,0,0);
                terraceItems.PickRandom().AddItemToFloorplan(floorplan);
                break;
            case "Tunnel":
                //Aways draw a tunnel when drafting from tunnel
                floorplan.EveryTime().FloorplansAreDrawn().Where(DraftedFromHere).Do(evt =>
                {
                    Floorplan tunnel = floorplan.original.CreateInstance(Floorplan.IDToDirection(floorplan.entranceId));
                    int id = Random.Range(0, 2);
                    evt.drawnFloorplans[id] = tunnel;
                });
                //surprise if reach the edge
                int exitId = (floorplan.entranceId + 2) % 4;
                floorplan.TheFirstTime().FloorplanIsDrafted().
                    Where(_ => !GridManager.instance.ValidCoordinate(floorplan.coordinate + Floorplan.IDToDirection(exitId))).
                    Do(_ =>
                {
                    floorplan.connections[exitId] = false;
                    new Key(5).AddItemToFloorplan(floorplan);
                    floorplan.OnChanged?.Invoke();
                });
                break;
            case "Utility Closet":
                //power all rooms of the same category
                floorplan.ForEveryFloorplan(MatchCategoryWith(floorplan), evt => evt.Floorplan.AddMultiplier(floorplan.Alias, () => 2));
                break;
            case "Vault":
                int lastRoomCount = 0;
                floorplan.EveryTime().PlayerEnterFloorplan().Do(_ =>
                {
                    int coinAmount = GameManager.floorplanDict.Count - lastRoomCount;
                    if (coinAmount <= 0) return;
                    new Coin(coinAmount).PickUp();
                    lastRoomCount = GameManager.floorplanDict.Count;
                });
                break;
            case "Vestibule":
                floorplan.EveryTime().FloorplanConnected().
                    Do(evt =>
                    {
                        for (int i = 0; i < floorplan.connectedFloorplans.Count; i++)
                        {
                            Floorplan currentFloorplan = floorplan.connectedFloorplans[i];
                            if (currentFloorplan == evt.connectedFloorplan) continue;
                            if (currentFloorplan.connectedFloorplans.Contains(evt.connectedFloorplan)) continue;
                            Helpers.ConnectFloorplans(currentFloorplan, evt.connectedFloorplan);
                            Debug.Log($"{floorplan.Name} connected {currentFloorplan.Name} to {evt.connectedFloorplan.Name}");
                        }
                    });
                break;
            case "Walk-In Closet":
                int walkinClosetItemCount = 4;
                if (!ReferenceEquals(draftedFloorplan, null) && draftedFloorplan.IsOfCategory(FloorCategory.RestRoom))
                    walkinClosetItemCount += 2;

                for (int i = 0; i < walkinClosetItemCount; i++)
                    Helpers.AddFloorplanItems(floorplan, true);
                return;
            case "":
                break;
        }
        Helpers.AddFloorplanItems(floorplan);
        bool DraftedFromHere<T>(T evt) where T : Event => Helpers.CurrentFloorplan() == floorplan;
    }

    #region EffectCreation
    public static Effect TheFirstTime(this Floorplan floorplan) => new (floorplan, 1);
    public static Effect EveryTime(this Floorplan floorplan) => new (floorplan);
    public static Effect TheNext_Times(this Floorplan floorplan, uint times) => new(floorplan, (int)times);
    public static void ForEveryFloorplan(this Floorplan floorplan, Func<FloorplanEvent, bool> condition,
        Action<FloorplanEvent> action)
    {
        foreach (var room in GameManager.floorplanDict)
        {
            if (!CheckCondition(room)) continue;
            action?.Invoke(new(room.Key, room.Value));
        }
        floorplan.EveryTime().AnyFloorplanIsDrafted().Where(condition).Do(action);

        bool CheckCondition(KeyValuePair<Vector2Int, Floorplan> data) => condition?.Invoke(new(data.Key, data.Value)) ?? true;
    }
    #endregion

    #region EventListeners
    //Floorplan
    public static EventListener<Action<CoordinateEvent>, CoordinateEvent>
        FloorplanIsDrafted(this Effect effect) => new(effect,
        (a) => effect.floorplan.onDrafted += a,
        (a) => effect.floorplan.onDrafted -= a);
    
    public static EventListener<Action<FloorplanConnectedEvent>, FloorplanConnectedEvent>
        FloorplanConnected(this Effect effect) => new(effect,
        (a) => effect.floorplan.onConnectToFloorplan += a,
        (a) => effect.floorplan.onConnectToFloorplan -= a);
    public static EventListener<Action<Event>, Event> PlayerEnterFloorplan(this Effect effect) => new(effect,
        (a) => effect.floorplan.onEnter += a,
        (a) => effect.floorplan.onEnter -= a);
    public static EventListener<Action<Event>, Event> PlayerExitFloorplan(this Effect effect) => new(effect,
        (a) => effect.floorplan.onExit += a,
        (a) => effect.floorplan.onExit -= a);
    
    //GameEvent
    public static EventListener<Action<FloorplanEvent>, FloorplanEvent>
        AnyFloorplanIsDrafted(this Effect effect) => new(effect,
        (a) => GameEvent.onDraftedFloorplan += a,
        (a) => GameEvent.onDraftedFloorplan -= a);
    
    public static EventListener<Action<FloorplanConnectedEvent>, FloorplanConnectedEvent>
        AnyFloorplanConnected(this Effect effect) => new(effect,
        (a) => GameEvent.onConnectFloorplans += a,
        (a) => GameEvent.onConnectFloorplans -= a);
    
    public static EventListener<Action<FloorplanEvent>, FloorplanEvent>
        PlayerEnterAnyFloorplan(this Effect effect) => new(effect,
        (a) => GameEvent.OnEnterFloorplan += a,
        (a) => GameEvent.OnEnterFloorplan -= a);

    public static EventListener<Action<FloorplanEvent>, FloorplanEvent>
        PlayerExitAnyFloorplan(this Effect effect) => new(effect,
        (a) => GameEvent.OnExitFloorplan += a,
        (a) => GameEvent.OnExitFloorplan -= a);

    public static EventListener<Action<DrawFloorplanEvent>, DrawFloorplanEvent>
        FloorplansAreDrawn(this Effect effect) => new(effect,
        (a) => GameEvent.onDrawFloorplans += a,
        (a) => GameEvent.onDrawFloorplans -= a);

    public static EventListener<Action<ItemEvent>, ItemEvent>
        ItemCollected(this Effect effect) => new(effect,
        (a) => GameEvent.OnCollectItem += a,
        (a) => GameEvent.OnCollectItem -= a);

    #endregion

    #region Effects
    public static EventListener<Action<T>, T> Do<T>(this EventListener<Action<T>, T> listener, Action<T> action) where T : Event
    {
        listener.AddAction(DoAction);
        return listener;
        void DoAction(T evt)
        {
            bool fulfilConditions = true;
            for (int i = 0; i < listener.conditions.Count; i++)
                fulfilConditions &= listener.conditions[i]?.Invoke(evt) ?? true;
            if (!fulfilConditions) return;

            bool hasMoreUses = listener.effect.TryUse(out var canUse);
            if (!canUse) return;
            action?.Invoke(evt);
            if (hasMoreUses) return;
            listener.RemoveAction(DoAction);
        }
    }

    //Player changes
    public static EventListener<Action<T>, T> ChangePlayerSteps<T>(this EventListener<Action<T>,T> listener, int amount) where T : Event =>
        listener.Do(_ => Player.ChangeSteps(amount));
    public static EventListener<Action<T>,T> ChangePlayerCoins<T>(this EventListener<Action<T>,T> listener, int amount) where T : Event =>
        listener.Do(_ => Player.ChangeCoins(amount));
    public static EventListener<Action<T>,T> ChangePlayerKeys<T>(this EventListener<Action<T>,T> listener, int amount) where T : Event =>
        listener.Do(_ => Player.ChangeKeys(amount));

    //Floorplan changes
    public static EventListener<Action<T>,T> AddItemToFloorplan<T>(this EventListener<Action<T>,T> listener, Item item) where T : Event =>
        listener.Do(_ => item.AddItemToFloorplan(listener.effect.floorplan));
    public static EventListener<Action<T>, T> AddItemToThatFloorplan<T>(this EventListener<Action<T>, T> listener, Item item) where T : CoordinateEvent =>
        listener.Do(evt => item.AddItemToFloorplan(GameManager.floorplanDict[evt.Coordinates]));
    public static EventListener<Action<T>,T> AddPointsToFloorplan<T>(this EventListener<Action<T>,T> listener, int amount) where T : Event =>
        listener.Do(_ => listener.effect.floorplan.AddBonus(listener.effect.floorplan.Alias, () => amount));
    public static EventListener<Action<T>,T> AddPointBonusToFloorplan<T>(this EventListener<Action<T>,T> listener, Func<int> bonus) where T : Event =>
        listener.Do(_ => listener.effect.floorplan.AddBonus(listener.effect.floorplan.Alias, bonus));
    public static EventListener<Action<T>,T> PowerFloorplan<T>(this EventListener<Action<T>,T> listener) where T : Event =>
        listener.Do(_ => listener.effect.floorplan.AddMultiplier(listener.effect.floorplan.Alias, () => 2));
    public static EventListener<Action<T>,T> AddPointsToThatFloorplan<T>(this EventListener<Action<T>,T> listener, int amount) where T : CoordinateEvent =>
        listener.Do(evt => GameManager.floorplanDict[evt.Coordinates].AddBonus(listener.effect.floorplan.Alias, () => amount));
    public static EventListener<Action<T>,T> AddPointBonusToThatFloorplan<T>(this EventListener<Action<T>,T> listener, Func<int> amount) where T : CoordinateEvent =>
        listener.Do(evt => GameManager.floorplanDict[evt.Coordinates].AddBonus(listener.effect.floorplan.Alias, amount));
    public static EventListener<Action<T>,T> PowerThatFloorplan<T>(this EventListener<Action<T>, T> listener) where T : CoordinateEvent =>
        listener.Do(evt => GameManager.floorplanDict[evt.Coordinates].AddMultiplier(listener.effect.floorplan.Alias, () => 2));
    public static EventListener<Action<T>,T> SetupFloorplanShop<T>(this EventListener<Action<T>, T> listener, string title, List<PurchaseData> shopList)
        where T : CoordinateEvent
    {
        listener.Do(CreateShop);
        return listener;
        void CreateShop(CoordinateEvent evt)
        {
            if (!GameManager.floorplanDict.TryGetValue(evt.Coordinates, out var floorplan)) return;
            bool firstEntered = false;
            floorplan.onEnter += SetupShop;
            floorplan.onExit += CloseShop;
            listener.RemoveAction(CreateShop);
            return;

            void SetupShop(Event subEvt)
            {
                if (firstEntered)
                    ShopWindow.SetupShop(title, shopList);
                else
                    ShopWindow.OpenShop(title, shopList);
                firstEntered = true;
            }
            void CloseShop(Event subEvt) => ShopWindow.CloseShop();
        }
    }

    #endregion

    #region Conditions
    public static EventListener<Action<T>, T> Where<T>(this EventListener<Action<T>, T> listener,
        params Func<T, bool>[] check) where T : Event
    {
        for (int i = 0; i < check.Length; i++)
            listener.conditions.Add(check[i]);
        return listener;
    }
    public static Func<FloorplanEvent, bool> IsOfCategory(FloorCategory type) =>
        evt => NumberUtil.ContainsBytes((int)evt.Floorplan.Category, (int)type);
    public static Func<FloorplanEvent, bool> MatchCategoryWith(Floorplan floorplan) =>
        evt => evt.Floorplan.IsOfCategory(floorplan.Category);
    public static Func<FloorplanEvent, bool> IsNot(Floorplan floorplan) =>
        evt => evt.Floorplan != floorplan;
    #endregion
}