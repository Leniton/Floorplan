using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
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

    public int currentValue { get; private set; }

    [SerializeMethod]
    public void ChangeValue(int delta) => ChangeValueSequence(delta).Begin();

    public ISequence ChangeValueSequence(int delta,
        float? customDuration = null, AnimationCurve customCurve = null)
    {
        ISequence sequence = CustomSequence.EmptySequence();
        var wrapper = CustomSequence.EmptySequence();
        wrapper = new CustomSequence(BeginWrapper, sequence.End);
        sequence.OnFinished += wrapper.FinishSequence;
        return wrapper;

        void BeginWrapper()
        {
            sequence = CountAnimationSequence(currentValue + delta,
                customCurve ?? curve, customDuration ?? animationDuration);
            sequence.OnFinished += wrapper.FinishSequence;
            sequence.Begin();
        }
    }

    public void ChangeValue(int delta, float customDuration, AnimationCurve customCurve) =>
        ChangeValueSequence(delta, customDuration, customCurve).Begin();

    [SerializeMethod]
    public void ChangeToValue(int value) => ChangeToValueSequence(value).Begin();
    
    public ISequence ChangeToValueSequence(int value,
        float? customDuration = null, AnimationCurve customCurve = null) => 
        CountAnimationSequence(value, customCurve ?? curve, customDuration ?? animationDuration);

    public void ChangeToValue(int value, float customDuration, AnimationCurve customCurve) =>
        ChangeToValueSequence(value, customDuration, customCurve).Begin();

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
