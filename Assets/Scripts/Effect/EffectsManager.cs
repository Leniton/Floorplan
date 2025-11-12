using Lenix.NumberUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class EffectsManager
{
    public static void AddRoomEffect(Room room)
    {
        if (room.Name == "Entrance Hall") return;
        Room draftedRoom = Helpers.CurrentRoom();
        switch (room.Name)
        {
            case "Attic":
                int atticItemCount = 6;
                for (int i = 0; i < atticItemCount; i++)
                    Helpers.AddRoomItems(room, true);
                return;
            case "Bathroom":
                room.TheFirstTime().PlayerEnterRoom().Do(_ =>
                {
                    int currentSteps = Player.steps;
                    int stepChange = 10 - (currentSteps % 10);
                    room.AddMultiplier(room.Alias, () => stepChange);
                    Player.ChangeSteps(stepChange);
                });
                break;
            case "Bedroom":
                room.TheFirstTime().PlayerExitRoom()
                    .Do(_ => Player.ChangeSteps(room.CalculatePoints()));
                break;
            case "Boiler Room":
                room.EveryTime().RoomConnected().PowerThatRoom();
                break;
            case "Boudoir":
                room.TheFirstTime().RoomIsDrafted().AddItemToRoom(new CategoryKey(RoomCategory.RestRoom));
                break;
            case "Bunk Room":
                //double draft
                room.TheFirstTime().
                    RoomIsDrafted().Do(evt =>
                    {
                        GameManager.roomDict[(room.coordinate + Vector2Int.one) * -1] = room;
                        room.onDrafted?.Invoke(evt);
                        GameEvent.onDraftedRoom?.Invoke(new(evt.Coordinates,
                            GameManager.roomDict[evt.Coordinates]));
                    });
                //double recolor
                room.EveryTime().RoomChangedCategory().
                    Do(evt => room.onCategoryChanged?.Invoke(evt));
                //double connect
                bool retrigger = false;
                room.EveryTime().RoomConnected().Where(_ => retrigger = !retrigger)
                    .Do(evt => 
                        Helpers.ConnectRooms(evt.baseRoom, evt.connectedRoom));
                break;
            case "Cassino":
                room.TheFirstTime().PlayerEnterRoom().Do(evt =>
                {
                    int r = Random.Range(0, 100);
                    if (r < 70) // gotta lie to the player sometimes
                    {
                        Player.ChangeCoins(Player.coins);
                        MessageWindow.ShowMessage($"Luck is on your side, your coins doubled!!!");
                    }
                    else
                    {
                        Player.ChangeCoins(-(Player.coins / 2));
                        MessageWindow.ShowMessage($"That's too bad, you lost half your coins...");
                    }
                });
                break;
            case "Cemetery":
                room.EveryTime().RoomsAreDrawn().Where(DraftedFromHere).Do(evt =>
                    evt.IncreaseChanceOfDrawing(target => target.Type == RoomType.DeadEnd,.6f, _ =>
                        Helpers.CreateSpareRoom(possibleTypes: new List<RoomType>() { RoomType.DeadEnd })));
                break;
            case "Chapel":
                room.EveryTime().PlayerEnterRoom().ChangePlayerCoins(-1);
                break;
            case "Cloister":
                //contains a placed statue
                ItemUtilities.Statue(true).AddItemToRoom(room);
                //bonus for each decoration
                room.AddBonus(room.Alias, DecorationBonus);
                int DecorationBonus()
                {
                    int bonus = 0;
                    for (int i = 0; i < room.items.Count; i++)
                    {
                        Decoration decoration = room.items[i] as Decoration;
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
                    amount = 10,
                    name = "Banana",
                    description = "Gain +4 steps",
                    OnBuy = () => ItemUtilities.Banana.PickUp()
                };
                PurchaseData keyCommissary = new()
                {
                    cost = 5,
                    amount = 5,
                    name = "Key",
                    description = "Used to draft powerful floorplans",
                    OnBuy = () => new Key(1).PickUp()
                };
                PurchaseData dice = new()
                {
                    cost = 8,
                    amount = 5,
                    name = "Dice",
                    description = "Used to reroll drawn floorplans",
                    OnBuy = () => new Dice(1).PickUp()
                };
                PurchaseData sledgeHammerCommissary = new()
                {
                    cost = 10,
                    amount = 1,
                    name = "Sledge Hammer",
                    description = "While <b>Active</b>, the next time you move towards a closed <b>Door</b> will open it.",
                    OnBuy = () => new SledgeHammer().PickUp()
                };

                List<PurchaseData> commissaryList = new(3)
                {
                    bananaCommissary,
                    keyCommissary,
                    dice,
                    sledgeHammerCommissary
                };
                room.TheFirstTime().RoomIsDrafted().SetupRoomShop(room.Name, commissaryList);
                break;
            case "Conservatory":
                new CategoryKey().AddItemToRoom(room);
                break;
            case "Courtyard":
                new Key(3).AddItemToRoom(room);
                break;
            case "Dark Room":
                room.EveryTime().ModifiedDraw().Where(DraftedFromHere).Do(evt =>
                {
                    for (int i = 0; i < evt.drawnRooms.Length; i++)
                    {
                        var drawnRoom = evt.drawnRooms[i];
                        drawnRoom.Name = $"Dark {drawnRoom.Name}";
                        drawnRoom.Description = "-";
                        drawnRoom.AddCategory(RoomCategory.MysteryRoom);
                    }
                });
                break;
            case "Den":
                room.TheFirstTime().RoomIsDrafted().AddItemToRoom(new Key(1));
                break;
            case "Dining Room":
                int stepsFromFood = 0;
                ItemUtilities.Meal().AddItemToRoom(room);
                room.AddBonus(room.Alias, () => stepsFromFood);
                room.EveryTime().ItemCollected().Where(evt => evt.item is Food).Do(evt => stepsFromFood += (evt.item as Food).stepGain);
                break;
            case "Dormitory":
                //when you connect a rest room, add a snack to this room
                room.EveryTime().RoomConnected().
                    Where(IsOfCategory(RoomCategory.RestRoom)).
                    Do(evt => ItemUtilities.Orange.
                        AddItemToRoom(evt.Room));
                room.EveryTime().AnyRoomChangeCategory().
                    Where(GainedCategory(RoomCategory.RestRoom)).
                    Where(IsConnectedToRoom(room)).
                    Do(evt => ItemUtilities.Orange.
                        AddItemToRoom(evt.Room));
                break;
            case "Drawing Room":
                int startAmount = 0;
                room.EveryTime().PlayerEnterRoom().Do(_ =>
                {
                    startAmount = Player.dices;
                    //Debug.Log($"entered room with {startAmount} dices");
                    Player.dices += 2;
                });
                room.EveryTime().ItemCollected().
                    Where(_ => ReferenceEquals(room, Helpers.CurrentRoom()), evt => evt.item is Dice).
                    Do(evt =>
                {
                    //Debug.Log($"Gained dice while on drawing room");
                    Dice dice = evt.item as Dice;
                    startAmount += dice.diceAmount;
                });
                room.EveryTime().PlayerExitRoom().Do(_ => Player.dices = Mathf.Min(startAmount, Player.dices));
                break;
            case "Energy Room":
                PurchaseData v2Battery = new()
                {
                    cost = 15,
                    amount = 3,
                    name = "V2 Battery",
                    description = "Place on floorplans to multiply their total points by 2",
                    OnBuy = () => new Battery(activate: true).PickUp()
                };
                PurchaseData v3Battery = new()
                {
                    cost = 20,
                    amount = 2,
                    name = "V3 Battery",
                    description = "Place on floorplans to multiply their total points by 3",
                    OnBuy = () => new Battery(3, true).PickUp()
                };
                PurchaseData v4Battery = new()
                {
                    cost = 25,
                    amount = 1,
                    name = "V4 Battery",
                    description = "Place on floorplans to multiply their total points by 4",
                    OnBuy = () => new Battery(4, true).PickUp()
                };
                
                List<PurchaseData> energyRoomList = new() { v2Battery, v3Battery, v4Battery };
                room.TheFirstTime().RoomIsDrafted().SetupRoomShop(room.name, energyRoomList);
                break;
            case "Gallery":
                int visits = 0;
                room.AddBonus(room.Alias, () => visits);
                room.EveryTime().PlayerEnterRoom().Where(_ => Player.coins > Player.minCoins).Do(_ =>
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
                room.AddBonus(room.Alias, () => bonusPoints);
                List<PurchaseData> giftList = new () { one, three, five, ten };
                room.TheFirstTime().RoomIsDrafted().SetupRoomShop(room.Name, giftList);
                break;
            case "Great Hall":
                //extra points for each different type of room connected
                RoomCategory connectedCategories = 0;
                room.EveryTime().RoomConnected()
                    .Do(AddToConnectedCategories).
                    AddPointBonusToThatRoom(() => NumberUtil.SeparateBits((int)connectedCategories).Length * 2);
                room.EveryTime().AnyRoomChangeCategory().
                    Where(IsConnectedToRoom(room)).
                    Do(AddToConnectedCategories);

                void AddToConnectedCategories(RoomEvent evt) =>
                    connectedCategories |= evt.Room.Category;
                break;
            case "Guest Bedroom":
                //essentialy free to move in
                room.EveryTime().PlayerEnterRoom().ChangePlayerSteps(2);

                int gBedroomBonus = 5;
                //extra points for each connected rest room
                room.EveryTime().RoomConnected().
                    Where(IsOfCategory(RoomCategory.RestRoom)).
                    AddPointsToThatRoom(gBedroomBonus);
                room.EveryTime().AnyRoomChangeCategory().
                    Where(IsConnectedToRoom(room)).
                    Where(GainedCategory(RoomCategory.RestRoom)).
                    AddPointsToThatRoom(gBedroomBonus);
                break;
            case "Gymnasium":
                room.EveryTime().PlayerEnterRoom().ChangePlayerSteps(-2);
                break;
            case "Hallway Closet":
                int hallwayClosetItemCount = 2;
                for (int i = 0; i < hallwayClosetItemCount; i++)
                    Helpers.AddRoomItems(room, true);
                
                if (ReferenceEquals(draftedRoom, null) || !draftedRoom.IsOfCategory(RoomCategory.Hallway)) return;
                //add rare item
                var hallwayClosetPool = room.ItemPool();
                hallwayClosetPool.ChangeRarities(0,0,1,0);
                hallwayClosetPool.PickRandom().Invoke().AddItemToRoom(room);
                return;
            case "Haunted Room":
                Room haunted = null;
                var evtListener = room.EveryTime().DrawnRoomChange();
                evtListener.AddAction(HauntDraft);
                room.EveryTime().AnyRoomIsDrafted().Do(evt =>
                {
                    if (evt.Room.original != haunted) return;
                    evtListener.RemoveAction(HauntDraft);
                });
                void HauntDraft(DrawRoomEvent evt)
                {
                    haunted = room.CreateInstance(Room.IDToDirection(room.entranceId));

                    //Add to pool
                    int id = Random.Range(0, evt.drawnRooms.Length - 1);
                    evt.drawnRooms[id] = haunted;
                }
                break;
            case "Hovel":
                //buff rest rooms
                room.ForEveryRoom(IsOfCategory(RoomCategory.RestRoom), 
                    evt => evt.Room.AddBonus(room.Alias, HovelBonus));
                room.EveryTime().AnyRoomChangeCategory().
                    Where(GainedCategory(RoomCategory.RestRoom)).
                    AddPointBonusToThatRoom(HovelBonus);
                int HovelBonus() => 1;
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
                room.TheFirstTime().RoomIsDrafted().SetupRoomShop(room.Name, kitchenList);
                break;
            case "Laundry Room":
                room.EveryTime().RoomIsDrafted().AddPointBonusToRoom(() =>
                    NumberUtil.SeparateBits((int)room.Category).Length * 5);

                room.EveryTime().RoomConnected().Do(evt =>
                {
                    var categories = NumberUtil.SeparateBits((int)evt.Room.Category);
                    for (int i = 0; i < categories.Length; i++)
                        room.AddCategory((RoomCategory)categories[i]);
                });
                room.EveryTime().AnyRoomChangeCategory().Where(IsConnectedToRoom(room))
                    .Where(IsNot(room)).Do(evt => room.AddCategory(evt.category));
                break;
            case "Library":
                room.EveryTime().ModifiedDraw().Where(DraftedFromHere).Do(evt =>
                {
                    for (int i = 0; i < evt.drawnRooms.Length; i++)
                    {
                        var costLessFloorplan = evt.drawnRooms[i].
                            CreateInstance(Room.IDToDirection(evt.drawnRooms[i].entranceId));
                        costLessFloorplan.keyCost = 0;
                        evt.drawnRooms[i] = costLessFloorplan;
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
                CategoryKey keyColor = new();
                PurchaseData colorKey = new()
                {
                    cost = 8,
                    amount = 1,
                    name = keyColor.Name,
                    description = "Guarantee you draw rooms of the same category",
                    OnBuy = () => new CategoryKey(keyColor.floorCategory).PickUp()
                };
                PurchaseData sledgeHammer = new()
                {
                    cost = 10,
                    amount = 3,
                    name = "SledgeHammer",
                    description = "Move towards an closed connection to open it.",
                    OnBuy = () => new SledgeHammer().PickUp()
                };

                List<PurchaseData> locksmithList = new() { key, keyBundle, colorKey, sledgeHammer };
                room.TheFirstTime().RoomIsDrafted().SetupRoomShop(room.Name, locksmithList);
                break;
            case "Master Bedroom":
                //buffs all rest room for each rest room
                int otherBonus = 0;
                room.ForEveryRoom(IsOfCategory(RoomCategory.RestRoom), MBedroomBonus);
                room.EveryTime().AnyRoomChangeCategory().
                    Where(GainedCategory(RoomCategory.RestRoom)).
                    Do(MBedroomBonus);

                void MBedroomBonus(RoomEvent evt)
                {
                    otherBonus += 1;
                    evt.Room.AddBonus(room.Alias, () => otherBonus);
                }
                break;
            case "Mail Room":
                const int draftsNeeded = 3;
                int count = 0;
                room.TheNext_Times(draftsNeeded).AnyRoomIsDrafted().Where(IsNot(room)).Do(_ =>
                {
                    count++;
                    if (count < draftsNeeded) return;
                    MessageWindow.ShowMessage("Your package has been delivered!!");
                    var picker = room.ItemPool();
                    //add common item
                    picker.ChangeRarities(1, 0, 0, 0);
                    picker.PickRandom().Invoke().AddItemToRoom(room);
                    //add uncommon item
                    picker.ChangeRarities(0, 1, 0, 0);
                    picker.PickRandom().Invoke().AddItemToRoom(room);
                    //add rare item
                    picker.ChangeRarities(0, 0, 1, 0);
                    picker.PickRandom().Invoke().AddItemToRoom(room);
                });
                break;
            case "Office":
                Coin payment = new(2);
                //floorplans of the same category gain coins
                room.ForEveryRoom(MatchCategoryWith(room),
                    evt => payment.AddItemToRoom(evt.Room));
                //when a floorplan gains a category, pay them too
                room.EveryTime().AnyRoomChangeCategory().Where(evt =>
                        NumberUtil.ContainsAnyBits((int)room.Category, (int)evt.category)).
                    Where(IsNot(room)).
                    AddItemToThatRoom(payment);
                //when it gains a category, pay floorplans of that category
                room.EveryTime().RoomChangedCategory().Do(evt =>
                {
                    //pay only the new floorplans
                    RoomCategory categories = room.Category ^ evt.category;
                    room.ForEveryRoom(NotReceivedPayment, evt =>
                        payment.AddItemToRoom(evt.Room));

                    bool NotReceivedPayment(RoomEvent floorplanEvent) =>
                        NumberUtil.ContainsAnyBits
                            ((int)floorplanEvent.Room.Category, (int)room.Category) 
                        &&
                        !NumberUtil.ContainsAnyBits
                            ((int)floorplanEvent.Room.Category, (int)categories);
                });
                break;
            case "Pantry":
                new Coin().AddItemToRoom(room);
                new Food().AddItemToRoom(room);
                break;
            case "Pirate Safe":
                Room treasure = null;
                string treasureName = "Treasure Room";
                string treasureAlias = "Treasure";
                string treasureDescription = "Contains 3 <b>Treasures</b>.";
                int treasurePoints = 0;
                
                room.TheFirstTime().PlayerEnterRoom().Do(evt =>
                {
                    MessageWindow.ShowMessage("There's a <b>Treasure</b> in the house!!!", () =>
                    {
                        Helpers.GetHouseData(out var occupied, out var empty);
                        if (empty is { Count: > 0 })
                        {
                            //Draft on empty space
                            Vector2Int coordinate = empty[Random.Range(0, empty.Count)];
                            var types = Helpers.GetPossibleRoomTypes(coordinate, out var slots);
                            var entrance = slots[Random.Range(0, slots.Count)];
                            treasure = Helpers.CreateRoom(treasureName, treasureDescription, treasurePoints,
                                types[Random.Range(0, types.Count)], RoomCategory.StorageRoom, alias: treasureAlias,
                                entrance: entrance, onDraftEffect: OnDraftTreasure);
                            treasure.CorrectRotation(slots);
                            GameManager.PlaceRoom(treasure, coordinate);
                        }
                        else if (occupied is { Count: > 0 })
                        {
                            //Turn another room into it
                            Vector2Int coordinate = occupied[Random.Range(0, occupied.Count)];
                            while (coordinate == room.coordinate)
                                coordinate = occupied[Random.Range(0, occupied.Count)];

                            treasure = GameManager.roomDict[coordinate];
                            treasure.Name = $"{treasureAlias} {treasure.Alias}";
                            treasure.Description = $"{treasure.Description}\nI have treasure!!";
                            treasure.AddCategory(RoomCategory.StorageRoom);
                            OnDraftTreasure(new(coordinate));
                            treasure.OnChanged?.Invoke();
                        }
                    });
                });

                void OnDraftTreasure(CoordinateEvent evt)
                {
                    //add treasures
                    new Coin(30).AddItemToRoom(treasure);
                    new Key(20).AddItemToRoom(treasure);
                    new Dice(10).AddItemToRoom(treasure);
                    new Food(20){Name = "Stamina Potion"}.AddItemToRoom(treasure);
                    ItemUtilities.Treasure().AddItemToRoom(treasure);
                    new SledgeHammer().AddItemToRoom(treasure);
                    new Battery(4).AddItemToRoom(treasure);
                }
                break;
            case "Pump Room":
                room.EveryTime().RoomConnected().AddPointBonusToThatRoom(() => room.basePoints);
                break;
            case "Secret Passage":
                //add connection to drafted floorplans
                room.EveryTime().RoomsAreDrawn().Where(DraftedFromHere).Do(evt =>
                {
                    for (int i = 0; i < evt.drawnRooms.Length; i++)
                    {
                        Room drawnFloorplan = evt.drawnRooms[i];
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
                            Helpers.AddRoomItems(drawnFloorplan, true);
                            continue;
                        }
                        //otherwise open a connection on floorplan
                        Room newFloorplan = drawnFloorplan.CreateInstance(Room.IDToDirection(drawnFloorplan.entranceId));
                        newFloorplan.OpenConnection(closedConnectionID);
                        evt.drawnRooms[i] = newFloorplan;
                        //Debug.Log($"open connection on {newFloorplan} => {Floorplan.IDToDirection(closedConnectionID)}({newFloorplan.connections[closedConnectionID]})");
                    }
                });

                //if there's already a floorplan on a openConnection, connect to it
                room.TheFirstTime().RoomIsDrafted().Do(evt =>
                {
                    for (int i = 0; i < room.connections.Length; i++)
                    {
                        if (i == room.entranceId || !room.connections[i]) continue;
                        Vector2Int connection = Room.IDToDirection(i);
                        if (!GameManager.roomDict.TryGetValue(room.coordinate + connection, out var otherFloorplan)) continue;
                        int connectionID = Room.DirectionToID(-connection);
                        if (otherFloorplan.connections[connectionID]) continue;
                        //Debug.Log($"{otherFloorplan.Name} may connect with {floorplan.Name}");
                        otherFloorplan.OpenConnection(connectionID);
                    }
                });
                break;
            case "Simple Room":
                room.EveryTime().ModifiedDraw().Where(DraftedFromHere).Do(evt =>
                {
                    for (int i = 0; i < evt.drawnRooms.Length; i++)
                    {
                        var original = evt.drawnRooms[i];
                        evt.drawnRooms[i] = Helpers.CreateSpareRoom(new List<RoomCategory>() { original.Category}, new List<RoomType>() { original.Type });
                    }
                });
                break;
            case "Storeroom":
                int storeroomItemCount = 3;
                var items = room.ItemPool();
                items.ChangeRarities(1, 0, 0, 0);
                for (int i = 0; i < storeroomItemCount; i++)
                    items.PickRandom().Invoke().AddItemToRoom(room);
                return;
            case "Spare Room":
                int doorCount = room.original.DoorCount;
                switch (room.Category)
                {
                    case RoomCategory.CursedRoom:
                        room.TheFirstTime().RoomIsDrafted().ChangePlayerSteps(-(11 - (2 * doorCount)));
                        break;
                    case RoomCategory.RestRoom:
                        room.TheFirstTime().PlayerEnterRoom().ChangePlayerSteps(15 - (3 * doorCount));
                        break;
                    case RoomCategory.Shop:
                        new Coin(12 - (2 * doorCount)).AddItemToRoom(room);
                        break;
                    case RoomCategory.StorageRoom:
                        int spareroomItems = 5 - doorCount;
                        var spareroomPicker = room.ItemPool();
                        for (int i = 0; i < spareroomItems; i++)
                            spareroomPicker.PickRandom().Invoke().AddItemToRoom(room);
                        return;
                    case RoomCategory.Hallway:
                        room.EveryTime().RoomConnected().AddPointsToThatRoom(5 - doorCount);
                        break;
                    case RoomCategory.MysteryRoom:
                        room.TheFirstTime().RoomIsDrafted().Do(_
                            => room.AddMultiplier(room.Alias, () => 6 - doorCount));
                        break;
                }
                break;
            case "Terrace":
                RarityPicker<Func<Item>> terraceItems = room.ItemPool();
                terraceItems.ChangeRarities(0,1,0,0);
                terraceItems.PickRandom().Invoke().AddItemToRoom(room);
                break;
            case "Trading Post":
                List<PurchaseData> trades = new();
                room.EveryTime().PlayerEnterRoom().Do(_ => UpdateTrades());
                room.TheFirstTime().RoomIsDrafted().SetupRoomShop(room.Name, trades);

                void UpdateTrades()
                {
                    trades.Clear();
                    if (Player.keys > 0)
                    {
                        trades.Add(new()
                        {
                            name = "Key",
                            description = "Trade a Key for 2 coins",
                            amount = 2,
                            OnBuy = () =>
                            {
                                Player.ChangeKeys(-1);
                                Player.ChangeCoins(2);
                                RefreshTradingPost();
                            }
                        });
                    }
                    if (Player.dices > 0)
                    {
                        trades.Add(new()
                        {
                            name = "Dice",
                            description = "Trade a Dice for 2 keys",
                            amount = 2,
                            OnBuy = () =>
                            {
                                Player.dices--;
                                Player.ChangeKeys(2);
                                RefreshTradingPost();
                            }
                        });
                    }

                    for (int i = 0; i < Player.items.Count; i++)
                    {
                        if (trades.Count >= 5) break;
                        var item = Player.items[i];
                        bool rare = item is SledgeHammer or Battery;
                        int diceAmount = rare ? 4 : 2;
                        trades.Add(new()
                        {
                            name = $"{item.Name}",
                            description = $"Trade {item.Name} for {diceAmount} dices",
                            amount = 2,
                            OnBuy = () =>
                            {
                                Player.items.Remove(item);
                                Player.dices += diceAmount;
                                RefreshTradingPost();
                            }
                        });
                    }
                }

                void RefreshTradingPost()
                {
                    UpdateTrades();
                    ShopWindow.OpenShop(room.Name, trades);
                }
                break;
            case "Tunnel":
                //Aways draw a tunnel when drafting from tunnel
                room.EveryTime().DrawnRoomChange().Where(DraftedFromHere).Do(evt =>
                {
                    Room tunnel = room.original.CreateInstance(Room.IDToDirection(room.entranceId));
                    
                    //Add to pool
                    int id = Random.Range(0, evt.drawnRooms.Length - 1);
                    evt.drawnRooms[id] = tunnel;
                    
                    //remove exit if leads to an invalid position
                    for (int i = 0; i < tunnel.connections.Length; i++)
                    {
                        if (i == room.entranceId) continue;
                        int exitId = i;
                        var exitCoordinate = evt.targetCoordinate + Room.IDToDirection(exitId);
                        if (GridManager.instance.ValidCoordinate(exitCoordinate)) continue;
                        tunnel.CloseConnection(exitId);
                    }
                });
                //surprise if reach the edge
                room.TheFirstTime().RoomIsDrafted().
                    Where(_ => room.Type == RoomType.DeadEnd).AddItemToRoom(new Key(5));
                break;
            case "Utility Closet":
                //power all rooms of the same category
                room.ForEveryRoom(MatchCategoryWith(room),
                    evt => evt.Room.AddMultiplier(room.Alias, () => 2));
                //when a floorplan gains a category, power them too
                room.EveryTime().AnyRoomChangeCategory().Where(evt =>
                    NumberUtil.ContainsAnyBits((int)room.Category, (int)evt.category)).
                    Where(IsNot(room)).
                    PowerThatRoom();
                //when it gains a category, power floorplans of that category
                room.EveryTime().RoomChangedCategory().Do(evt =>
                {
                    //power only the new floorplans
                    RoomCategory categories = room.Category ^ evt.category;
                    room.ForEveryRoom(NotPoweredMatch, evt => 
                        evt.Room.AddMultiplier(room.Alias, () => 2));

                    bool NotPoweredMatch(RoomEvent floorplanEvent) =>
                        NumberUtil.ContainsAnyBits
                        ((int)floorplanEvent.Room.Category, (int)room.Category) 
                        &&
                        !NumberUtil.ContainsAnyBits
                        ((int)floorplanEvent.Room.Category, (int)categories);
                });
                break;
            case "Vault":
                bool canCollect = false;
                Coin coins = new(GameManager.roomDict.Count * 2);
                room.EveryTime().AnyRoomIsDrafted().Do(_ =>
                {
                    coins.amount += 2;
                    if(canCollect) return;
                    coins.AddItemToRoom(room);
                    canCollect = true;
                });
                room.EveryTime().ItemCollected().Where(evt => evt.item == coins).Do(_ =>
                {
                    coins.amount = 0;
                    canCollect = false;
                });
                break;
            case "Vestibule":
                room.EveryTime().RoomConnected().
                    Do(evt =>
                    {
                        for (int i = 0; i < room.connectedRooms.Count; i++)
                        {
                            Room currentFloorplan = room.connectedRooms[i];
                            if (currentFloorplan == evt.connectedRoom) continue;
                            if (currentFloorplan.connectedRooms.Contains(evt.connectedRoom)) continue;
                            Helpers.ConnectRooms(currentFloorplan, evt.connectedRoom);
                            Debug.Log($"{room.Name} connected {currentFloorplan.Name} to {evt.connectedRoom.Name}");
                        }
                    });
                break;
            case "Walk-In Closet":
                int walkinClosetItemCount = 4;
                for (int i = 0; i < walkinClosetItemCount; i++)
                    Helpers.AddRoomItems(room, true);
                
                if (ReferenceEquals(draftedRoom, null) || !draftedRoom.IsOfCategory(RoomCategory.RestRoom)) return;
                //add uncommon items
                var walkinClosetPool = room.ItemPool();
                walkinClosetPool.ChangeRarities(0,1,0,0);
                walkinClosetItemCount = 2;
                for (int i = 0; i < walkinClosetItemCount; i++)
                    walkinClosetPool.PickRandom().Invoke().AddItemToRoom(room);
                return;
            case "Weight Room":
                room.TheFirstTime().RoomIsDrafted().AddItemToRoom(ItemUtilities.EnergyBar());
                room.TheFirstTime().PlayerExitRoom().Do(_ =>
                {
                    int halfStep = Player.steps / 2;
                    Player.ChangeSteps(-halfStep);
                    room.AddBonus(room.Alias, () => halfStep);
                });
                break;
            case "":
                return;
        }
        Helpers.AddRoomItems(room);
        bool DraftedFromHere<T>(T evt) where T : Event => Helpers.CurrentRoom() == room;
    }

    #region EffectCreation
    public static Effect TheFirstTime(this Room room) => new (room, 1);
    public static Effect EveryTime(this Room room) => new (room);
    public static Effect TheNext_Times(this Room room, uint times) => new(room, (int)times);
    public static void ForEveryRoom(this Room room, Func<RoomEvent, bool> condition,
        Action<RoomEvent> action)
    {
        foreach (var roomPair in GameManager.roomDict)
        {
            if (!CheckCondition(roomPair)) continue;
            action?.Invoke(new(roomPair.Key, roomPair.Value));
        }
        room.EveryTime().AnyRoomIsDrafted().Where(condition).Do(action);

        bool CheckCondition(KeyValuePair<Vector2Int, Room> data) => condition?.Invoke(new(data.Key, data.Value)) ?? true;
    }
    #endregion

    #region EventListeners
    //Floorplan
    public static EventListener<Action<CoordinateEvent>, CoordinateEvent>
        RoomIsDrafted(this Effect effect) => new(effect,
        (a) => effect.room.onDrafted += a,
        (a) => effect.room.onDrafted -= a);
    
    public static EventListener<Action<RoomConnectedEvent>, RoomConnectedEvent>
        RoomConnected(this Effect effect) => new(effect,
        (a) => effect.room.onConnectToRoom += a,
        (a) => effect.room.onConnectToRoom -= a);
    public static EventListener<Action<Event>, Event> PlayerEnterRoom(this Effect effect) => new(effect,
        (a) => effect.room.onEnter += a,
        (a) => effect.room.onEnter -= a);
    public static EventListener<Action<Event>, Event> PlayerExitRoom(this Effect effect) => new(effect,
        (a) => effect.room.onExit += a,
        (a) => effect.room.onExit -= a);
    
    //GameEvent
    public static EventListener<Action<RoomEvent>, RoomEvent>
        AnyRoomIsDrafted(this Effect effect) => new(effect,
        (a) => GameEvent.onDraftedRoom += a,
        (a) => GameEvent.onDraftedRoom -= a);
    
    public static EventListener<Action<RoomConnectedEvent>, RoomConnectedEvent>
        AnyRoomConnected(this Effect effect) => new(effect,
        (a) => GameEvent.onConnectRooms += a,
        (a) => GameEvent.onConnectRooms -= a);
    
    public static EventListener<Action<RoomEvent>, RoomEvent>
        PlayerEnterAnyRoom(this Effect effect) => new(effect,
        (a) => GameEvent.onEnterRoom += a,
        (a) => GameEvent.onEnterRoom -= a);

    public static EventListener<Action<RoomEvent>, RoomEvent>
        PlayerExitAnyRoom(this Effect effect) => new(effect,
        (a) => GameEvent.onExitRoom += a,
        (a) => GameEvent.onExitRoom -= a);

    public static EventListener<Action<DrawRoomEvent>, DrawRoomEvent>
        RoomsAreDrawn(this Effect effect) => new(effect,
        (a) => GameEvent.onDrawRooms += a,
        (a) => GameEvent.onDrawRooms -= a);
    public static EventListener<Action<DrawRoomEvent>, DrawRoomEvent>
        DrawnRoomChange(this Effect effect) => new(effect,
        (a) => GameEvent.onDrawChange += a,
        (a) => GameEvent.onDrawChange -= a);
    public static EventListener<Action<DrawRoomEvent>, DrawRoomEvent>
        ModifiedDraw(this Effect effect) => new(effect,
        (a) => GameEvent.onModifyDraw += a,
        (a) => GameEvent.onModifyDraw -= a);
    public static EventListener<Action<ItemEvent>, ItemEvent>
        ItemCollected(this Effect effect) => new(effect,
        (a) => GameEvent.onCollectItem += a,
        (a) => GameEvent.onCollectItem -= a);

    public static EventListener<Action<CategoryChangeEvent>, CategoryChangeEvent>
        RoomChangedCategory(this Effect effect) => new(effect,
        a => effect.room.onCategoryChanged += a,
        a => effect.room.onCategoryChanged -= a);
    public static EventListener<Action<CategoryChangeEvent>, CategoryChangeEvent>
        AnyRoomChangeCategory(this Effect effect) => new(effect,
        a => GameEvent.onRoomCategoryChanged += a,
        a => GameEvent.onRoomCategoryChanged -= a);

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
    public static EventListener<Action<T>,T> AddItemToRoom<T>(this EventListener<Action<T>,T> listener, Item item) where T : Event =>
        listener.Do(_ => item.AddItemToRoom(listener.effect.room));
    public static EventListener<Action<T>, T> AddItemToThatRoom<T>(this EventListener<Action<T>, T> listener, Item item) where T : CoordinateEvent =>
        listener.Do(evt => item.AddItemToRoom(GameManager.roomDict[evt.Coordinates]));
    public static EventListener<Action<T>,T> AddPointsToRoom<T>(this EventListener<Action<T>,T> listener, int amount) where T : Event =>
        listener.Do(_ => listener.effect.room.AddBonus(listener.effect.room.Alias, () => amount));
    public static EventListener<Action<T>,T> AddPointBonusToRoom<T>(this EventListener<Action<T>,T> listener, Func<int> bonus) where T : Event =>
        listener.Do(_ => listener.effect.room.AddBonus(listener.effect.room.Alias, bonus));
    public static EventListener<Action<T>,T> PowerRoom<T>(this EventListener<Action<T>,T> listener) where T : Event =>
        listener.Do(_ => listener.effect.room.AddMultiplier(listener.effect.room.Alias, () => 2));
    public static EventListener<Action<T>,T> AddPointsToThatRoom<T>(this EventListener<Action<T>,T> listener, int amount) where T : CoordinateEvent =>
        listener.Do(evt => GameManager.roomDict[evt.Coordinates].AddBonus(listener.effect.room.Alias, () => amount));
    public static EventListener<Action<T>,T> AddPointBonusToThatRoom<T>(this EventListener<Action<T>,T> listener, Func<int> amount) where T : CoordinateEvent =>
        listener.Do(evt => GameManager.roomDict[evt.Coordinates].AddBonus(listener.effect.room.Alias, amount));
    public static EventListener<Action<T>,T> PowerThatRoom<T>(this EventListener<Action<T>, T> listener) where T : CoordinateEvent =>
        listener.Do(evt => GameManager.roomDict[evt.Coordinates].AddMultiplier(listener.effect.room.Alias, () => 2));
    public static EventListener<Action<T>,T> SetupRoomShop<T>(this EventListener<Action<T>, T> listener, string title, List<PurchaseData> shopList)
        where T : CoordinateEvent
    {
        listener.Do(CreateShop);
        return listener;
        void CreateShop(CoordinateEvent evt)
        {
            if (!GameManager.roomDict.TryGetValue(evt.Coordinates, out var floorplan)) return;
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

    public static Func<T, bool> Not<T>(Func<T, bool> condition) where T : Event => 
        evt => !condition.Invoke(evt);
    public static Func<RoomEvent, bool> IsOfCategory(RoomCategory type) =>
        evt => NumberUtil.ContainsBytes((int)evt.Room.Category, (int)type);
    public static Func<RoomEvent, bool> MatchCategoryWith(Room room)
    {
        return evt =>
        {
            int[] categories = NumberUtil.SeparateBits((int)evt.Room.Category);
            for (int i = 0; i < categories.Length; i++)
                if (room.IsOfCategory((RoomCategory)categories[i]))
                    return true;
            return false;
        };
    }

    public static Func<RoomEvent, bool> IsNot(Room room) =>
        evt => evt.Room != room;
    public static Func<RoomEvent, bool> IsConnectedToRoom(Room room) =>
        evt => room.connectedRooms.Contains(evt.Room);
    public static Func<CategoryChangeEvent, bool> GainedCategory(RoomCategory category) =>
        evt => evt.category == category;
    
    #endregion
}