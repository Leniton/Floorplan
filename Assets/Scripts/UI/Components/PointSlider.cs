using System.Collections;
using SerializableMethods;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointSlider : MonoBehaviour
{
    [SerializeField] private TMP_Text totalPoints;
    [SerializeField] private Slider totalPointsSlider;

    [Header("Animation Parameters")] 
    [SerializeField] private float baseSpeed = .5f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

    private int currentPoints;

    [SerializeMethod]
    public void ChangePoints(int points)
    {
        totalPointsSlider.maxValue = PointsManager.currentRequirement;
        StartCoroutine(SliderChangeEffect(points));
    }

    private void UpdatePoints(int points)
    {
        UpdateSlider(points);
        UpdateText(points);
    }

    private void UpdateSlider(float value)
    {
        totalPointsSlider.value = value;
    }

    private void UpdateText(int points)
    {
        totalPoints.text = $"{points}/{totalPointsSlider.maxValue}";
    }

    private IEnumerator SliderChangeEffect(int finalValue)
    {
        int current = currentPoints;
        float durationPerStep = baseSpeed / totalPointsSlider.maxValue;
        float duration = durationPerStep * Mathf.Abs(finalValue - currentPoints);
        yield return ScriptAnimations.Animate(
            t =>
            {
                var value = Mathf.Lerp(current, finalValue, t);
                UpdateSlider(value);
                UpdateText(Mathf.RoundToInt(value));
            }, curve, duration);
        currentPoints = finalValue;
    }
}
