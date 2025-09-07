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
    [SerializeField] private GameObject[] entrances;
    [SerializeField] private Transform colorsContainer;
    
    [Header("External References")]
    [SerializeField] private FloorplanColorMask colorMaskPrefab;

    private Floorplan currentFloorplan;

    private List<FloorplanColorMask> colors = new();

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
        image.enabled = !ReferenceEquals(currentFloorplan, null);
        text.text = currentFloorplan.Name;

        for (int i = 0; i < entrances.Length; i++)
            entrances[i].SetActive(currentFloorplan.connections[i]);
        SetupColors();
        pattern.gameObject.SetActive(currentFloorplan.renovation is not null and { overlayPattern: not null });
        if (!pattern.gameObject.activeSelf) return;
        pattern.sprite = currentFloorplan.renovation.overlayPattern;
    }

    private void SetupColors()
    {
        image.color = default;
        int[] categories = NumberUtil.SeparateBits((int)currentFloorplan.Category);
        colors.EnsureEnoughInstances(colorMaskPrefab, categories.Length, colorsContainer);
        float fillSlice = 1f / categories.Length;
        for (int i = categories.Length - 1; i > 0; i--)
        {
            var currentColor = colors[i];
            currentColor.SetColor(GameSettings.current.floorplanColors.GetColor((FloorCategory)categories[i]));
            currentColor.SetFillAmount(fillSlice * (categories.Length - i));
        }

        var lastColor = colors[0];
        lastColor.SetColor(GameSettings.current.floorplanColors.GetColor((FloorCategory)categories[0]));
        lastColor.SetFillAmount(1);
    }
}
