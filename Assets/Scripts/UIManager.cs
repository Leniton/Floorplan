using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("General Data")]
    [SerializeField] private TMP_Text steps;
    [SerializeField] private TMP_Text crayons;
    [SerializeField] private TMP_Text coins;
    [Header("Floorplan Details")]
    [SerializeField] private GameObject detailsContainer;
    [SerializeField] private FloorplanDetails details;

    private static UIManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        steps.text = Player.steps.ToString();
        crayons.text = Player.keys.ToString();
        coins.text = Player.coins.ToString();
    }

    public static void ShowDetails(Floorplan floorplan)
    {
        instance.details.Setup(floorplan);
        instance.detailsContainer.SetActive(true);
    }

    public void CloseDetailsPopup()
    {
        detailsContainer.SetActive(false);
    }
}
