using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class LevelStateService
{
    public static LevelState State;

    private const string fileName = "PlayData.json";
    private const string prePath = "Assets/Resources/";


    public static void SaveState()
    {
        string json = JsonConvert.SerializeObject(State);

        System.IO.File.WriteAllText(prePath + fileName, json);

    }

    public static bool LoadState(out LevelState levelState)
    {
        levelState = null;
        if (System.IO.File.Exists(prePath + fileName))
        {
            string json = System.IO.File.ReadAllText(prePath + fileName);
            levelState = JsonConvert.DeserializeObject<LevelState>(json);
            return true;
        }
        return false;
    }

    public static void DeleteState()
    {
        if (!System.IO.File.Exists(prePath + fileName)) return;
        System.IO.File.Delete(prePath + fileName);
    }

    internal static void CreateState(LevelData levelData)
    {
        State = new(false, levelData.FirstLetters, new List<string>(), new List<Point>());
    }

    internal static void AddFoundWord(string tryWord, List<LetterUnit> foundLetters)
    {
        State.FoundWords.Add(tryWord);

        foreach (var letter in foundLetters)
            State.FoundLetters.Add(letter.Point);
    }
}