using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CircleLayoutGroup : LenixSOLayoutGroup
{
    [Space(10), Tooltip("Will the final object overlap with the first one?")] 
    public bool overlapEnd = false;

    [Range(0, 1)] public float offset;
    public Rotation rotation = Rotation.Clockwise;
    public float startPadding = 0;
    public float endPadding = 0;

    [Header("Rotate Elements")]
    [SerializeField] private bool rotateElements;
    public Rotation elementRotation = Rotation.Clockwise;
    [SerializeField, Range(0, 360)] private float rotationOffset;

    public override void AdjustElements()
    {
        RectTransform[] childs = GetEnabledElements();
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
            pos.x = x * spacing;
            pos.y = y * spacing;
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