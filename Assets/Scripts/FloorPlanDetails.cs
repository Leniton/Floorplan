using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloorplanDetails : MonoBehaviour
{
    [SerializeField] private FloorplanUI floorplanUI;
    [SerializeField] private TMP_Text points;
    [SerializeField] private TMP_Text cost;
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
        int currentPoints = this.floorplan.CalculatePoints();
        points.text = currentPoints != 0 ? $"+{currentPoints}" : string.Empty;
        cost.gameObject.SetActive(this.floorplan.keyCost > 0);
        cost.text = this.floorplan.keyCost.ToString();
        description.text = floorplan.Description;
    }
}
