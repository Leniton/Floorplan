using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraftManager : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject draftScreen;
    [SerializeField] private FloorplanDetails floorplanUI;

    [SerializeField] Floorplan floorplan;

    private void Awake()
    {
        DraftFloorplan();
    }

    public void DraftFloorplan()
    {
        background.SetActive(true);
        draftScreen.SetActive(true);
        floorplanUI.Setup(floorplan);
    }
}
