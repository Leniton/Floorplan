using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class HoverOptions : MonoBehaviour
{
    [SerializeField] private LenixSOLayoutGroup layoutGroup;
    [SerializeField] private HoverButton prefab;
    public int maxElements;
    public RectTransform rectTransform { get; private set; }
    public LenixSOLayoutGroup LayoutGroup => layoutGroup;
    
    //visual
    [HideInInspector] public List<HoverButton> optionsButton = new();
    
    //move animation
    private float openOffset;
    private float openSpacing;
    public bool expand { get; private set; } = true;
    private Coroutine moveCoroutine;
    private bool moving => moveCoroutine != null;

    public event Action OnDoneMoving;

    private void Awake()
    {
        rectTransform = layoutGroup.GetComponent<RectTransform>();
        openOffset = layoutGroup.offset;
        openSpacing = layoutGroup.spacing;
        if (ReferenceEquals(prefab, null))
            Addressables.LoadAssetAsync<GameObject>("HoverButton").Completed +=
                (o) => prefab = o.Result.GetComponent<HoverButton>();
    }

    public void SetupOptions(List<ButtonCallback> options)
    {
        int optionCount = Mathf.Min(options.Count, maxElements);

        if (optionsButton.Count < optionCount)
        {
            //create enough instances
            int difference = optionCount - optionsButton.Count;
            for (int i = 0; i < difference; i++)
            {
                var instance = Instantiate(prefab, layoutGroup.transform);
                optionsButton.Add(instance);
                layoutGroup.overrideElements.Add(instance.transform as RectTransform);
            }
        }

        for (int i = 0; i < optionsButton.Count; i++)
        {
            if (i >= optionCount)
            {
                optionsButton[i].gameObject.SetActive(false);
                continue;
            }
            optionsButton[i].gameObject.SetActive(true);
            if(options[i] != null) optionsButton[i].SetOption(options[i]);
        }
        layoutGroup.AdjustElements();
    }

    public void ChangeOptionsVisibility(bool value)
    {
        if (expand == value) return;
        expand = value;
        if (!moving)
            moveCoroutine = StartCoroutine(MoveAnimation());
    }

    public IEnumerator MoveAnimation()
    {
        const float duration = .08f;

        bool expandLayout = !expand;
        while (expandLayout != expand)
        {
            expandLayout = expand;
            float targetSpacing = expandLayout ? openSpacing : 0;
            float initialSpacing = Mathf.Abs(targetSpacing - openSpacing);
            float targetOffset = expandLayout ? openOffset : 0;
            float initialOffset = Mathf.Abs(targetOffset - openOffset);
            float time = 0;
            while (time < duration)
            {
                float scaledTime = time / duration;
                layoutGroup.offset = Mathf.Lerp(initialOffset, targetOffset, scaledTime);
                layoutGroup.spacing = Mathf.Lerp(initialSpacing, targetSpacing, scaledTime);
                layoutGroup.AdjustElements();
                yield return null;
                time += Time.deltaTime;
            }
            layoutGroup.offset = targetOffset;
            layoutGroup.spacing = targetSpacing;
            layoutGroup.AdjustElements();
        }
        OnDoneMoving?.Invoke();
        moveCoroutine = null;
    }

    public void Hide()
    {
        expand = false;
        if(moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = null;
        layoutGroup.offset = 0;
        layoutGroup.spacing = 0;
        for (int i = 0; i < optionsButton.Count; i++)
            optionsButton[i].gameObject.SetActive(false);
        layoutGroup.AdjustElements();
    }
}
