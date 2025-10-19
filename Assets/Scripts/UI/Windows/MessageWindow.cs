using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-5)]
public class MessageWindow : MonoBehaviour
{
    private static MessageWindow instance;
    private static Coroutine messageCoroutine;
    private static Queue<IEnumerator> messageQueue = new();

    [SerializeField] GameObject messageContainer;
    [SerializeField] TMP_Text messageText;

    private void Awake()
    {
        instance = this;
        CloseWindow();
    }

    public static void ShowMessage(string message, Action onDone = null)
    {
        messageQueue.Enqueue(MessageEffect(message, onDone));
        if (messageCoroutine == null)
            messageCoroutine = instance.StartCoroutine(ShowMessages());
    }

    private static IEnumerator MessageEffect(string message, Action onDone)
    {
        instance.OpenWindow();
        instance.messageText.text = message;
        yield return new WaitForSeconds(1.5f);
        onDone?.Invoke();
    }

    private static IEnumerator ShowMessages()
    {
        while (messageQueue.Count > 0)
        {
            yield return messageQueue.Dequeue();
            instance.messageText.text = string.Empty;
            yield return new WaitForSeconds(.5f);
        }
        instance.CloseWindow();
        messageCoroutine = null;
    }

    private void OpenWindow()
    {
        instance.messageContainer.SetActive(true);
    }

    private void CloseWindow()
    {
        instance.messageContainer.SetActive(false);
    }
}
