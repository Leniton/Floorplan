using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public abstract class LenixSOLayoutGroup : MonoBehaviour
{
    [Tooltip("If true this layout group will update its positions every frame")] public bool isStatic = false;

    [SerializeField] protected List<RectTransform> overrideElements;

    protected virtual void OnEnable() => AdjustElements();

    protected virtual void Update()
    {
        if (isStatic && Application.isPlaying) return; //Check for application playing to ignore it when in editor
        AdjustElements();
    }

    public abstract void AdjustElements();
}
