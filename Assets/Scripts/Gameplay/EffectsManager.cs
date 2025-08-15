using System;
using System.Collections.Generic;
using UnityEngine;
using Lenix.NumberUtilities;
using Random = UnityEngine.Random;

public static class EffectsManager
{
    public static void AddFloorplanEffect(Floorplan floorplan)
    {
        Floorplan draftedFloorplan = floorplan.DraftedFrom();
        switch (floorplan.Name)
        {
            case "Attic":
                int atticItemCount = 6;
                for (int i = 0; i < atticItemCount; i++)
                    ItemsManager.AddFloorplanItems(floorplan, true);
                break;
            case "Bathroom":
                floorplan.TheFirstTime().PlayerEnterFloorplan().Do(_ =>
                {
                    int currentSteps = Player.steps;
                    currentSteps = Mathf.CeilToInt(currentSteps / 10f);
                    Player.ChangeSteps((currentSteps * 10) - Player.steps);
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
                RarityPicker<Item> cloisterItems = ItemsManager.GetPossibleFloorplanItems(floorplan);
                cloisterItems.ChangeRarities(1,0,0,0);
                cloisterItems.PickRandom().Place(floorplan);
                break;
            case "Commissary":
                PurchaseData bananas = new()
                {
                    cost = 4,
                    amount = 3,
                    name = "Banana",
                    description = "Gain +3 steps",
                    OnBuy = () => new Food(3).PickUp()
                };
                PurchaseData keys = new()
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
                    amount = 2,
                    name = "Dice",
                    description = "Used to reroll drawn floorplans",
                    OnBuy = () => new Dice(1).PickUp()
                };
                PurchaseData keyBundle = new()
                {
                    cost = 12,
                    amount = 1,
                    name = "Key bundle",
                    description = "Used to draft powerful floorplans, now in a neat package",
                    OnBuy = () => new Key(3).PickUp()
                };
                List<PurchaseData> commissaryList = new() { bananas, keys, dice, keyBundle };
                floorplan.TheFirstTime().FloorplanIsDrafted().SetupFloorplanShop(floorplan.Name, commissaryList);
                break;
            case "Den":
                floorplan.TheFirstTime().FloorplanIsDrafted().AddItemToFloorplan(new Key(1));
                break;
            case "Dining Room":
                int stepsFromFood = 0;
                new Food(10) { Name = "Meal"}.Place(floorplan);
                floorplan.pointBonus.Add(floorplan.Name, () => stepsFromFood);
                floorplan.EveryTime().ItemCollected().Where(evt => evt.item is Food).Do(evt => stepsFromFood += (evt.item as Food).stepsAmount);
                break;
            case "Dormitory":
                //connected rest room gain extra points
                floorplan.EveryTime().FloorplanConnected().
                    Where(IsOfCategory(FloorCategory.RestRoom)).
                    AddPointsToThatFloorplan(2);
                //first time entering a connected restroom gain steps
                floorplan.EveryTime().FloorplanConnected().
                    Where(IsOfCategory(FloorCategory.RestRoom)).
                    Do(evt =>
                    {
                        evt.connectedFloorplan.
                            TheFirstTime().
                            PlayerEnterFloorplan().
                            ChangePlayerSteps(5);
                    });
                break;
            case "Gallery":
                int visits = 0;
                floorplan.pointBonus.Add(floorplan.Name, () => visits);
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
                floorplan.pointBonus.Add(floorplan.Name, () => bonusPoints);
                List<PurchaseData> giftList = new () { one, three, five, ten };
                floorplan.TheFirstTime().FloorplanIsDrafted().SetupFloorplanShop(floorplan.Name, giftList);
                break;
            case "Great Hall":
                //extra points for each different type of room connected
                FloorCategory connectedCategories = 0;
                floorplan.pointBonus.Add(floorplan.Name, () => NumberUtil.SeparateBits((int)connectedCategories).Length * 2);
                floorplan.EveryTime().FloorplanConnected()
                    .Do(evt => connectedCategories |= evt.connectedFloorplan.Category);
                break;
            case "Guest Bedroom":
                //essentialy free to move in
                floorplan.EveryTime().PlayerEnterFloorplan().ChangePlayerSteps(2);
                //combine points of connected rest rooms
                floorplan.EveryTime().FloorplanConnected().Where(IsOfCategory(FloorCategory.RestRoom)).Do(evt =>
                    floorplan.pointBonus.Add(floorplan.Name, evt.connectedFloorplan.CalculatePoints));
                break;
            case "Hallway Closet":
                int hallwayClosetItemCount = 2;
                if (!ReferenceEquals(draftedFloorplan, null) &&
                    draftedFloorplan.IsOfCategory(FloorCategory.Hallway))
                    hallwayClosetItemCount += 1;

                for (int i = 0; i < hallwayClosetItemCount; i++)
                    ItemsManager.AddFloorplanItems(floorplan, true);
                break;
            case "Kitchen":
                PurchaseData apple = new()
                {
                    cost = 2,
                    amount = 6,
                    name = "Apple",
                    description = "Gain +2 steps",
                    OnBuy = () => new Food(2).PickUp()
                };
                PurchaseData banana = new()
                {
                    cost = 3,
                    amount = 5,
                    name = "Banana",
                    description = "Gain +3 steps",
                    OnBuy = () => new Food(3).PickUp()
                };
                PurchaseData orange = new()
                {
                    cost = 5,
                    amount = 3,
                    name = "Orange",
                    description = "Gain +5 steps",
                    OnBuy = () => new Food(5).PickUp()
                };
                List<PurchaseData> kitchenList = new () { apple, banana, orange };
                floorplan.TheFirstTime().FloorplanIsDrafted().SetupFloorplanShop(floorplan.Name, kitchenList);
                break;
            case "Library":
                floorplan.EveryTime().FloorplansAreDrawn().Where(DraftedFromHere).Do(evt =>
                {
                    for (int i = 0; i < evt.drawnFloorplans.Length; i++)
                        evt.drawnFloorplans[i].keyCost = 0;
                });
                break;
            case "Master Bedroom":
                int selfBonus = 0;
                int otherBonus = 5;
                floorplan.pointBonus.Add(floorplan.Name, () => selfBonus);
                foreach (var room in GameManager.floorplanDict.Values)
                {
                    if (!NumberUtil.ContainsAnyBits((int)room.Category, (int)floorplan.Category)) continue;
                    Debug.Log($"buffing {room.Name}");
                    selfBonus += 2;
                    room.pointBonus.Add(floorplan.Name, () => otherBonus);
                }
                floorplan.EveryTime().AnyFloorplanIsDrafted().
                    Where(IsOfCategory(FloorCategory.RestRoom)).
                    AddPointsToThatFloorplan(otherBonus).Do(_ => selfBonus += 2);
                break;
            case "Pantry":
                new Coin().Place(floorplan);
                new Food().Place(floorplan);
                break;
            case "Pump Room":
                floorplan.EveryTime().FloorplanConnected().AddPointBonusToThatFloorplan(floorplan.CalculatePoints);
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
                            ItemsManager.AddFloorplanItems(drawnFloorplan);
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
                RarityPicker<Item> terraceItems = ItemsManager.GetPossibleFloorplanItems(floorplan);
                terraceItems.ChangeRarities(0,1,0,0);
                terraceItems.PickRandom().Place(floorplan);
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
                    new Key(5).Place(floorplan);
                    floorplan.OnChanged?.Invoke();
                });
                break;
            case "Utility Closet":
                //power all rooms of the same category
                foreach (var room in GameManager.floorplanDict.Values)
                {
                    if (!NumberUtil.ContainsAnyBits((int)room.Category, (int)floorplan.Category)) continue;
                    Debug.Log($"Powering {room.Name}");
                    room.multBonus.Add(floorplan.Name, () => 2);
                }
                //power all rooms of the same category
                floorplan.EveryTime().AnyFloorplanIsDrafted().
                    Where(MatchCategoryWith(floorplan)).
                    PowerThatFloorplan();
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
                    ItemsManager.AddFloorplanItems(floorplan, true);
                break;
            case "":
                break;
        }

