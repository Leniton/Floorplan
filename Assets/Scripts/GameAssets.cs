using AddressableAsyncInstances;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameAssets
{
    public static Sprite[] patterns;

    public static void LoadAssets(Action onDone = null)
    {
        Checklist patternChecklist = new(0);
        string fileName = "patterns";
        int patternCount = 84;
        patterns = new Sprite[patternCount];
        for (int i = 0; i < patternCount; i++)
        {
            int id = i;
            string path = $"{fileName}[{fileName}_{i}]";
            patternChecklist.AddStep();
            AAAsset<Sprite>.LoadAsset(path, AddLoadedSprite);

            void AddLoadedSprite(Sprite sprite)
            {
                patterns[id] = sprite;
                patternChecklist.FinishStep();
            }
        }
    }
}
