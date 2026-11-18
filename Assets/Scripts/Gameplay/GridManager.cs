using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class GridManager : SetupComponent
{
    public const int xSize = 5;
    public const int ySize = 5;

    [SerializeField] private string prefabPath;
    [SerializeField] private GridLayoutGroup grid;

    private GameObject slotPrefab;
    private Vector2Int lastCoordinate;
    private Vector2Int coordinate;
    private Button[] slots;

    private Coroutine moveCoroutine;

    public Vector2Int currentPosition => coordinate;

    public event Action<Vector2Int> onClick; 
    public event Action<Vector2Int, Vector2Int> OnStartMove;

    public static GridManager instance;

    protected override void Awake()
    {
        grid ??= gameObject.GetComponent<GridLayoutGroup>();
        slots = new Button[xSize * ySize];
        Addressables.LoadAssetAsync<GameObject>(prefabPath).Completed += operation =>
        {
            slotPrefab = operation.Result;
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = Instantiate(slotPrefab, transform).GetComponent<Button>();
                slots[i].onClick.AddListener(() => onClick?.Invoke(new(i / xSize, i % xSize)));
            }

            onDoneLoading?.Invoke();
        };

        coordinate = new((xSize / 2), 0);
        // UpdatePosition(false);
    }

    private void OnDestroy()
    {
        if (!ReferenceEquals(instance, this)) return;
        instance = null;
    }

    public void ShiftSelection(Vector2Int direction)
    {
        Vector2Int newCoordinate = coordinate + direction;
        if (!ValidCoordinate(newCoordinate)) return;
        lastCoordinate = coordinate;
        coordinate = newCoordinate;
        OnStartMove?.Invoke(lastCoordinate, newCoordinate);
    }

    public bool ValidCoordinate(Vector2Int coordinate)
    {
        return !(coordinate.x < 0 || coordinate.x >= xSize ||
            coordinate.y < 0 || coordinate.y >= ySize);
    }

    public Vector2 GetCoordinatePosition(Vector2Int coordinate)
    {
        Vector2 position;
        position.x = (grid.cellSize.x + grid.spacing.x) * ((xSize / 2) - coordinate.x);
        position.y = (grid.cellSize.y + grid.spacing.y) * -coordinate.y;
        return position;
    }

    public RectTransform GetSlotRect(Vector2Int coordinate) =>
        (RectTransform)slots[coordinate.x + (xSize * coordinate.y)].transform;
    
    public Button GetSlot(Vector2Int coordinate) => slots[coordinate.x + (xSize * coordinate.y)];

    public void SetInteractive(bool value)
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].interactable = value;
    }
}
