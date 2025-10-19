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

    public event Action<Room> onPickedFloorplan;

    public Room currentRoom;

    public FloorplanUI FloorplanUI => floorplanUI;

    private void Awake()
    {
        button.onClick.AddListener(FloorplanPick);
    }

    private void FloorplanPick()
    {
        onPickedFloorplan?.Invoke(currentRoom);
    }

    public void Setup(Room room)
    {
        if (currentRoom != null)
            currentRoom.OnChanged -= InternalSetup;
        currentRoom = room;
        currentRoom.OnChanged += InternalSetup;
        InternalSetup();
    }

    private void InternalSetup()
    {
        floorplanUI.Setup(currentRoom);
        int currentPoints = currentRoom.CalculatePoints();
        points.text = (currentPoints > 0 ? "+" : string.Empty) + $"{currentPoints}";
        cost.gameObject.SetActive(currentRoom.keyCost > 0);
        cost.text = currentRoom.keyCost.ToString();
        description.text = currentRoom.Description;
    }
}
