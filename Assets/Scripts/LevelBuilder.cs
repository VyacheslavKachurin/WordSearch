using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine.Advertisements;

public class LevelBuilder : MonoBehaviour
{
    TextAsset _levelDataAsset;
    [SerializeField] GameBoard _gameBoard;
    [SerializeField] LevelView _levelView;
    [SerializeField] LevelLogic _levelLogic;
    [SerializeField] AbilityLogic _abilityLogic;
    [SerializeField] AdsController _adsController;
    [SerializeField] BgController _bgController;
    [SerializeField] private LineProvider _lineProvider;
    [SerializeField] InputHandler _inputHandler;

    [SerializeField] private IAPManager _iapManager;

    private LevelData _levelData;


#if UNITY_EDITOR
    [SerializeField] private int _episode;
    [SerializeField] private bool _loadTargetLevel = false;
    [SerializeField] private int _season;
#endif

    void Awake()
    {
        Debug.Log($" is classic game: {Session.IsClassicGame}");
        _adsController.Init();
    }

    private void Start()
    {
        CreateLevel();

        LevelView.NextLevelClicked += GoNextLevel;

        IShopItems shopItems = _levelView.ShopItems;
        _iapManager.FillUpShopItems(shopItems);

    }

    [ContextMenu("Build Level")]
    private void StartBuildLevel()
    {
        StartCoroutine(BuildLevel());
    }

    private IEnumerator BuildLevel()
    {
        do
        {
            ClearLines();
            CreateLevel();
            DrawLines();
            MakeScreenShot();
            yield return new WaitForSeconds(0.1f);
        }
        while (GameDataService.IncreaseLevel());
    }

    private void MakeScreenShot()
    {
        var season = GameDataService.GameData.Season;
        var episode = GameDataService.GameData.Episode;
        var subject = _levelData.Subject;
        var path = $"Pics/S{season}_E{episode}_{subject}.png";
        Debug.Log($"Screenshot path: {path}");
        ScreenCapture.CaptureScreenshot(path);

    }

    [ContextMenu("Clear Lines")]
    private void ClearLines()
    {
        _lineProvider.ResetState();
    }

    private void DrawLines()
    {
        for (int i = 0; i < _levelData.FirstLetters.Count; i++)
        {
            var firstLetter = _levelData.FirstLetters[i];
            var lastLetter = _levelData.LastLetters[i];
            var firstUnit = _gameBoard.Letters[firstLetter.Y, firstLetter.X];
            var color = firstUnit.GetColor();
            var lastUnit = _gameBoard.Letters[lastLetter.Y, lastLetter.X];
            _lineProvider.CreateLine(firstUnit.transform.position, color);
            _lineProvider.Append(lastUnit.transform.position);
            _lineProvider.FinishDraw(true, null, false);

        }
    }

    private void OnDestroy()
    {
        LevelView.NextLevelClicked -= GoNextLevel;
    }

    private void GoNextLevel()
    {
        if (GameDataService.IncreaseLevel())
        {
            LevelStateService.DeleteState();
            CreateLevel();
        }
        else
        {
            _levelView.ShowGameOver();
        }

    }


    [ContextMenu("Create Level")]
    public void CreateLevel()
    {

        _lineProvider.ResetState();
        Session.IsSelecting = true;

        var path = GameDataService.GetPathToLevel();

#if UNITY_EDITOR
        if (_loadTargetLevel)
        {
            path = $"LevelData/Season {_season}/LevelData {_episode}";
            // GameDataService.CreateGame();
            var episodes = GameDataService.CountEpisodes(_season);
            GameDataService.GameData = new GameData(_season, 1, episodes, _episode);

        }
#endif

        _levelDataAsset = Resources.Load<TextAsset>(path);
        Debug.Log($"Loading textAsset: {path}");

        _levelData = JsonConvert.DeserializeObject<LevelData>(_levelDataAsset.text);

        _gameBoard.BuildBoard(_levelData);
        var letterDistances = _gameBoard.GetLetterDistances();
        var directions = _gameBoard.GetDirectionVectors();
        _inputHandler.SetDirections(directions);
        _inputHandler.SetLetterDistances(letterDistances);

        _levelView.SetLevelData(_levelData);

        _levelLogic.SetData(_levelData);
        _abilityLogic.SetData(_levelData, _gameBoard, _levelView);
        StartCoroutine(SetLineSize());
        LoadState();
        _levelView.SetProgressBar(GameDataService.GameData.Episode, GameDataService.GameData.TotalEpisodes);

        SetBg();
        var gameData = GameDataService.GameData;

        if (Session.IsGameWon)
        {
            _levelView.ShowGameOver();
        }
        else
        {
            EventSender.SendLevelReached(gameData.Season, gameData.Episode, gameData.Level);
        }
    }

    private void SetBg()
    {
        _bgController.CreateBackView();
    }


    private IEnumerator SetLineSize()
    {

        yield return new WaitForEndOfFrame();
        _lineProvider.SetLineSize(_gameBoard.GetLetterHeight());
    }


    public void LoadState()
    {
        Debug.Log($"Load state called");
        if (LevelStateService.LoadState(out var levelState))
        {
            _levelView.SetState(levelState);
            _gameBoard.SetState(levelState);
            _lineProvider.SetState(levelState, _gameBoard.Letters);
            _abilityLogic.SetState(levelState);
            _levelLogic.SetState(levelState);

            if (levelState.FoundWords.Count == _levelData.Words.Count)
            {
                Debug.Log($"Level done");
                _levelView.ShowFinishView(GameDataService.GameData.Episode, GameDataService.GameData.TotalEpisodes);
            }

        }
        else
        {
            LevelStateService.CreateState(_levelData);
        }
    }

    [ContextMenu("Log Data")]
    private void LogData()
    {
        var totalLetters = _levelData.Width * _levelData.Height;
        var usedLetters = totalLetters - _levelData.FakeLetters.Count;
        var ratio = (float)totalLetters / usedLetters;
        Debug.Log($"Ratio: {totalLetters} / {usedLetters}  = {ratio}");
    }

}
