using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SerializableMethods;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Util.Extensions;

public class ValueCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    [FormerlySerializedAs("showSign")]
    [Header("Parameters")]
    [SerializeField] private bool showValueSign = true;
    public string prefix;
    public string sufix;
    [SerializeField] private float animationDuration = .5f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

    private int currentValue;

    [SerializeMethod]
    public void ChangeValue(int delta)
    {
        StartCoroutine(CountAnimation(currentValue + delta, curve, animationDuration));
    }

    public void ChangeValue(int delta, float customDuration, AnimationCurve customCurve)
    {
        StartCoroutine(CountAnimation(currentValue + delta, customCurve, customDuration));
    }
    
    [SerializeMethod]
    public void ChangeToValue(int value)
    {
        StartCoroutine(CountAnimation(value, curve, animationDuration));
    }

    public void ChangeToValue(int value, float customDuration, AnimationCurve customCurve)
    {
        StartCoroutine(CountAnimation(value, customCurve, customDuration));
    }

    public void UpdateText(int newValue)
    {
        StringBuilder sb = new(prefix);
        if (showValueSign && newValue > 0) sb.Append("+");
        sb.Append($"{newValue}{sufix}");
        text.SetText(sb.ToString());
    }

    public ISequence CountAnimationSequence(int value, AnimationCurve curve, float duration)
    {
        return new CoroutineSequence(new(CountAnimation(value, curve, duration), () =>
        {
            currentValue = value;
            UpdateText(currentValue);
        }), this);
    }

    private IEnumerator CountAnimation(int finalValue, AnimationCurve curve, float duration)
    {
        int current = currentValue;

        yield return ScriptAnimations.Animate(t => 
                UpdateText(Mathf.RoundToInt(Mathf.Lerp(current, finalValue, t))),
            curve, duration);

        currentValue = finalValue;
        UpdateText(currentValue);
    }

    private void Reset()
    {
        text = GetComponent<TMP_Text>();
    }
}
