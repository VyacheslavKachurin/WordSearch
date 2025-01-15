using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

public static class GameDataService
{
    private static string _prePath = Application.persistentDataPath;
    const string _stampDataPath = "/StampData.json";
    private const string _fileName = "/GameData.json";

    public static GameData GameData { get; set; }

    public static StampsData StampsData;

    private const string USERNAME_KEY = "Username";



    public static void SaveStampData()
    {
        string json = JsonConvert.SerializeObject(StampsData);
        System.IO.File.WriteAllText(_prePath + _stampDataPath, json);
    }

    public static StampsData LoadStampData()
    {
        StampsData = new StampsData();
        if (System.IO.File.Exists(_prePath + _stampDataPath))
        {
            string json = System.IO.File.ReadAllText(_prePath + _stampDataPath);
            StampsData = JsonConvert.DeserializeObject<StampsData>(json);
        }
        else
        {
            var inData = Resources.Load("StampData") as TextAsset;
            var parsedData = JsonConvert.DeserializeObject<StampsData>(inData.text);
            parsedData.Seasons[0].IsUnlocked = true;
            StampsData = parsedData;
            SaveStampData();
        }
        return StampsData;
    }

    public static void DeleteStampData()
    {
        if (System.IO.File.Exists(_prePath + _stampDataPath))
            System.IO.File.Delete(_prePath + _stampDataPath);
    }

    public static void UnlockStamp(int season)
    {
        StampsData.Seasons[season].IsUnlocked = true;
        SaveStampData();
    }


    public static GameData CreateGame()
    {
        Debug.Log($"Creating Game");
        var totalEpisodes = CountEpisodes(1);
        GameData = new GameData(1, 1, totalEpisodes, 1);
        SaveGame();
        return GameData;
    }

    public static string GetUsername()
    {
        string username = "";
        if (PlayerPrefs.HasKey(USERNAME_KEY))
            username = PlayerPrefs.GetString(USERNAME_KEY);
        else
        {
            username = Guid.NewGuid().ToString();
            username = username.Substring(0, 4);
            username = "Guest_" + username;
            PlayerPrefs.SetString(USERNAME_KEY, username);
        }

        return username;
    }

    public static void SaveGame()
    {
        Debug.Log($"Saving Game");
        string json = JsonConvert.SerializeObject(GameData);
        System.IO.File.WriteAllText(_prePath + _fileName, json);
    }

    public static void DeleteGame()
    {
        if (System.IO.File.Exists(_prePath + _fileName))
        {
            System.IO.File.Delete(_prePath + _fileName);
        }
    }

    public static int CountEpisodes(int season)
    {
        var path = $"LevelData/Season {season}";
        var files = Resources.LoadAll(path).Select(x => x.name);

        var sortedFiles = files.OrderBy(x => x).ToList();
        Debug.Log($"Counting episodes: {path}, count: {sortedFiles.Count}");
        return sortedFiles.Count;
    }

    public static GameData LoadGame()
    {
        if (System.IO.File.Exists(_prePath + _fileName))
        {
            string json = System.IO.File.ReadAllText(_prePath + _fileName);
            GameData = JsonConvert.DeserializeObject<GameData>(json);
        }
        else
        {
            GameData = CreateGame();
        }

        return GameData;
    }

    public static string GetPathToLevel()
    {

        var game = LoadGame();
        var path = $"LevelData/Season {game.Season}/LevelData {game.Episode}";
        return path;
    }

    internal static bool IncreaseLevel()
    {
        if (GameData.Episode < GameData.TotalEpisodes)
        {
            GameData.Episode++;
        }
        else
        {
            if (Resources.Load($"LevelData/Season {GameData.Season + 1}/LevelData 1") == null)
            {
                Debug.Log($"Game Over");
                Session.IsGameWon = true;
                SaveGame();
                return false;
            }
            UnlockStamp(GameData.Season);
            GameData.Episode = 1;
            GameData.Season++;
            GameData.TotalEpisodes = CountEpisodes(GameData.Season);

        }
        GameData.Level++;
        SaveGame();

        return true;
    }

    internal static void ClearProgress()
    {
        if (System.IO.File.Exists(_prePath + _fileName))
            System.IO.File.Delete(_prePath + _fileName);

        if (System.IO.File.Exists(_prePath + _stampDataPath))
            System.IO.File.Delete(_prePath + _stampDataPath);
    }
}