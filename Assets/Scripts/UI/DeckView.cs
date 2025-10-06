using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckView : MonoBehaviour
{
    [SerializeField] private FloorplanDetails detailsPrefab;
    [SerializeField] private RectTransform content;
    [SerializeField] private Toggle undraftedToggle;

    private List<FloorplanDetails> detailsList = new();

    private void Awake()
    {
        undraftedToggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool on)
    {
        LoadView();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        LoadView();
    }

    private void LoadView()
    {
        List<Floorplan> floorplanList = undraftedToggle.isOn ? GameManager.DraftPool: RunData.playerDeck.deck;
        
        detailsList.EnsureEnoughInstances(detailsPrefab, floorplanList.Count, content, 
            details => details.onPickedFloorplan += Glossary.OpenGlossary);
        for (int i = 0; i < floorplanList.Count; i++)
            detailsList[i].Setup(floorplanList[i]);
    }
    
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
