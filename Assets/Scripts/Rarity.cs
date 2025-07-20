using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RarityPicker<T>
{
    public float commonRate = .5f;
    public float uncommonRate = .3f;
    public float rareRate = .15f;
    public float legendRate = .05f;

    private List<T>[] pool;

    public RarityPicker(float common = .5f, float uncommon = .3f, float rare = .15f, float legend = .05f)
    {
        pool = new List<T>[4];
        for (int i = 0; i < pool.Length; i++) pool[i] = new();

        commonRate = common;
        uncommonRate = uncommon;
        rareRate = rare;
        legendRate = legend;
    }

    #region ModifyList
    public void AddToPool(T element, Rarity rarity) => pool[(int)rarity].Add(element);
    public void SetCommonPool(List<T> values) => pool[(int)Rarity.Commom] = values;
    public void SetUncommonPool(List<T> values) => pool[(int)Rarity.Uncommon] = values;
    public void SetRarePool(List<T> values) => pool[(int)Rarity.Rare] = values;
    public void SetLegendPool(List<T> values) => pool[(int)Rarity.Legend] = values;
    #endregion

    public void ChangeRarities(float common = .5f, float uncommon = .3f, float rare = .15f, float legend = .05f)
    {
        commonRate = common;
        uncommonRate = uncommon;
        rareRate = rare;
        legendRate = legend;
    }

    public T PickRandom(float minRandomValue = 0, bool removeFromPool = false)
    {
        float totalRarity = commonRate + uncommonRate + rareRate + legendRate;
        List<float> rarities = new() { commonRate, uncommonRate, rareRate, legendRate };
        rarities.Sort();
        float r = Random.Range(minRandomValue, totalRarity);
        float rarityOffset = 0;
        for (int i = 0; i < pool.Length; i++)
        {
            float rarity = rarities[rarities.Count - (i + 1)];//list is in inverse order
            if (r - rarityOffset < rarity)
            {
                List<T> pickedRarity = pool[i];

                //treat the case where there's not any element of that rarity (get a rarer one, if possible)

                int elementId = Random.Range(0, pickedRarity.Count);
                T element = pickedRarity[elementId];
                if (removeFromPool) pickedRarity.RemoveAt(elementId);
                return element;
            }
            rarityOffset += rarity;
        }

        return default;
    }
}

public enum Rarity
{
    Commom,
    Uncommon,
    Rare,
    Legend
}