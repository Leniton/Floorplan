using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("General Data")]
    [SerializeField] private TMP_Text steps;
    [SerializeField] private TMP_Text crayons;
    [SerializeField] private TMP_Text coins;
    [SerializeField] private GameObject messageContainer;
    [SerializeField] private TMP_Text messageText;
    [Header("Special Items")]
    [SerializeField] private GameObject itemsContainer;
    [SerializeField] private GameObject dices;
    [SerializeField] private TMP_Text diceText;
    [SerializeField] private GameObject sledgeHammer;
    [SerializeField] private Image colorKey;
    [SerializeField] private FloorplanColors colorRef;
    [Header("Floorplan Details")]
    [SerializeField] private FloorplanWindow details;
    [SerializeField] private BagWindow currentDetails;

    private static UIManager instance;
    private static Coroutine messageCoroutine;
    private static Queue<IEnumerator> messageQueue = new();

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        steps.text = Player.steps.ToString();
        crayons.text = Player.keys.ToString();
        coins.text = Player.coins.ToString();

        dices?.SetActive(Player.dices > 0);
        diceText?.SetText(Player.dices.ToString());
        sledgeHammer?.SetActive(Player.activeSledgeHammer);
        colorKey.gameObject.SetActive(Player.activeKey);
        itemsContainer.SetActive(dices.activeSelf || sledgeHammer.activeSelf || colorKey.gameObject.activeSelf);
        if (!colorKey.gameObject.activeSelf) return;
        colorKey.color = colorRef.GetColor(Player.currentKey.floorCategory);
    }

    public static void ShowDetails(Floorplan floorplan)
    {
        instance.details.SetupWindow(floorplan);
        instance.details.gameObject.SetActive(true);
    }

    public static void ShowCurrentFloorplan()
    {
        instance.currentDetails.OpenBag();
    }

    public static void ShowMessage(string message, Action onDone = null)
    {
        messageQueue.Enqueue(MessageEffect(message, onDone));
        if (messageCoroutine == null)
            messageCoroutine = instance.StartCoroutine(ShowMessages());
    }

    private static IEnumerator MessageEffect(string message, Action onDone)
    {
        instance.messageContainer.SetActive(true);
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
        instance.messageContainer.SetActive(false);
        messageCoroutine = null;
    }
}
