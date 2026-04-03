using System;
using System.Collections.Generic;
using UnityEngine;

public static class EffectHelper
{
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
    
    public static EventListener<Action<T>, T> Where<T>(this EventListener<Action<T>, T> listener,
        params Func<T, bool>[] check) where T : Event
    {
        for (int i = 0; i < check.Length; i++)
            listener.conditions.Add(check[i]);
        return listener;
    }
}