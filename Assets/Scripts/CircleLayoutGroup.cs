using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CircleLayoutGroup : MonoBehaviour
{
    [Tooltip("If true this layout group will update its positions every frame")] public bool isStatic = false;
    [Tooltip("Will the final object overlap with the first one?")] public bool overlapEnd = false;

    [Space, Range(0, 1)] public float offset;
    public Rotation rotation = Rotation.Clockwise;
    public float radius = 50;
    public float startPadding = 0;
    public float endPadding = 0;

    [Header("Rotate Elements")]
    [SerializeField] private bool rotateElements;
    public Rotation elementRotation = Rotation.Clockwise;
    [SerializeField, Range(0, 360)] private float rotationOffset;

    [SerializeField] private List<RectTransform> overrideElements;

    private void OnEnable()
    {
        AdjustElements();
    }

    private void Update()
    {
        if (isStatic && Application.isPlaying) return; //Check for application playing to ignore it when in editor
        AdjustElements();
    }

    public void AdjustElements()
    {
        List<RectTransform> enabledChilds = new();
        if (overrideElements is { Count: <= 0 })
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform rectTransform = (RectTransform)transform.GetChild(i);
                if (rectTransform.gameObject.activeSelf) enabledChilds.Add(rectTransform);
            }
        }
        else
        {
            for (int i = 0; i < overrideElements.Count; i++)
            {
                RectTransform rectTransform = overrideElements[i];
                if (rectTransform.gameObject.activeSelf) enabledChilds.Add(rectTransform);
            }
        }
        RectTransform[] childs = enabledChilds.ToArray();
        int elementPositions = overlapEnd ? 1 : 0;
        float progression = Mathf.Clamp01((1 - endPadding) - startPadding) / (childs.Length - elementPositions);
        float currentP = offset + startPadding;
        int order = (int)rotation;
        int rotationOrder = (int)elementRotation;
        for (int i = 0; i < childs.Length; i++)
        {
            float proportion = currentP % 1;
            float x = Mathf.Sin(2 * Mathf.PI * proportion * order);
            float y = Mathf.Cos(2 * Mathf.PI * proportion * order);

            Vector2 pos = childs[i].anchoredPosition;
            pos.x = x * radius;
            pos.y = y * radius;
            childs[i].anchoredPosition = pos;

            Vector3 rotation = childs[i].eulerAngles;
            rotation.z = rotateElements ? ((360 * rotationOrder) * proportion) + rotationOffset : 0;
            childs[i].localEulerAngles = rotation;

            currentP += progression;
        }
    }
}

public enum Rotation
{
    Clockwise = 1,
    CounterClockwise = -1
}