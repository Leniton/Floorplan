using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloorplanDetails : MonoBehaviour
{
    [SerializeField] private FloorplanUI floorplanUI;
    [SerializeField] private TMP_Text description;

    public void Setup(Floorplan floorplan)
    {
        floorplanUI.Setup(floorplan);
        description.text = floorplan.Description;
    }
}
