using System.Collections;
using SerializableMethods;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util.Extensions;

public class ValueSlider : MonoBehaviour
{
    [SerializeField] private TMP_Text totalPoints;
    [SerializeField] private Slider totalPointsSlider;

    [Header("Animation Parameters")] 
    [SerializeField] private float animationDuration = .5f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

    public int currentValue { get; private set; }

    [SerializeMethod]
    public void UpdateMaxValue(int maxValue)
    {
        totalPointsSlider.maxValue = maxValue;
        SetValue(currentValue);
    }

    [SerializeMethod]
    public void ChangeValue(int deltaValue) =>
        ChangeToValueSequence(currentValue + deltaValue).Begin();

    [SerializeMethod]
    public void ChangeToValue(int value) => ChangeToValueSequence(value).Begin();

    public void SetValue(int value)
    {
        currentValue = value;
        UpdateSlider(value);
        UpdateText(value);
    }

    private void UpdateSlider(float value)
    {
        totalPointsSlider.value = value;
    }

    private void UpdateText(int points)
    {
        totalPoints.text = $"{points}/{totalPointsSlider.maxValue}";
    }

    public ISequence ChangeValueSequence(int delta)
    {
        ISequence sequence = CustomSequence.EmptySequence();
        var wrapper = CustomSequence.EmptySequence();
        wrapper = new CustomSequence(BeginWrapper, sequence.End);
        sequence.OnFinished += wrapper.FinishSequence;
        return wrapper;

        void BeginWrapper()
        {
            Debug.Log($"changing {delta}");
            sequence = ChangeToValueSequence(currentValue + delta);
            sequence.OnFinished += wrapper.FinishSequence;
            sequence.Begin();
        }
    }

    public ISequence ChangeToValueSequence(int value)
    {
        var finalValue = (int)Mathf.Clamp(value,
            totalPointsSlider.minValue, totalPointsSlider.maxValue);
        return new CoroutineSequence(new(
            SliderChangeEffect(finalValue), () => SetValue(finalValue)), this);
    }

    private IEnumerator SliderChangeEffect(int finalValue)
    {
        int current = currentValue;
        float durationPerStep = animationDuration / totalPointsSlider.maxValue;
        float duration = durationPerStep * Mathf.Abs(finalValue - currentValue);
        yield return ScriptAnimations.Animate(
            t =>
            {
                var value = Mathf.Lerp(current, finalValue, t);
                UpdateSlider(value);
                UpdateText(Mathf.RoundToInt(value));
            }, curve, duration);
        currentValue = finalValue;
        UpdateSlider(currentValue);
        UpdateText(currentValue);
    }
}
