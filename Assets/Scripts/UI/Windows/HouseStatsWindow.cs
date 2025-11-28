using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;
using Util.Extensions;

public class HouseStatsWindow : MonoBehaviour
{
    [SerializeField] private ValueSlider valueSlider;
    [SerializeField] private ValueCounter coinsGained;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private TMP_Text textPrefab;

    private int finalPoints;
    
    private WaitForSeconds delay = new(.5f);
    private SequenceManager queue;

    private static HouseStatsWindow house;
    public static ISequence delaySequence => new CoroutineSequence(new(house.Delay()));
    public static event Action<SequenceManager> OnCheckBonus;

    private void Awake() => house = this;

    public void SkipCurrentAnimation()
    {
        if (!queue.running) return;
        queue?.EndCurrent();
    }

    public void ShowStatsAndEnd()
    {
        gameObject.SetActive(true);
        coinsGained.UpdateText(0);
        valueSlider.UpdateMaxValue(PointsManager.currentRequirement);
        valueSlider.SetValue(0);
        finalPoints = PointsManager.GetTotalPoints();
        queue = new();
        queue.OnFinished += FinishHouse;
        
        queue.Add(valueSlider.ChangeToValueSequence(finalPoints));
        queue.Add(delaySequence);
        FullHouseBonus();
        StepsLeftBonus();
        OnCheckBonus?.Invoke(queue);
        MultiplierBonus();

        //buffer to need to skip to end
        queue.Add(new CustomSequence(null, null));
        queue.Begin();
    }
    
    private void FullHouseBonus()
    {
        int size = GridManager.xSize * GridManager.ySize;
        for (int i = 0; i < size; i++)
            if (!GameManager.roomDict.ContainsKey(new(
                    i % GridManager.xSize, i / GridManager.xSize)))
                return;
        int bonusValue = Mathf.FloorToInt(PointsManager.currentRequirement / 2f);
        ISequence sequence = valueSlider.ChangeValueSequence(bonusValue);
        sequence.OnFinished += () => finalPoints += bonusValue;
        SequenceManager manager = new();
        manager.Add(new ParallelSequences(sequence, BonusTextSequence("Full House! (+50% points)")));
        manager.Add(delaySequence);
        queue.Add(manager);
    }

    private void StepsLeftBonus()
    {
        int stepsLeft = Player.steps;
        var sequence = coinsGained.ChangeValueSequence(stepsLeft);
        sequence.OnFinished += () => Player.ChangeCoins(stepsLeft);
        SequenceManager manager = new();
        manager.Add(new ParallelSequences(sequence, BonusTextSequence($"Steps left ({stepsLeft.SignedValue()})")));
        manager.Add(delaySequence);
        queue.Add(manager);
    }

    private void MultiplierBonus()
    {
        int currentCheck = 2;
        int currentBonus = 5;
        while (currentCheck < 11)
        {
            if (PointsManager.currentRequirement * currentCheck > finalPoints) break;
            int bonus = currentBonus;
            var sequence = coinsGained.ChangeValueSequence(bonus);
            sequence.OnFinished += () => Player.ChangeCoins(bonus);
            SequenceManager manager = new();
            manager.Add(new ParallelSequences(sequence,
                BonusTextSequence($"x{currentCheck} bonus ({bonus.SignedValue()})")));
            manager.Add(delaySequence);
            queue.Add(manager);
            currentCheck++;
            currentBonus += 5;
        }
    }
    
    private void FinishHouse()
    {
        OnCheckBonus = null;
        int targetScene = 2;
        if (finalPoints >= PointsManager.currentRequirement)
        {
            //win, progress
            PointsManager.Progress();
        }
        else
        {
            //lose, reset
            PointsManager.Reset();
            targetScene = 0;
        }
        PointsManager.ResetBonus();
        GameManager.roomDict.Clear();
        GameEvent.ResetListeners();
        SceneManager.LoadScene(targetScene);
    }

    private IEnumerator Delay() { yield return delay; }

    private ISequence BonusTextSequence(string text)
    {
        return new CustomSequence(() =>
        {
            var instance = Instantiate(textPrefab, scroll.content);
            instance.color = Color.white;
            instance.text = text;
        });
    }

    public static ISequence PointBonusSequence(string name, int amount)
    {
        ISequence sequence = house.valueSlider.ChangeValueSequence(amount);
        sequence.OnFinished += () => house.finalPoints += amount;
        return new ParallelSequences(sequence, house.BonusTextSequence($"{name} ({amount.SignedValue()} points)"));
    }
    public static ISequence CoinBonusSequence(string name, int amount)
    {
        ISequence sequence = house.valueSlider.ChangeValueSequence(amount);
        sequence.OnFinished += () => Player.ChangeCoins(amount);
        return new ParallelSequences(sequence, house.BonusTextSequence($"{name} ({amount.SignedValue()} coins)"));
    }
}
