using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Text;
using UnityEngine.Networking;
using System.IO.Compression;


public static class ProgressService
{
    private static string _prePath = Application.persistentDataPath;
    const string _stampDataPath = "/StampData.json";
    private const string _fileName = "/GameData.json";

    public static UserProgress Progress { get; set; }

    public static StampsData StampsData;

    public static bool AdsRemoved => Progress.AdsRemoved;


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

    public static string GetStampTitle(int season)
    {
        return StampsData.Seasons[season].Name;
    }

    public static void DeleteStampData()
    {
        if (System.IO.File.Exists(_prePath + _stampDataPath))
            System.IO.File.Delete(_prePath + _stampDataPath);
    }

    public static void UnlockStamp(int season)
    {
        StampsData.Seasons[season - 1].IsUnlocked = true;
        SaveStampData();
    }

    public static async Task SaveGameAsync()
    {
        string json = JsonConvert.SerializeObject(Progress);
        using var httpClient = new HttpClient(new HttpClientHandler());

        var url = $"{Services.GetHostUrl()}/save_progress/{Services.UserId}";
        //   using UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(url, json);
        //   webRequest.SetRequestHeader("Content-Type", "application/json");
        // webRequest.SetRequestHeader("Accept", "application/json");
        //webRequest.timeout = 10;
        //webRequest.SendWebRequest();
        // var result = webRequest;
        var result = await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        if (!result.StatusCode.Equals(200))
        {
            Debug.Log($"Failed to save progress to server: {result.StatusCode},reason: {result.ReasonPhrase}, url: {url}");
        }
        else
        {
            Debug.Log($"Success to save progress to server: {result.StatusCode}");
        }

    }

    public static async Task SaveCoins(int balance)
    {
        Progress.Coins = balance;

        // using UnityWebRequest webRequest = new UnityWebRequest();
        using HttpClient httpClient = new HttpClient(new HttpClientHandler());

        var url = $"{Services.GetHostUrl()}/update_coins/{Services.UserId}/{balance}";
        //   var result = webRequest.SendWebRequest();

        var response = await httpClient.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            Debug.Log($"Success to update coins to server: {response.StatusCode}");
        }
        else
        {
            Debug.Log($"Failed to update coins to server: {response.StatusCode},reason: {response.ReasonPhrase},url: {url}");
        }

    }


    public static void ClearProgress()
    {
        if (System.IO.File.Exists(_prePath + _fileName))
        {
            System.IO.File.Delete(_prePath + _fileName);
        }
    }

    public static int CountEpisodes(int nextSeason)
    {

        var path = Path.Combine(Application.persistentDataPath, "Levels", $"Season {nextSeason}");
        var files = Directory.GetFiles(path);
        return files.Length;
    }

    public static async Task<UserProgress> LoadProgress()
    {
        Debug.Log("load progress");
        var url = $"{Services.GetHostUrl()}/Users/{Services.UserId}";
        using UnityWebRequest webRequest = UnityWebRequest.Get(url);
        webRequest.timeout = 10;
        await webRequest.SendWebRequest();
        while (!webRequest.isDone)
            await Task.Yield();
        var response = webRequest;
        UserProgress progress = new();
        if (response.responseCode == 200)
        {

            var json = response.downloadHandler.text;
            progress = JsonConvert.DeserializeObject<UserProgress>(json);
        }
        else
        {
            Debug.Log($"Failed to load progress from server: {response.responseCode},reason: {response.error},url: {url}");
        }
        Progress = progress;
        return progress;
    }

    public async static Task LoadLevels()
    {

        var url = $"{Services.GetHostUrl()}/levels/get";
        var zipPath = Application.persistentDataPath + "/Levels.zip";
        using UnityWebRequest webRequest = new UnityWebRequest(url);
        webRequest.downloadHandler = new DownloadHandlerFile(zipPath);
        webRequest.timeout = 10;
        var response = webRequest.SendWebRequest();

        while (!response.isDone)
            await Task.Yield();

        if (response.webRequest.result == UnityWebRequest.Result.Success)
        {
            var levelsPath = Path.Combine(_prePath, "Levels");
            if (Directory.Exists(levelsPath))
                Extensions.DeleteDirectory(levelsPath);

            ZipFile.ExtractToDirectory(zipPath, levelsPath);

            if (File.Exists(zipPath))
                File.Delete(zipPath);

        }
    }

    public static void DeleteLocalProgress()
    {
        if (System.IO.File.Exists(_prePath + _fileName))
            System.IO.File.Delete(_prePath + _fileName);
    }

    internal static async Task FinishLevel()
    {
        if (Progress.Episode == Progress.TotalEpisodes)
        {
            var nextSeason = Progress.Season + 1;
            var nextEpisode = 1;

            UnlockStamp(nextSeason);

            Progress.Season = nextSeason;
            Progress.Episode = nextEpisode;
            try { Progress.TotalEpisodes = CountEpisodes(nextSeason); }
            catch
            {
                Session.IsGameWon = true;
                return;
            }

        }
        else
        {
            var nextEpisode = Progress.Episode + 1;
            Progress.Episode = nextEpisode;
        }
        var nextLevel = Progress.Level + 1;
        Progress.Level = nextLevel;
        SaveGameAsync();

    }

    public static async Task SetAdsRemovedAsync()
    {
        // using UnityWebRequest webRequest = UnityWebRequest.Get($"{Services.GetHostUrl()}/ads/remove/{Services.UserId}");
        using var httpClient = new HttpClient(new HttpClientHandler());
        // var result = webRequest.SendWebRequest();
        var url = $"{Services.GetHostUrl()}/ads/remove/{Services.UserId}";
        var result = await httpClient.GetAsync(url);
        //  while (!result.isDone)
        //     Task.Yield();
        var response = result;
        if (response.StatusCode == HttpStatusCode.OK)
        {
            Debug.Log($"Success to remove ads to server: {response.StatusCode}");
        }
        else
        {
            Debug.Log($"Failed to remove ads to server: {response.StatusCode},reason: {response.ReasonPhrase},url: {url}");
        }

    }

    internal static async Task LoadPictures()
    {
        var url = $"{Services.GetHostUrl()}/pictures/get";
        var zipPath = Application.persistentDataPath + "/Pictures.zip";
        using UnityWebRequest webRequest = new UnityWebRequest(url);
        webRequest.downloadHandler = new DownloadHandlerFile(zipPath);
        webRequest.timeout = 10;
        var response = webRequest.SendWebRequest();

        while (!response.isDone)
            await Task.Yield();

        if (response.webRequest.result == UnityWebRequest.Result.Success)
        {
            var picsPath = Path.Combine(_prePath, "Pictures");
            if (Directory.Exists(picsPath))
                Extensions.DeleteDirectory(picsPath);

            ZipFile.ExtractToDirectory(zipPath, picsPath);

            while (!Directory.Exists(picsPath))
                await Task.Yield();

            if (File.Exists(zipPath))
                File.Delete(zipPath);
        }
    }
}