using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lenix.NumberUtilities;
using Random = UnityEngine.Random;

public class EffectsManager : MonoBehaviour
{
    private void Start()
    {
        GameEvent.onDraftedFloorplan += AddFloorplanEffect;
    }

    public void AddFloorplanEffect(FloorplanEvent evt)
    {
        Vector2Int draftedCoordinates = evt.Coordinates + Floorplan.IDToDirection(evt.Floorplan.entranceId);
        Floorplan floorplan = evt.Floorplan;
        if(!GameManager.floorplanDict.TryGetValue(draftedCoordinates, out var draftedFloorplan)) return;
        switch (evt.Floorplan.Name)
        {
            case "Bedroom":
                GameEvent.OnEnterFloorplan += BedroomEffect;
                void BedroomEffect(FloorplanEvent subEvt)
                {
                    if (evt.Floorplan != subEvt.Floorplan) return;
                    Player.ChangeSteps(5);
                    GameEvent.OnEnterFloorplan -= BedroomEffect;
                }
                break;
            case "Bathroom":
                GameEvent.OnEnterFloorplan += BathroomEffect;
                void BathroomEffect(FloorplanEvent subEvt)
                {
                    if (evt.Floorplan != subEvt.Floorplan) return;
                    int currentSteps = Player.steps;
                    currentSteps = Mathf.CeilToInt(currentSteps / 10f);
                    Player.ChangeSteps((currentSteps * 10) - Player.steps);
                    GameEvent.OnEnterFloorplan -= BathroomEffect;
                }
                break;
            case "Bunk Room":
                break;
                GameEvent.onDraftedFloorplan -= AddFloorplanEffect;
                GameEvent.onDraftedFloorplan?.Invoke(evt);
                GameEvent.onDraftedFloorplan += AddFloorplanEffect;

                GameEvent.onConnectFloorplans += DoubleConnection;
                void DoubleConnection(FloorplanConnectedEvent subEvt)
                {
                    if(evt.Floorplan.ConnectedToFloorplan(subEvt, out var other)) return;
                    other.connectedFloorplans.Add(floorplan);
                    GameEvent.onConnectFloorplans -= DoubleConnection;
                    GameEvent.onConnectFloorplans?.Invoke(subEvt);
                    GameEvent.onConnectFloorplans += DoubleConnection;
                }
                break;
            case "Dormitory":
                break;
                GameEvent.onConnectFloorplans += DormitoryEffect;
                void DormitoryEffect(FloorplanConnectedEvent subEvt)
                {
                    if (!floorplan.ConnectedToFloorplan(subEvt, out var other)) return;
                    if(!NumberUtil.ContainsBytes((int)other.Category, (int)FloorCategory.RestRoom)) return;
                    //connected bedrooms gain extra points
                    other.pointBonus.Add(() => 2);
                    //first time entering a connected restroom gain steps
                    GameEvent.OnEnterFloorplan += AddStepsEffect;
                    void AddStepsEffect(FloorplanEvent addedEvt)
                    {
                        if(addedEvt.Floorplan != other) return;
                        Player.ChangeSteps(5);
                        GameEvent.OnEnterFloorplan -= AddStepsEffect;
                    }
                }
                break;
            case "Boudoir":
                GameEvent.OnEnterFloorplan += OnEnterBoudoir;
                GameEvent.OnExitFloorplan += OnExitBoudoir;
                void OnEnterBoudoir(FloorplanEvent subEvt)
                {
                    if (evt.Floorplan != subEvt.Floorplan) return;
                    GameEvent.onDrawFloorplans += IncreaseRestroomChance;
                }
                void OnExitBoudoir(FloorplanEvent subEvt)
                {
                    if (evt.Floorplan != subEvt.Floorplan) return;
                    GameEvent.onDrawFloorplans -= IncreaseRestroomChance;
                }
                void IncreaseRestroomChance(DrawFloorplanEvent evt)
                {
                    int restroomCount = 0;
                    for (int i = 0; i < evt.drawnFloorplans.Length; i++)
                    {
                        if (!NumberUtil.ContainsBytes((int)evt.drawnFloorplans[i].Category,
                                (int)FloorCategory.RestRoom)) continue;
                        restroomCount++;
                    }

                    if (restroomCount > 0) return;
                    List<Floorplan> possibleRestroom = new();
                    RarityPicker<Floorplan> modifiedList = new();
                    for (int i = 0; i < evt.possibleFloorplans.Count; i++)
                    {
                        Floorplan restRoom = evt.possibleFloorplans[i];
                        if (!NumberUtil.ContainsBytes((int)restRoom.Category, (int)FloorCategory.RestRoom))
                            continue;
                        //check if it is selected already
                        if (restRoom == evt.drawnFloorplans[0] ||
                            restRoom == evt.drawnFloorplans[1] ||
                            restRoom == evt.drawnFloorplans[2]) continue;
                        modifiedList.AddToPool(restRoom, restRoom.Rarity);
                        possibleRestroom.Add(restRoom);
                    }

                    if (possibleRestroom.Count <= 0) return;
                    int id = Random.Range(0, 2);
                    evt.drawnFloorplans[id] = modifiedList.PickRandom();
                }
                break;
            case "Guest Bedroom":
                GameEvent.OnEnterFloorplan += GuestBedroomStepsEffect;
                void GuestBedroomStepsEffect(FloorplanEvent subEvt)
                {
                    if (evt.Floorplan != subEvt.Floorplan) return;
                    Player.ChangeSteps(2);
                }
                GameEvent.onConnectFloorplans += GuestBedroomEffect;
                void GuestBedroomEffect(FloorplanConnectedEvent subEvt)
                {
                    if (!floorplan.ConnectedToFloorplan(subEvt, out var other)) return;
                    if(!NumberUtil.ContainsBytes((int)other.Category, (int)FloorCategory.RestRoom)) return;
                    //bonus points equal to connected restrooms points
                    //Debug.Log($"{floorplan.Name} connected to {other.Name}");
                    floorplan.pointBonus.Add(other.CalculatePoints);
                }
                break;
            case "Great Hall":
                //extra points for each different type of room connected
                FloorCategory connectedCategories = 0;
                floorplan.pointBonus.Add(() => NumberUtil.SeparateBits((int)connectedCategories).Length * 2);
                
                GameEvent.onConnectFloorplans += GreatHallEffect;
                void GreatHallEffect(FloorplanConnectedEvent subEvt)
                {
                    if(!floorplan.ConnectedToFloorplan(subEvt, out var other)) return;
                    connectedCategories |= other.Category;
                }
                break;
            case "Tunnel":
                //surprise if reach the edge?
                int exitId = (floorplan.entranceId + 2) % 4;
                if (!GridManager.instance.ValidCoordinate
                    (evt.Coordinates + Floorplan.IDToDirection(exitId)))
                {
                    floorplan.connections[exitId] = false;
                    floorplan.AddItemToFloorplan(new Key(5));
                    floorplan.OnChanged?.Invoke();
                }
                //Aways draw a tunnel when drafting from tunnel
                GameEvent.OnEnterFloorplan += OnEnterTunnel;
                GameEvent.OnExitFloorplan += OnExitTunnel;
                void OnEnterTunnel(FloorplanEvent subEvt)
                {
                    if (evt.Floorplan != subEvt.Floorplan) return;
                    GameEvent.onDrawFloorplans += AddTunnelToDrawnFloorplans;
                }
                void OnExitTunnel(FloorplanEvent subEvt)
                {
                    if (evt.Floorplan != subEvt.Floorplan) return;
                    GameEvent.onDrawFloorplans -= AddTunnelToDrawnFloorplans;
                }
                void AddTunnelToDrawnFloorplans(DrawFloorplanEvent evt)
                {
                    Floorplan tunnel = floorplan.original.CreateInstance(Floorplan.IDToDirection(floorplan.entranceId));
                    int id = Random.Range(0, 2);
                    evt.drawnFloorplans[id] = tunnel;
                }
                break;
            case "Vestibule":
                break;
                GameEvent.onConnectFloorplans += OnConnectVestibule;
                void OnConnectVestibule(FloorplanConnectedEvent subEvt)
                {
                    if (!floorplan.ConnectedToFloorplan(subEvt, out var other)) return;
                    for (int i = 0; i < floorplan.connectedFloorplans.Count; i++)
                    {
                        Floorplan currentFloorplan = floorplan.connectedFloorplans[i];
                        if (currentFloorplan == other) continue;
                        if (currentFloorplan.connectedFloorplans.Contains(other)) continue;
                        //Debug.Log($"{floorplan.Name} connected {currentFloorplan.Name} to {other.Name}");
                        Helpers.ConnectFloorplans(currentFloorplan, other);
                    }
                }
                break;
            case "Attic":
                RarityPicker<Item> atticItems = ItemsManager.GetPossibleFloorplanItems(floorplan);
                int atticItemCount = 6;
                for (int i = 0; i < atticItemCount; i++)
                    floorplan.AddItemToFloorplan(atticItems.PickRandom());
                break;
            case "Walk-In Closet":
                RarityPicker<Item> walkinClosetItems = ItemsManager.GetPossibleFloorplanItems(floorplan);
                int walkinClosetItemCount = 4;
                if (NumberUtil.ContainsBytes((int)draftedFloorplan.Category, (int)FloorCategory.Hallway))
                    walkinClosetItemCount += 2;

                for (int i = 0; i < walkinClosetItemCount; i++)
                    floorplan.AddItemToFloorplan(walkinClosetItems.PickRandom());
                break;
            case "Hallway Closet":
                RarityPicker<Item> hallwayClosetItems = ItemsManager.GetPossibleFloorplanItems(floorplan);
                int hallwayClosetItemCount = 2;
                if (NumberUtil.ContainsBytes((int)draftedFloorplan.Category, (int)FloorCategory.Hallway))
                    hallwayClosetItemCount += 1;
                
                for (int i = 0; i < hallwayClosetItemCount; i++)
                    floorplan.AddItemToFloorplan(hallwayClosetItems.PickRandom());
                break;
            case "Cloister":
                RarityPicker<Item> cloisterItems = ItemsManager.GetPossibleFloorplanItems(floorplan);
                cloisterItems.ChangeRarities(1,0,0,0);
                floorplan.AddItemToFloorplan(cloisterItems.PickRandom());
                break;
            case "Terrace":
                RarityPicker<Item> terraceItems = ItemsManager.GetPossibleFloorplanItems(floorplan);
                terraceItems.ChangeRarities(0,1,0,0);
                floorplan.AddItemToFloorplan(terraceItems.PickRandom());
                break;
            case "Utility Closet":
                break;
                //power all current black rooms
                foreach (var room in GameManager.floorplanDict.Values)
                {
                    if(!NumberUtil.ContainsBytes((int)room.Category, (int)FloorCategory.BlackRooms)) continue;
                    room.pointMult.Add(() => 2);
                }
                //power all following black rooms
                GameEvent.onDraftedFloorplan += UtilityClosetEffect;
                void UtilityClosetEffect(FloorplanEvent subEvt)
                {
                    if(!NumberUtil.ContainsBytes((int)subEvt.Floorplan.Category, (int)FloorCategory.BlackRooms)) return;
                    subEvt.Floorplan.pointMult.Add(() => 2);
                }
                break;
            case "Boiler Room":
                GameEvent.onConnectFloorplans += BoilerRoomEffect;
                void BoilerRoomEffect(FloorplanConnectedEvent subEvt)
                {
                    if (!floorplan.ConnectedToFloorplan(subEvt, out var other)) return;
                    //connected rooms are powered
                    Debug.Log($"power {other.Name}");
                    other.pointMult.Add(() => 2);
                }
                break;
            case "Kitchen":
                PurchaseData apple = new()
                {
                    cost = 2,
                    amount = 6,
                    name = "Apple",
                    description = "Gain +2 steps",
                    OnBuy = () => new Food(2).Initialize()
                };
                PurchaseData banana = new()
                {
                    cost = 3,
                    amount = 5,
                    name = "Banana",
                    description = "Gain +3 steps",
                    OnBuy = () => new Food(3).Initialize()
                };
                PurchaseData orange = new()
                {
                    cost = 5,
                    amount = 3,
                    name = "Orange",
                    description = "Gain +5 steps",
                    OnBuy = () => new Food(5).Initialize()
                };

                List<PurchaseData> kitchenList = new () { apple, banana, orange };
                bool enteredKitchen = false;
                GameEvent.OnEnterFloorplan += OnEnterKitchen;
                GameEvent.OnExitFloorplan += OnExitShop;
                void OnEnterKitchen(FloorplanEvent subEvt)
                {
                    if (evt.Floorplan != subEvt.Floorplan) return;
                    if (!enteredKitchen)
                    {
                        enteredKitchen = true;
                        ShopWindow.OpenShop("Kitchen", kitchenList);
                        return;
                    }
                    ShopWindow.SetupShop("Kitchen", kitchenList);
                }
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

                floorplan.pointBonus.Add(() => bonusPoints);
                List<PurchaseData> giftList = new () { one, three, five, ten };
                bool enteredGiftShop = false;
                GameEvent.OnEnterFloorplan += OnEnterGiftShop;
                GameEvent.OnExitFloorplan += OnExitShop;
                void OnEnterGiftShop(FloorplanEvent subEvt)
                {
                    if (evt.Floorplan != subEvt.Floorplan) return;
                    if (!enteredGiftShop)
                    {
                        enteredGiftShop = true;
                        ShopWindow.OpenShop("Gift Shop", giftList);
                        return;
                    }
                    ShopWindow.SetupShop("Gift Shop", giftList);
                }
                break;
            case "Commissary":
                PurchaseData bananas = new()
                {
                    cost = 4,
                    amount = 3,
                    name = "Banana",
                    description = "Gain +3 steps",
                    OnBuy = () => new Food(3).Initialize()
                };
                PurchaseData keys = new()
                {
                    cost = 5,
                    amount = 5,
                    name = "Key",
                    description = "Used to draft powerful floorplans",
                    OnBuy = () => new Key(1).Initialize()
                };
                PurchaseData dice = new()
                {
                    cost = 8,
                    amount = 2,
                    name = "Dice",
                    description = "Used to reroll drawn floorplans",
                    OnBuy = () => new Dice(1).Initialize()
                };
                PurchaseData keyBundle = new()
                {
                    cost = 12,
                    amount = 1,
                    name = "Key bundle",
                    description = "Used to draft powerful floorplans, now in a neat package",
                    OnBuy = () => new Key(3).Initialize()
                };

                List<PurchaseData> commissaryList = new() { bananas, keys, dice, keyBundle };
                bool enteredCommissary = false;
                GameEvent.OnEnterFloorplan += OnEnterCommissary;
                GameEvent.OnExitFloorplan += OnExitShop;
                void OnEnterCommissary(FloorplanEvent subEvt)
                {
                    if (evt.Floorplan != subEvt.Floorplan) return;
                    if (!enteredCommissary)
                    {
                        enteredCommissary = true;
                        ShopWindow.OpenShop("Commissary", commissaryList);
                        return;
                    }
                    ShopWindow.SetupShop("Commissary", commissaryList);
                }
                break;
            case "Cassino":
                GameEvent.OnEnterFloorplan += OnEnterCassino;
                void OnEnterCassino(FloorplanEvent subEvt)
                {
                    if (evt.Floorplan != subEvt.Floorplan) return;
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
                    GameEvent.OnEnterFloorplan -= OnEnterCassino;
                }
                break;
            case "Vault":
                break;
                int lastRoomCount = 0;
                GameEvent.OnEnterFloorplan += OnEnterVault;
                void OnEnterVault(FloorplanEvent subEvt)
                {
                    if (evt.Floorplan != subEvt.Floorplan) return;
                    int coinAmount = GameManager.floorplanDict.Count - lastRoomCount;
                    if (coinAmount <= 0) return;
                    lastRoomCount = GameManager.floorplanDict.Count;
                    new Coin(coinAmount).Initialize();
                }
                break;
            case "Dining Room":
                int eatenFood = 0;
                floorplan.AddItemToFloorplan(new Food(10));
                floorplan.pointBonus.Add(() => eatenFood);
                GameEvent.OnCollectItem += OnCollectFood;
                void OnCollectFood(ItemEvent subEvt)
                {
                    if(subEvt.item is not Food) return;
                    eatenFood++;
                }
                break;
            case "Gallery":
                int visits = 0;
                floorplan.pointBonus.Add(() => visits);
                GameEvent.OnEnterFloorplan += OnEnterGallery;
                void OnEnterGallery(FloorplanEvent subEvt)
                {
                    if (evt.Floorplan != subEvt.Floorplan) return;
                    if(Player.coins <= 0) return;
                    Player.ChangeCoins(-1);
                    visits++;
                }
                break;
            case "Pump Room":
                GameEvent.onConnectFloorplans += PumpRoomEffect;
                void PumpRoomEffect(FloorplanConnectedEvent subEvt)
                {
                    if (!floorplan.ConnectedToFloorplan(subEvt, out var other)) return;
                    //connected bedrooms gain extra points
                    other.pointBonus.Add(floorplan.CalculatePoints);
                }
                break;
            case "":
                break;
        }
        void OnExitShop(FloorplanEvent subEvt)
        {
            if (evt.Floorplan != subEvt.Floorplan) return;
            ShopWindow.CloseShop();
        }
    }

