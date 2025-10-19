using System;
using System.Collections;
using System.Collections.Generic;
using Lenix.NumberUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloorplanUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Image pattern;
    [SerializeField] private TMP_Text text;
    [SerializeField] private RectTransform[] entrances;
    [SerializeField] private Transform colorsContainer;
    
    [Header("External References")]
    [SerializeField] private FloorplanColorMask colorMaskPrefab;

    private Room currentRoom;

    private List<FloorplanColorMask> colors = new();

    private Vector2 entranceDimensions;

    private void Awake()
    {
        entranceDimensions = entrances[0].sizeDelta;
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
        image.enabled = !ReferenceEquals(currentRoom, null);
        if (ReferenceEquals(currentRoom, null)) return;
        text.text = currentRoom.Name;

        for (int i = 0; i < entrances.Length; i++)
            entrances[i].gameObject.SetActive(currentRoom.connections[i]);
        SetupColors();
        pattern.gameObject.SetActive(currentRoom.renovation is not null and { overlayPattern: not null });
        pattern.sprite = currentRoom.renovation?.overlayPattern;
    }

    private void SetupColors()
    {
        image.color = default;
        int[] categories = NumberUtil.SeparateBits((int)currentRoom.Category);
        colors.EnsureEnoughInstances(colorMaskPrefab, categories.Length, colorsContainer);
        if (categories is { Length: <= 0 }) categories = new[] { (int)RoomCategory.Blank };
        float fillSlice = 1f / categories.Length;
        for (int i = categories.Length - 1; i > 0; i--)
        {
            var currentColor = colors[i];
            currentColor.SetColor(GameSettings.current.roomColors.GetColor((RoomCategory)categories[i]));
            currentColor.SetFillAmount(fillSlice * (categories.Length - i));
        }

        var lastColor = colors[0];
        lastColor.SetColor(GameSettings.current.roomColors.GetColor((RoomCategory)categories[0]));
        lastColor.SetFillAmount(1);
    }

    public void HighlightDirection(Vector2Int direction)
    {
        int id = Room.DirectionToID(direction);
        for (int i = 0; i < entrances.Length; i++)
        {
            float scale = entranceDimensions.x * (i == id ? .8f : 0);
            entrances[i].sizeDelta = entranceDimensions + (Vector2.right * scale);
        }
    }
}