        bool DraftedFromHere<T>(T evt) where T : Event => Helpers.CurrentFloorplan() == floorplan;
    }

    #region EffectCreation
    public static Effect TheFirstTime(this Floorplan floorplan) => new (floorplan, 1);
    public static Effect EveryTime(this Floorplan floorplan) => new (floorplan);
    public static Effect TheNext_Times(this Floorplan floorplan, uint times) => new(floorplan, (int)times);
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
        listener.Do(_ => item.Place(listener.effect.floorplan));
    public static EventListener<Action<T>, T> AddItemToThatFloorplan<T>(this EventListener<Action<T>, T> listener, Item item) where T : CoordinateEvent =>
        listener.Do(evt => item.Place(GameManager.floorplanDict[evt.Coordinates]));
    public static EventListener<Action<T>,T> AddPointsToFloorplan<T>(this EventListener<Action<T>,T> listener, int amount) where T : Event =>
        listener.Do(_ => listener.effect.floorplan.pointBonus.Add(listener.effect.floorplan.Name, () => amount));
    public static EventListener<Action<T>,T> AddPointBonusToFloorplan<T>(this EventListener<Action<T>,T> listener, Func<int> bonus) where T : Event =>
        listener.Do(_ => listener.effect.floorplan.pointBonus.Add(listener.effect.floorplan.Name, bonus));
    public static EventListener<Action<T>,T> PowerFloorplan<T>(this EventListener<Action<T>,T> listener) where T : Event =>
        listener.Do(_ => listener.effect.floorplan.multBonus.Add(listener.effect.floorplan.Name, () => 2));
    public static EventListener<Action<T>,T> AddPointsToThatFloorplan<T>(this EventListener<Action<T>,T> listener, int amount) where T : CoordinateEvent =>
        listener.Do(evt => GameManager.floorplanDict[evt.Coordinates].pointBonus.Add(listener.effect.floorplan.Name, () => amount));
    public static EventListener<Action<T>,T> AddPointBonusToThatFloorplan<T>(this EventListener<Action<T>,T> listener, Func<int> amount) where T : CoordinateEvent =>
        listener.Do(evt => GameManager.floorplanDict[evt.Coordinates].pointBonus.Add(listener.effect.floorplan.Name, amount));
    public static EventListener<Action<T>,T> PowerThatFloorplan<T>(this EventListener<Action<T>, T> listener) where T : CoordinateEvent =>
        listener.Do(evt => GameManager.floorplanDict[evt.Coordinates].multBonus.Add(listener.effect.floorplan.Name, () => 2));

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