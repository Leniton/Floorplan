using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloorplanUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text text;

    public void Setup(Floorplan floorplan)
    {
        image.color = floorplan.Color;
        text.text = floorplan.Name;
    }
}
