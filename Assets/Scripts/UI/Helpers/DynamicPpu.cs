using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways,RequireComponent (typeof(Image))]
public class DynamicPpu : MonoBehaviour
{
    [SerializeField] private Vector2 targetSize;
    [SerializeField, Min(.01f)] private float targetPpu = 1;
    [SerializeField, Range(0, 1)] private float widthToHeightRatio = .5f;

    private Image _image;

    private Image image 
    {  
        get
        {
            if (ReferenceEquals(_image, null)) _image = GetComponent<Image>();
            return _image; 
        } 
    }

    private void Update()
    {
        Vector2 currentSize = image.rectTransform.rect.size;
        float widthRate = (currentSize.x / targetSize.x) * widthToHeightRatio;
        float heightRate = (currentSize.y / targetSize.y) * (1 - widthToHeightRatio);

        image.pixelsPerUnitMultiplier = targetPpu / (widthRate + heightRate);
    }

    private void Reset()
    {
        _image = GetComponent<Image>();
        targetPpu = _image.pixelsPerUnitMultiplier;
        targetSize = GetComponent<RectTransform>().sizeDelta;
    }
}
