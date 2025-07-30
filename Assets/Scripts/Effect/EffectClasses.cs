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

public class EventListener<T> where T : Delegate
{
    private Action<T> addAction = null;
    private Action<T> removeAction = null;

    public Effect effect;

    public EventListener(Effect _effect, 
        Action<T> add = null, Action<T> remove = null)
    {
        effect = _effect;
        addAction += add;
        removeAction += remove;
    }

    public void AddAction(T action) => addAction?.Invoke(action);
    public void RemoveAction(T action) => removeAction?.Invoke(action);
}

public class Condition
{
    
}