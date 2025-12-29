using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Cheat
{
    public class CheatConsole : MonoBehaviour
    {
        #region Static Setup
        private static CheatConsole instance;
        public static event Action OnSetupDone;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void CreateCheat()
        {
            if (!ReferenceEquals(instance, null)) return;
            instance = CreateCanvas().AddComponent<CheatConsole>();
            DontDestroyOnLoad(instance);
            instance.input = CreateInputField();
        }

        private static GameObject CreateCanvas()
        {
            GameObject obj = CanvasObject("Cheat");
            var canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            var scaler = obj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new(1080, 1920);
            scaler.referencePixelsPerUnit = 100;

            return obj;
        }
        private static TMP_InputField CreateInputField()
        {

            TMP_InputField input = CanvasObject("Input", out var inputRect).AddComponent<TMP_InputField>();

            CanvasObject("TextArea", out var textArea);
            textArea.gameObject.AddComponent<RectMask2D>();
            textArea.SetParent(inputRect);
            textArea.anchorMin = Vector2.zero;
            textArea.anchorMax = Vector2.one;
            textArea.sizeDelta = Vector2.zero;

            Image bg = CanvasObject("BG", out var bgRect).AddComponent<Image>();
            Color color = Color.black;
            color.a = .7f;
            bg.color = color;
            bgRect.SetParent(textArea, false);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = CanvasObject("Text", out var textRect).AddComponent<TextMeshProUGUI>();
            textRect.SetParent(textArea.transform);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            text.alignment = TextAlignmentOptions.MidlineLeft;

            input.textComponent = text;
            input.textViewport = textArea;
            input.transform.SetParent(instance.transform, false);
            inputRect.anchorMin = Vector2.zero;
            inputRect.anchorMax = Vector2.one;
            inputRect.sizeDelta = Vector2.zero;

            input.pointSize = 70;
            return input;
        }

        private static GameObject CanvasObject(string name) => new GameObject(name, typeof(RectTransform));
        private static GameObject CanvasObject(string name, out RectTransform rectTransform)
        {
            var obj = CanvasObject(name);
            rectTransform = obj.transform as RectTransform;
            return obj;
        }
        #endregion

        #region Event Bus
        private static readonly Dictionary<Type, Action<IGenericEvent>> Subscribed = new();
        private static readonly Dictionary<Delegate, Action<IGenericEvent>> EventLookup = new();

        public void Subscribe<T>(Action<T> callback) where T : IGenericEvent
        {
            Type type = typeof(T);
            Action<IGenericEvent> newAction = (o) => callback?.Invoke((T)o);
            EventLookup[callback] = newAction;
            if (!Subscribed.ContainsKey(type))
            {
                Subscribed[type] = newAction;
                return;
            }

            Subscribed[type] += newAction;
        }

        public void Unsubscribe<T>(Action<T> callback) where T : IGenericEvent
        {
            Type type = typeof(T);
            if (!Subscribed.ContainsKey(type) || !EventLookup.ContainsKey(callback)) return;
            Subscribed[type] -= EventLookup[callback];
            EventLookup.Remove(callback);
        }

        public void CallEvent<T>(IEvent<T> evt)
        {
            Type type = evt.GetType();
            if (!Subscribed.ContainsKey(type)) return;

            Subscribed[type]?.Invoke(evt);
        }
        #endregion

        private static Dictionary<string, Action<string[]>> knownCommands = new();

        private TMP_InputField input;

        private void Start()
        {
            input.onSubmit.AddListener(SubmitCommand);
            input.onEndEdit.AddListener(CloseCommandWindow);
            OnSetupDone?.Invoke();
            CloseCommandWindow();
        }

        private void Update()
        {
            if (Keyboard.current.slashKey.wasPressedThisFrame && Keyboard.current.shiftKey.isPressed)
            {
                OpenCommandWindow();
            }
        }

        private void OpenCommandWindow()
        {
            if (input.isFocused) return;
            input.gameObject.SetActive(true);
            input.text = string.Empty;
            input.ActivateInputField();
        }

        private void CloseCommandWindow(string text = "")
        {
            input.gameObject.SetActive(false);
            input.text = string.Empty;
        }

        private void SubmitCommand(string text)
        {
            var splitText = text.Split(' ');
            if (!knownCommands.ContainsKey(splitText[0])) return;
            Debug.Log($"Submit: {text}");
            var parameters = new string[splitText.Length - 1];
            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = splitText[i + 1];
            knownCommands[splitText[0]]?.Invoke(parameters);
        }

        public static void RegisterCommand(string command, Action<string[]> callback)
        {
            if (!knownCommands.ContainsKey(command)) knownCommands.Add(command, null);
            knownCommands[command] += callback;
        }
    }

    /// <summary>
    /// Inherited only by IEvent, must not be Inherited!!
    /// </summary>
    public interface IGenericEvent { }

    public interface IEvent<T> : IGenericEvent
    {
        public T GetData();
    }
}