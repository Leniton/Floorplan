using UnityEngine;

[ExecuteAlways]
public class ScaleWithParent : MonoBehaviour
{
    [SerializeField] private bool invert;
    [SerializeField] private float minScale = 0.01f, maxScale = 99999;
    [SerializeField] private RectTransform parent;
    [SerializeField] private Vector2 baseSize;

    private void Update()
    {
        if(!parent) return;
        float proportion = parent.rect.size.x / baseSize.x;

        if (invert) proportion = 1 / proportion;

        proportion = Mathf.Clamp(proportion, minScale, maxScale);
        
        transform.localScale = Vector2.one * proportion;
    }
}