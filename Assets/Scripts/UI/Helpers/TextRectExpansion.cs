using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Components.Utility
{
    [ExecuteAlways]
    public class TextRectExpansion : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Vector2 minValue, maxValue;
        [SerializeField] private Vector2 padding;
        private RectTransform _rectTransform;
        public bool expandOnUpdate = true;
        [Range(0.01f, 1)] public float lerpSpeed = 1f;

        private RectTransform rect
        {
            get
            {
                if (!_rectTransform) _rectTransform = (RectTransform)transform;
                return _rectTransform;
            }
        }

        private void Update()
        {
            if (!text || !expandOnUpdate) return;
            ExpandRect();
        }

        private void ExpandRect()
        {
            if (string.IsNullOrEmpty(text.text))
            {
                rect.sizeDelta = minValue;
                LayoutRebuilder.ForceRebuildLayoutImmediate(text.rectTransform);
                return;
            }
            var targetWidth = Mathf.Clamp(text.bounds.size.x + padding.x, minValue.x, maxValue.x);
            var targetHeight = Mathf.Clamp(text.bounds.size.y + padding.y, minValue.y, maxValue.y);

            var targetSize = new Vector2(targetWidth, targetHeight);
            rect.sizeDelta = Vector2.Lerp(rect.sizeDelta, targetSize, lerpSpeed);

            LayoutRebuilder.ForceRebuildLayoutImmediate(text.rectTransform);
        }
    }
}