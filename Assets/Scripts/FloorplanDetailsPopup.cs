using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorplanDetailsPopup : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private FloorplanDetails details;

    private static FloorplanDetailsPopup instance;

    private void Awake()
    {
        instance = this;
        ClosePopup();
    }

    public static void ShowDetails(Floorplan floorplan)
    {
        instance.details.Setup(floorplan);
        instance.container.SetActive(true);
    }

    public void ClosePopup()
    {
        container.SetActive(false);
    }
}
