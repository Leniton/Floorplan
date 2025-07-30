using System;
using System.Collections.Generic;
using UnityEngine;

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
        void OnEnterFloorplan(GenericFloorplanEvent evt)
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
    public static EventListener<Action<GenericFloorplanEvent>> AnyFloorplanIsDrafted(this Effect effect) => new(effect, 
            (a) => GameEvent.onDraftedFloorplan += a,
            (a) => GameEvent.onDraftedFloorplan -= a);
    public static EventListener<Action<FloorplanConnectedEvent>> AnyFloorplanConnected(this Effect effect) => new(effect,
        (a) => GameEvent.onConnectFloorplans += a,
        (a) => GameEvent.onConnectFloorplans -= a);
    public static EventListener<Action<GenericFloorplanEvent>> PlayerEnterAnyFloorplan(this Effect effect) => new (effect, 
        (a) => GameEvent.OnEnterFloorplan += a,
        (a) => GameEvent.OnEnterFloorplan -= a);
    public static EventListener<Action<GenericFloorplanEvent>> PlayerExitAnyFloorplan(this Effect effect) => new (effect, 
        (a) => GameEvent.OnExitFloorplan += a,
        (a) => GameEvent.OnExitFloorplan -= a);

    #endregion

    #region Effects
    public static void AddItemToFloorplan<T>(this EventListener<Action<T>> listener, Item item) where T: Event
    {
        listener.AddAction(AddItem);
        void AddItem(Event evt)
        {
            bool hasMoreUses = listener.effect.TryUse(out var canUse);
            if (!canUse) return;
            listener.effect.floorplan.AddItemToFloorplan(item);
            if (hasMoreUses) return;
            listener.RemoveAction(AddItem);
        }
    }
    public static void AddItemToThatFloorplan<T>(this EventListener<Action<T>> listener, Item item) where T : CoordinateEvent
    {
        listener.AddAction(AddItem);

        void AddItem(CoordinateEvent evt)
        {
            bool hasMoreUses = listener.effect.TryUse(out var canUse);
            if (!canUse) return;
            GameManager.floorplanDict[evt.Coordinates].AddItemToFloorplan(item);
            if (hasMoreUses) return;
            listener.RemoveAction(AddItem);
        }
    }
    public static void ChangePlayerSteps<T>(this EventListener<Action<T>> listener, int amount) where T : Event
    {
        listener.AddAction(AddSteps);
        void AddSteps(Event evt)
        {
            bool hasMoreUses = listener.effect.TryUse(out var canUse);
            if (!canUse) return;
            Player.ChangeSteps(amount);
            if (hasMoreUses) return;
            listener.RemoveAction(AddSteps);
        }
    }
    #endregion
}
