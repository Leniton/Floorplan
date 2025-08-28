using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloorplanColorMask : MonoBehaviour
{
    [SerializeField] private Image mask;
    [SerializeField] private Image colorImage;

    public void SetColor(Color color)
    {
        colorImage.color = color;
    }

    public void SetFillAmount(float amount)
    {
        mask.fillAmount = amount;
    }
    
    private void Reset()
    {
        mask = GetComponent<Image>();
        colorImage = transform.GetChild(0)?.GetComponent<Image>();
    }
}
