using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollPicker : MonoBehaviour, IEndDragHandler
{
    [SerializeField] private float lockSpeed = 800;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private Scrollbar scrollbar;
    
    private ICollection options;
    private float amountPerOption;
    public int currentOption { get; private set; }
    public RectTransform content => scroll.content;

    public void SetupPicker(ICollection collection)
    {
        options = collection;
        amountPerOption = 1f / (collection.Count - 1);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        PickOption();
    }

    private void PickOption()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ScrollToOption());
            return;
        }

        float currentPosition = Mathf.Clamp01(scrollbar.value);
        currentOption = Mathf.RoundToInt(currentPosition / amountPerOption);
        scrollbar.value = amountPerOption * currentOption;
    }

    private IEnumerator ScrollToOption()
    {
        //wait until
        while (Mathf.Abs(scroll.velocity.sqrMagnitude) > Mathf.Pow(lockSpeed, 2))
            yield return null;

        float currentPosition = Mathf.Clamp01(scrollbar.value);
        currentOption = Mathf.RoundToInt(currentPosition / amountPerOption);
        float current = scrollbar.value;
        float goal = amountPerOption * currentOption;

        float duration = .1f;
        float time = 0;
        while (time < duration)
        {
            float scaledTime = time / duration;
            scrollbar.value = Mathf.Lerp(current, goal, scaledTime);
            yield return null;
            time += Time.deltaTime;
        }
        scrollbar.value = goal;
    }
}
