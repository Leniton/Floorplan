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
    
    private WaitForSeconds delay = new(1.2f);
    private ISequence delaySequence;
    private SequenceManager queue;

    public void SkipCurrentAnimation()
    {
        if (queue.running)
            queue?.EndCurrent();
    }

    public void ShowStatsAndEnd()
    {
        gameObject.SetActive(true);
        valueSlider.UpdateMaxValue(PointsManager.currentRequirement);
        valueSlider.SetValue(0);
        finalPoints = PointsManager.GetTotalPoints();
        delaySequence = new CoroutineSequence(new(Delay()));
        queue = new();
        queue.OnFinished += FinishHouse;
        
        queue.Add(valueSlider.ChangeToValueSequence(finalPoints));
        queue.Add(delaySequence);
        FullHouseBonus();
        StepsLeftBonus();
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
        ISequence sequence = CustomSequence.EmptySequence();
        var sequenceWrapper = new CustomSequence(() =>
        {
            sequence = valueSlider.ChangeValueSequence(bonusValue);
            sequence.Begin();
        }, sequence.End);
        sequenceWrapper.OnFinished += () => finalPoints += bonusValue;
        queue.Add(new ParallelSequences(sequenceWrapper, BonusTextSequence("Full House!")));
        queue.Add(delaySequence);
    }

    private void StepsLeftBonus()
    {
        int stepsLeft = Player.steps;
        var sequence = coinsGained.ChangeValueSequence(stepsLeft);
        sequence.OnFinished += () => Player.ChangeCoins(stepsLeft);
        queue.Add(new ParallelSequences(sequence, BonusTextSequence($"Steps left ({stepsLeft})")));
        queue.Add(delaySequence);
    }

    private void MultiplierBonus()
    {
        int currentCheck = 2;
        int currentBonus = 10;
        while (currentCheck < 10)
        {
            if (finalPoints / currentCheck <= 0) break;
            Debug.Log($"{finalPoints} ({finalPoints / currentCheck})");
            int bonus = currentBonus;
            var sequence = coinsGained.ChangeValueSequence(bonus);
            sequence.OnFinished += () => Player.ChangeCoins(bonus);
            queue.Add(new ParallelSequences(sequence,
                BonusTextSequence($"x{currentCheck} bonus (+{bonus})")));
            queue.Add(delaySequence);
            currentCheck++;
            currentBonus += 5;
        }
    }
    
    private void FinishHouse()
    {
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
}
