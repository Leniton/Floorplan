using System.Collections;
using System.Collections.Generic;
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
    [Header("Windows")]
    [SerializeField] private GameObject deckPickWindow;
    [Header("Other")]
    [SerializeField] private FloorplanColors colors;

    private PlayerDeck currentDeck;

    private void Awake()
    {
        GameAssets.LoadAssets();
        GameSettings.current = new();
        GameSettings.current.floorplanColors = colors;
        startButton.onClick.AddListener(StartRun);
    }

    private void StartRun()
    {
        List<Floorplan> draftPool = new();
        //Load all floorplans
        Addressables.LoadAssetsAsync<Floorplan>("BaseFloorplan", floorplan =>
            draftPool.Add(floorplan.CreateInstance(Vector2Int.up))).Completed += _ => StartGame();

        void StartGame()
        {
            List<Floorplan> deckReference = ReferenceEquals(currentDeck, null) ? draftPool : currentDeck.deck;
            List<Floorplan> playedDeck = new(deckReference.Count);
            for (int i = 0; i < playedDeck.Capacity; i++)
                playedDeck.Add(deckReference[i].CreateInstance(Vector2Int.up));
            RunData.playerDeck.deck = playedDeck;
            RunData.allFloorplans.deck = draftPool;

            SceneManager.LoadScene(1);
        }
    }
}
