using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloorplanDetails : MonoBehaviour
{
    [SerializeField] private FloorplanUI floorplanUI;
    [SerializeField] private TMP_Text points;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Button button;

    public event Action<Floorplan> onPickedFloorplan;

    public Floorplan floorplan;

    private void Awake()
    {
        button.onClick.AddListener(FloorplanPick);
    }

    private void FloorplanPick()
    {
        onPickedFloorplan?.Invoke(floorplan);
    }

    public void Setup(Floorplan floorplan)
    {
        this.floorplan = floorplan;
        floorplanUI.Setup(floorplan);
        points.text = floorplan.basePoints != 0 ? $"+{floorplan.basePoints}" : string.Empty;
        description.text = floorplan.Description;
    }
}
