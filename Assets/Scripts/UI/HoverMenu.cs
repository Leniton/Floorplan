using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverMenu : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private CircleLayoutGroup layoutGroup;
    [SerializeField] private float expandRadius;

    bool expand;
    private Coroutine moveCoroutine;
    private bool moving => moveCoroutine != null;

    public void OnPointerDown(PointerEventData eventData)
    {
        expand = true;
        if (!moving)
            moveCoroutine = StartCoroutine(MoveAnimation());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        expand = false;
        if (!moving)
            moveCoroutine = StartCoroutine(MoveAnimation());
    }

    private IEnumerator MoveAnimation()
    {
        const float duration = .08f;

        bool expandLayout = !expand;

        while (expandLayout != expand)
        {
            expandLayout = expand;
            float goal = expandLayout ? expandRadius : 0;
            float origin = Mathf.Abs(goal - expandRadius);
            float time = 0;
            while (time < duration)
            {
                float scaledTime = time / duration;
                layoutGroup.radius = Mathf.Lerp(origin, goal, scaledTime);
                layoutGroup.AdjustElements();
                yield return null;
                time += Time.deltaTime;
            }
            layoutGroup.radius = goal;
            layoutGroup.AdjustElements();
        }

        moveCoroutine = null;
    }
}