using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelStateService
{
    public static LevelState State;
    public static event Action<int> OnActiveFirstLetterRemoved;
    private const string _fileName = "/LevelState.json";
    private static string _prePath = Application.persistentDataPath;


    public static void SaveState()
    {
        if (!Session.IsClassicGame) return;
        if (State == null) return;
        Debug.Log($"Saving State; path: {_prePath + _fileName}");
        string json = JsonConvert.SerializeObject(State);
        if (State == null) return;

        System.IO.File.WriteAllText(_prePath + _fileName, json);
    }

    public static bool LoadState(out LevelState levelState)
    {
        levelState = null;
        if (System.IO.File.Exists(_prePath + _fileName))
        {
            string json = System.IO.File.ReadAllText(_prePath + _fileName);
            levelState = JsonConvert.DeserializeObject<LevelState>(json);
            State = levelState;

            if (levelState.CurrentLevel == 0|| levelState.CurrentLevel != ProgressService.Progress.Level)
            {
                Debug.Log($"Level State deleted");
                DeleteState();
                return false;
            }
            return true;
        }
        Debug.Log($"Level State not found");
        return false;
    }

    public static void StoreRevealedLetter(RevealedLetter letter)
    {
        State.RevealedFirstLetters.Add(letter);
    }


    public static List<Point> GetSomeFakeLetters(LevelData data)
    {
        int amountToTake = Convert.ToInt32(data.FakeLetters.Count * 0.7f);
        var targetFakeLetters = data.FakeLetters
    .OrderBy(_ => UnityEngine.Random.value)
    .Take(amountToTake)
    .ToList();
        State.RevealedFakeLetters = targetFakeLetters;
        return State.RevealedFakeLetters;
    }

    public static void DeleteState()
    {
        if (!System.IO.File.Exists(_prePath + _fileName)) return;
        System.IO.File.Delete(_prePath + _fileName);
        Debug.Log($"Level State deleted");
    }

    public static void CreateState(LevelData levelData)
    {
        var level = ProgressService.Progress.Level;
        var firstLetters = levelData.Words.Select(x => x.FirstLetter).ToList();
        State = new(false, firstLetters);
        State.CurrentLevel=ProgressService.Progress.Level;
    }

    internal static void AddFoundWord(string word, List<LetterUnit> foundLetters, Color color)
    {
        if (!Session.IsClassicGame) return;
        var points = foundLetters.Select(x => x.Point).ToList();
        var foundWord = new FoundWord(word, points, color);

        var firstLetter = foundLetters[0].Point;
        State.ActiveFirstLetters.Remove(firstLetter);
        State.FoundWords.Add(foundWord);
    }

    public static void AddWord(string word)
    {
        var foundWord = new FoundWord(word, new List<Point>(), Color.white);
        State.FoundWords.Add(foundWord);

    }

    internal static Point GetFirstLetter()
    {
        var index = Random.Range(0, State.ActiveFirstLetters.Count);
        var point = State.ActiveFirstLetters[index];

        State.ActiveFirstLetters.Remove(point);
        OnActiveFirstLetterRemoved?.Invoke(State.ActiveFirstLetters.Count);
        return point;
    }
}