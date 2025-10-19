using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SetupComponent : MonoBehaviour
{
    public bool isLoaded {  get; protected set; }
    public Action onDoneLoading;
    protected abstract void Awake();
}