    public static void AddFloorplanEffect(Floorplan floorplan)
    {
        switch (floorplan.Name)
        {
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
                floorplan.TheFirstTime().//has to be everytime; but need to fix stackOverflow
                    FloorplanConnected().Do(evt =>
                        Helpers.ConnectFloorplans(evt.baseFloorplan, evt.connectedFloorplan));
                break;
            case "Den":
                floorplan.TheFirstTime().FloorplanIsDrafted().AddItemToFloorplan(new Key(1));
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
            case "Vault":
                int lastRoomCount = 0;
                floorplan.EveryTime().PlayerEnterFloorplan().Do(_ =>
                {
                    int coinAmount = GameManager.floorplanDict.Count - lastRoomCount;
                    if (coinAmount <= 0) return;
                    new Coin(coinAmount).Initialize();
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
            case "Utility Closet":
                //power all rooms of the same category
                foreach (var room in GameManager.floorplanDict.Values)
                {
                    if (!NumberUtil.ContainsAnyBits((int)room.Category, (int)floorplan.Category)) continue;
                    Debug.Log($"Powering {room.Name}");
                    room.pointMult.Add(() => 2);
                }
                //power all rooms of the same category
                floorplan.EveryTime().AnyFloorplanIsDrafted().
                    Where(IsNot(floorplan), MatchCategoryWith(floorplan)).
                    PowerThatFloorplan();
                break;
        }
    }

    public static Func<FloorplanEvent, bool> IsOfCategory(FloorCategory type) =>
        evt => NumberUtil.ContainsBytes((int)evt.Floorplan.Category, (int)type);
    public static Func<FloorplanEvent, bool> MatchCategoryWith(Floorplan floorplan) =>
        evt => NumberUtil.ContainsAnyBits((int)evt.Floorplan.Category, (int)floorplan.Category);
    public static Func<FloorplanEvent, bool> IsNot(Floorplan floorplan) =>
        evt => evt.Floorplan != floorplan;
}