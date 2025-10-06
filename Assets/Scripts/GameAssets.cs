using AddressableAsyncInstances;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameAssets
{
    public static Sprite[] patterns;
    public static Sprite[] books;

    public static void LoadAssets(Action onDone = null)
    {
        Checklist loadedAssets = new(0);
        loadedAssets.onCompleted += () => onDone?.Invoke();
        
        //load patterns
        loadedAssets.AddStep();
        Checklist patternChecklist = new(0);
        patternChecklist.onCompleted += loadedAssets.FinishStep;
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
        //load book sprites
        loadedAssets.AddStep();
        Checklist booksChecklist = new(0);
        booksChecklist.onCompleted += loadedAssets.FinishStep;
        fileName = "books";
        patternCount = 210;
        books = new Sprite[patternCount];
        for (int i = 0; i < patternCount; i++)
        {
            int id = i;
            string path = $"{fileName}[{fileName}_{i}]";
            booksChecklist.AddStep();
            AAAsset<Sprite>.LoadAsset(path, AddLoadedSprite);

            void AddLoadedSprite(Sprite sprite)
            {
                books[id] = sprite;
                booksChecklist.FinishStep();
            }
        }
    }
}
