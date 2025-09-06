using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button settingsutton;
    [SerializeField] Button glossaryButton;
    [Header("Windows")]
    [SerializeField] GameObject deckPickWindow;
    [Header("Other")]
    [SerializeField] FloorplanColors colors;

    private void Awake()
    {
        GameSettings.current = new();
        GameSettings.current.floorplanColors = colors;
        startButton.onClick.AddListener(StartRun);
    }

    private void StartRun()
    {
        SceneManager.LoadScene(1);
    }
}
