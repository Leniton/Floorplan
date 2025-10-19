using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerResourcesUI : MonoBehaviour
{
    [Header("General Data")]
    [SerializeField] private TMP_Text steps;
    [SerializeField] private TMP_Text crayons;
    [SerializeField] private TMP_Text coins;
    [Header("Special Items")]
    [SerializeField] private GameObject itemsContainer;
    [SerializeField] private GameObject dices;
    [SerializeField] private TMP_Text diceText;
    [SerializeField] private GameObject sledgeHammer;
    [SerializeField] private Image colorKey;

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
        colorKey.color = GameSettings.current.roomColors.GetColor(Player.currentKey.floorCategory);
    }
}
