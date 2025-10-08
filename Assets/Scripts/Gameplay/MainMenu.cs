using System;
using System.Collections;
using System.Collections.Generic;
using AddressableAsyncInstances;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsutton;
    [SerializeField] private Button glossaryButton;
    [Header("Other")]
    [SerializeField] private PlayerDeck currentDeck;
    [SerializeField] private FloorplanColors colors;
    [SerializeField] private ScrollPicker deckPicker;
    [SerializeField] private PlayerDeck[] possibleDecks;

    private void Awake()
    {
        GameAssets.LoadAssets();
        GameSettings.current = new();
        GameSettings.current.floorplanColors = colors;
        startButton.onClick.AddListener(StartRun);
        AAComponent<FloorplanUI>.LoadComponent("FloorplanUI", SetupDeckPick);
    }

    private void SetupDeckPick(FloorplanUI prefab)
    {
        for (int i = 0; i < possibleDecks.Length; i++)
        {
            var deck = possibleDecks[i];
            var instance = Instantiate(prefab, deckPicker.content);
            
            //make a filler floorplan
            var floorplan = ScriptableObject.CreateInstance<Floorplan>().CreateInstance(Vector2Int.up);
            floorplan.Name = deck.name;
            floorplan.Category = deck.preferredCategory;
            floorplan.connections[floorplan.entranceId] = false;
            instance.Setup(floorplan);
        }
        deckPicker.SetupPicker(possibleDecks);
    }

    private void StartRun()
    {
        PlayerDeck pickedDeck = currentDeck ?? possibleDecks[deckPicker.currentOption];
        List<Floorplan> draftPool = new();
        //Load all floorplans
        Addressables.LoadAssetsAsync<Floorplan>("BaseFloorplan", floorplan =>
            draftPool.Add(floorplan.CreateInstance(Vector2Int.up))).Completed += _ => StartGame();
        void StartGame()
        {
            List<Floorplan> deckReference = ReferenceEquals(pickedDeck, null) ? draftPool : pickedDeck.deck;
            List<Floorplan> playedDeck = new(deckReference.Count);
            for (int i = 0; i < playedDeck.Capacity; i++)
                playedDeck.Add(deckReference[i].CreateInstance(Vector2Int.up));
            RunData.playerDeck.deck = playedDeck;
            RunData.playerDeck.preferredCategory = currentDeck?.preferredCategory ?? 0;
            RunData.allFloorplans.deck = draftPool;

            SceneManager.LoadScene(1);
        }
    }
}
