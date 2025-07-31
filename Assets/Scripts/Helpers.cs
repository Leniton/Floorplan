using System;
using System.Collections.Generic;

public static class Helpers
{
    public static bool ConnectedToFloorplan(this Floorplan targetFloorplan, FloorplanConnectedEvent evt, out Floorplan other)
    {
        other = null;
        if(evt.baseFloorplan != targetFloorplan && evt.connectedFloorplan != targetFloorplan) return false;
        other = evt.baseFloorplan == targetFloorplan ? evt.connectedFloorplan : evt.baseFloorplan;
        return true;
    }
    
    public static void AddItemToFloorplan(this Floorplan floorplan, Item item)
    {
        GameEvent.OnEnterFloorplan += OnEnterFloorplan;
        void OnEnterFloorplan(FloorplanEvent evt)
        {
            if (evt.Floorplan != floorplan) return;
            item?.Initialize();
            GameEvent.OnEnterFloorplan -= OnEnterFloorplan;
        }
    }

    #region EffectCreation
    public static Effect TheFirstTime(this Floorplan floorplan) => new (floorplan, 1);
    public static Effect EveryTime(this Floorplan floorplan) => new (floorplan);
    public static Effect TheNext_Times(this Floorplan floorplan, uint times) => new(floorplan, (int)times);
    #endregion

    #region EventListeners
    //Floorplan
    public static EventListener<Action<CoordinateEvent>> FloorplanIsDrafted(this Effect effect) => new(effect, 
        (a) => effect.floorplan.onDrafted += a,
        (a) => effect.floorplan.onDrafted -= a);
    public static EventListener<Action<FloorplanConnectedEvent>> FloorplanConnected(this Effect effect) => new(effect,
        (a) => effect.floorplan.onConnectToFloorplan += a,
        (a) => effect.floorplan.onConnectToFloorplan -= a);
    public static EventListener<Action<Event>> PlayerEnterFloorplan(this Effect effect) => new(effect, 
        (a) => effect.floorplan.onEnter += a,
        (a) => effect.floorplan.onEnter -= a);
    public static EventListener<Action<Event>> PlayerExitFloorplan(this Effect effect) => new(effect, 
        (a) => effect.floorplan.onExit += a,
        (a) => effect.floorplan.onExit -= a);
    
    //GameEvent
    public static EventListener<Action<FloorplanEvent>> AnyFloorplanIsDrafted(this Effect effect) => new(effect, 
            (a) => GameEvent.onDraftedFloorplan += a,
            (a) => GameEvent.onDraftedFloorplan -= a);
    public static EventListener<Action<FloorplanConnectedEvent>> AnyFloorplanConnected(this Effect effect) => new(effect,
        (a) => GameEvent.onConnectFloorplans += a,
        (a) => GameEvent.onConnectFloorplans -= a);
    public static EventListener<Action<FloorplanEvent>> PlayerEnterAnyFloorplan(this Effect effect) => new (effect, 
        (a) => GameEvent.OnEnterFloorplan += a,
        (a) => GameEvent.OnEnterFloorplan -= a);
    public static EventListener<Action<FloorplanEvent>> PlayerExitAnyFloorplan(this Effect effect) => new (effect, 
        (a) => GameEvent.OnExitFloorplan += a,
        (a) => GameEvent.OnExitFloorplan -= a);

    #endregion

    #region Effects
    public static void Do<T>(this EventListener<Action<T>> listener, Action<T> action) where T : Event
    {
        listener.AddAction(DoAction);
        void DoAction(T evt)
        {
            bool hasMoreUses = listener.effect.TryUse(out var canUse);
            if (!canUse) return;
            action?.Invoke(evt);
            if (hasMoreUses) return;
            listener.RemoveAction(DoAction);
        }
    }
    //Player changes
    public static void ChangePlayerSteps<T>(this EventListener<Action<T>> listener, int amount) where T : Event =>
        listener.Do(_ => Player.ChangeSteps(amount));
    public static void ChangePlayerCoins<T>(this EventListener<Action<T>> listener, int amount) where T : Event =>
        listener.Do(_ => Player.ChangeCoins(amount));
    public static void ChangePlayerKeys<T>(this EventListener<Action<T>> listener, int amount) where T : Event =>
        listener.Do(_ => Player.ChangeKeys(amount));

    //Floorplan changes
    public static void AddItemToFloorplan<T>(this EventListener<Action<T>> listener, Item item) where T : Event =>
        listener.Do(_ => listener.effect.floorplan.AddItemToFloorplan(item));
    public static void AddItemToThatFloorplan<T>(this EventListener<Action<T>> listener, Item item) where T : CoordinateEvent =>
        listener.Do(evt => GameManager.floorplanDict[evt.Coordinates].AddItemToFloorplan(item));
    public static void AddPointsToFloorplan<T>(this EventListener<Action<T>> listener, int amount) where T : Event =>
        listener.Do(_ => listener.effect.floorplan.pointBonus.Add(() => amount));
    public static void AddPointBonusToFloorplan<T>(this EventListener<Action<T>> listener, Func<int> amount) where T : Event =>
        listener.Do(_ => listener.effect.floorplan.pointBonus.Add(amount));
    public static void PowerFloorplan<T>(this EventListener<Action<T>> listener) where T : Event =>
        listener.Do(_ => listener.effect.floorplan.pointMult.Add(() => 2));
    public static void SetupFloorplanShop<T>(this EventListener<Action<T>> listener, string title, List<PurchaseData> shopList) 
        where T : CoordinateEvent
    {
        listener.Do(CreateShop);
        void CreateShop(CoordinateEvent evt)
        {
            Floorplan floorplan = GameManager.floorplanDict[evt.Coordinates];
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
}
