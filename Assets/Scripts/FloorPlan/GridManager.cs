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
    private GameObject[] slots;
    private RectTransform rectTransform;

    private Coroutine moveCoroutine;

    public Vector2Int currentPosition => coordinate;
    private bool moving => moveCoroutine != null;

    public event Action<Vector2Int, Vector2Int> OnStartMove;
    public event Action<Vector2Int> OnMove;

    public static GridManager instance;

    protected override void Awake()
    {
        grid ??= gameObject.GetComponent<GridLayoutGroup>();
        rectTransform = (RectTransform)grid.transform;
        Addressables.LoadAssetAsync<GameObject>(prefabPath).Completed += operation =>
        {
            slotPrefab = operation.Result;
            for (int i = 0; i < slots.Length; i++)
                slots[i] = Instantiate(slotPrefab, transform);
            onDoneLoading?.Invoke();
        };
        slots = new GameObject[xSize * ySize];

        coordinate = new((xSize / 2), 0);
        UpdatePosition(false);
    }

    public void ShiftSelection(Vector2Int direction)
    {
        Vector2Int newCoordinate = coordinate + direction;
        if (!ValidCoordinate(newCoordinate)) return;
        lastCoordinate = coordinate;
        coordinate = newCoordinate;
        //print($"{lastCoordinate} shift {direction} to {coordinate}");
        UpdatePosition();
    }

    public bool ValidCoordinate(Vector2Int coordinate)
    {
        return !(coordinate.x < 0 || coordinate.x >= xSize ||
            coordinate.y < 0 || coordinate.y >= ySize);
    }

    private void UpdatePosition(bool animate = true)
    {
        //print($"coordinate: {coordinate}");
        if (animate ^ moving)
            moveCoroutine = StartCoroutine(MoveToPosition());
        else if (!moving)
        {
            rectTransform.anchoredPosition = GetCoordinatePosition(coordinate);
            OnMove?.Invoke(coordinate);
        }
    }

    public Vector2 GetCoordinatePosition(Vector2Int coordinate)
    {
        Vector2 position;
        position.x = (grid.cellSize.x + grid.spacing.x) * ((xSize / 2) - coordinate.x);
        position.y = (grid.cellSize.y + grid.spacing.y) * -coordinate.y;
        return position;
    }

    public RectTransform GetSlot(Vector2Int coordinate)
    {
        return (RectTransform)slots[coordinate.x + (xSize * coordinate.y)].transform;
    }

    private IEnumerator MoveToPosition()
    {
        const float moveDuration = .1f;
        Vector2Int targetCoordinate = Vector2Int.one * -1;
        while (targetCoordinate != coordinate)
        {
            targetCoordinate = coordinate;
            OnStartMove?.Invoke(lastCoordinate, targetCoordinate);
            Vector2 startPosition = rectTransform.anchoredPosition;
            Vector2 targetPosition = GetCoordinatePosition(targetCoordinate);
            float time = 0;
            while (time < moveDuration)
            {
                float scaledTime = time / moveDuration;
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, scaledTime);
                yield return null;
                time += Time.deltaTime;
            }
            rectTransform.anchoredPosition = targetPosition;
            OnMove?.Invoke(targetCoordinate);
        }
        moveCoroutine = null;
    }
}
