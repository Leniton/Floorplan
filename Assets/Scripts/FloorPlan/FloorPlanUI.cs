using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloorplanUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text text;

    [SerializeField] private GameObject[] entrances;

    private Floorplan currentFloorplan;

    public void Setup(Floorplan floorplan)
    {
        if (currentFloorplan != null)
            currentFloorplan.OnChanged -= InternalSetup;
        currentFloorplan = floorplan;
        currentFloorplan.OnChanged += InternalSetup;
        InternalSetup();
    }

    private void InternalSetup()
    {
        image.color = currentFloorplan.Color;
        text.text = currentFloorplan.Name;
        text.color = ColorExtension.ContrastGray(currentFloorplan.Color);

        for (int i = 0; i < entrances.Length; i++)
            entrances[i].SetActive(currentFloorplan.connections[i]);
    }
}
