using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect
{
    public int triggers = 0;//0 = everytime
    public int triggerCount = 0;
    public Floorplan floorplan;

    public Effect(Floorplan _floorplan, int retriggerAmount = 0)
    {
        floorplan = _floorplan;
        triggers = retriggerAmount;
    }

    public bool TryUse(out bool canUse)
    {
        canUse = triggers == 0 || triggerCount < triggers;
        triggerCount++;
        return triggers == 0 || triggerCount < triggers;
    }
}

public class EventListener<D,T2> where D : Delegate
{
    private Action<D> addAction = null;
    private Action<D> removeAction = null;

    public Effect effect;

    public List<Func<T2,bool>> conditions = new();

    public EventListener(Effect _effect, 
        Action<D> add = null, Action<D> remove = null)
    {
        effect = _effect;
        addAction += add;
        removeAction += remove;
    }

    public void AddAction(D action) => addAction?.Invoke(action);
    public void RemoveAction(D action) => removeAction?.Invoke(action);
}