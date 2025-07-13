using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    private const int xSize = 5;
    private const int ySize = 5;

    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GridLayoutGroup grid;

    private Vector2Int coordinate;
    private GameObject[] slots;
    private RectTransform rectTransform;

    private Coroutine moveCoroutine;
    private bool moving => moveCoroutine != null;

    public event Action<Vector2Int> OnMove;

    private void Awake()
    {
        grid ??= gameObject.GetComponent<GridLayoutGroup>();
        rectTransform = (RectTransform)grid.transform;
        slots = new GameObject[xSize * ySize];
        for (int i = 0; i < slots.Length; i++)
            slots[i] = Instantiate(slotPrefab, transform);

        coordinate = new((xSize / 2), 0);
        UpdatePosition(false);
    }

    public void ShiftSelection(Vector2Int direction)
    {
        //print($"direction {direction}");
        Vector2Int newCoordinate = coordinate + direction;
        if (newCoordinate.x < 0 || newCoordinate.x >= xSize ||
            newCoordinate.y < 0 || newCoordinate.y >= ySize) return;
        coordinate = newCoordinate;
        UpdatePosition();
    }

    private void UpdatePosition(bool animate = true)
    {
        //print($"coordinate: {coordinate}");
        if (animate ^ moving) 
            moveCoroutine = StartCoroutine(MoveToPosition());
        else
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

    private IEnumerator MoveToPosition()
    {
        const float moveDuration = .1f;
        Vector2Int targetCoordinate = Vector2Int.one * -1;
        while (targetCoordinate != coordinate)
        {
            targetCoordinate = coordinate;
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
        }
        moveCoroutine = null;
        OnMove?.Invoke(coordinate);
    }
}
