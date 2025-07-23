using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FloorplanDetails : MonoBehaviour
{
    [SerializeField] private FloorplanUI floorplanUI;
    [SerializeField] private TMP_Text points;
    [SerializeField] private TMP_Text cost;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Button button;

    public event Action<Floorplan> onPickedFloorplan;

    [FormerlySerializedAs("floorplan")] public Floorplan currentFloorplan;

    private void Awake()
    {
        button.onClick.AddListener(FloorplanPick);
    }

    private void FloorplanPick()
    {
        onPickedFloorplan?.Invoke(currentFloorplan);
    }

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
        floorplanUI.Setup(currentFloorplan);
        int currentPoints = this.currentFloorplan.CalculatePoints();
        points.text = currentPoints != 0 ? $"+{currentPoints}" : string.Empty;
        cost.gameObject.SetActive(currentFloorplan.keyCost > 0);
        cost.text = currentFloorplan.keyCost.ToString();
        description.text = currentFloorplan.Description;
    }
}
