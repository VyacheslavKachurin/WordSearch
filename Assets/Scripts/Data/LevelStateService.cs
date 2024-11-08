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
            return true;
        }
        Debug.Log($"Level State not found");
        return false;
    }

    public static void DeleteState()
    {
        if (!System.IO.File.Exists(_prePath + _fileName)) return;
        System.IO.File.Delete(_prePath + _fileName);
        Debug.Log($"Level State deleted");
    }

    internal static void CreateState(LevelData levelData)
    {
        State = new(false, levelData.FirstLetters, new List<string>(), new List<Point>(), new List<LineState>(), new List<Point>());
    }

    internal static void AddFoundWord(string tryWord, List<LetterUnit> foundLetters, LineState lineState)
    {
        State.FoundWords.Add(tryWord);

        foreach (var letter in foundLetters)
            State.FoundLetters.Add(letter.Point);

        State.Lines.Add(lineState);

        var firstLetter = foundLetters[0].Point;
        var letterToRemove = State.FirstLetters.FirstOrDefault(x => x.X == firstLetter.X && x.Y == firstLetter.Y);
        var isItThere = letterToRemove != null;
        if (isItThere)
            State.FirstLetters.Remove(letterToRemove);

    }

    internal static Point GetFirstPoint()
    {
        var index = Random.Range(0, State.FirstLetters.Count);
        var point = State.FirstLetters[index];

        if (State.OpenLetters.Contains(point))
        {
            Debug.Log($"This letter is already open");
            return GetFirstPoint();
        }
        State.FirstLetters.Remove(point);
        State.OpenLetters.Add(point);
        OnActiveFirstLetterRemoved?.Invoke(State.FirstLetters.Count);
        return point;
    }
}