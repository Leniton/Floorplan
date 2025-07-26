using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class RarityPicker<T>
{
    public float commonRate = .5f;
    public float uncommonRate = .3f;
    public float rareRate = .15f;
    public float legendRate = .05f;

    public bool allowEmptyResult = false;

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
    public void SetCommonPool(List<T> values) => pool[(int)Rarity.Common] = values;
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
        minRandomValue = Mathf.Min(minRandomValue, totalRarity - legendRate);//at most you guarantee a legend
        List<float> rarities = new() { commonRate, uncommonRate, rareRate, legendRate };
        List<float> sortedRarities = new() { commonRate, uncommonRate, rareRate, legendRate };
        sortedRarities.Sort();//rarity get sorted from least likely to most likely
        float r = Random.Range(minRandomValue, totalRarity);
        float rarityOffset = 0;
        for (int i = 0; i < pool.Length; i++)
        {
            float rarity = sortedRarities[^(i + 1)];
            if (r - rarityOffset < rarity)
            {
                int id = rarities.IndexOf(rarity);
                //Debug.Log($"{(Rarity)id} => {r}({rarity})");
                List<T> pickedRarity = pool[id];

                //treat the case where there's not any element of that rarity (get a rarer one, if possible)
                if (pickedRarity.Count <= 0 && !allowEmptyResult) pickedRarity = pool[NextClosestRarity(id)];

                int elementId = Random.Range(0, pickedRarity.Count);
                T element = elementId < pickedRarity.Count ? pickedRarity[elementId] : default;
                if (removeFromPool) pickedRarity.RemoveAt(elementId);
                return element;
            }
            rarityOffset += rarity;
        }

        return default;
    }

    private int NextClosestRarity(int current)
    {
        int modifier = 1;
        int id = current;
        while (id < pool.Length)
        {
            id += modifier;
            if (id >= pool.Length)
            {
                modifier = -1;
                id = current + modifier;
            }
            if (id < 0) break;

            if (pool[id].Count == 0) continue;
            return id;
        }

        int[] rarityCount = new int[4];
        for (int i = 0; i < pool.Length; i++)
        {
            rarityCount[i] = pool[i].Count;
        }

        StringBuilder sb = new();
        for (int i = 0; i < rarityCount.Length; i++)
            sb.Append($"\n{(Rarity)i}: {rarityCount[i]}");
        throw new System.Exception($"There's no more items on the pool!!\n{sb}");
    }
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Legend
}