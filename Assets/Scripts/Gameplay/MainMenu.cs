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
        GameSettings.current = new();
        GameSettings.current.floorplanColors = colors;
        startButton.onClick.AddListener(StartRun);
    }

    private void StartRun()
    {
        List<Floorplan> draftPool;
        if (!ReferenceEquals(currentDeck, null))
        {
            draftPool = new(currentDeck.deck.Count);
            for (int i = 0; i < draftPool.Capacity; i++)
                draftPool.Add(currentDeck.deck[i].CreateInstance(Vector2Int.up));

            StartGame();
            return;
        }
        draftPool = new();
        Addressables.LoadAssetsAsync<Floorplan>("BaseFloorplan", floorplan =>
            draftPool.Add(floorplan.CreateInstance(Vector2Int.up))).Completed += _ => StartGame();

        void StartGame()
        {
            RunData.playerDeck = ScriptableObject.CreateInstance<PlayerDeck>();
            RunData.playerDeck.deck = draftPool;
            SceneManager.LoadScene(1);
        }
    }
}
