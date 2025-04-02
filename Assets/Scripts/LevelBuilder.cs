using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine.Advertisements;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class LevelBuilder : MonoBehaviour
{
    TextAsset _levelDataAsset;
    [SerializeField] GameBoard _gameBoard;
    [SerializeField] LevelView _levelView;
    [SerializeField] LevelLogic _levelLogic;
    [SerializeField] AbilityLogic _abilityLogic;

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

        PlateView.OnReplayRequested += CreateLevel;
    }


    private async void Start()
    {
        try
        {
            CreateLevel();
        }
        catch (Exception e)
        {
            Debug.Log($"Error in Start: {e}");
        }

        LevelView.NextLevelClicked += LoadNextLevel;

        IShopItems shopItems = _levelView.ShopItems;
        await _iapManager.Create();
        _iapManager.InjectShopItems(shopItems);
        //     UnityAdsController.Instance.InitializeAds();

    }

    private void MakeScreenShot()
    {
        var season = ProgressService.Progress.Season;
        var episode = ProgressService.Progress.Episode;
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
        for (int i = 0; i < _levelData.Words.Length; i++)
        {
            var wordData = _levelData.Words[i];
            var firstLetter = wordData.FirstLetter;
            var lastLetter = wordData.LastLetter;
            var firstUnit = _gameBoard.Letters[firstLetter.Y, firstLetter.X];
            var lastUnit = _gameBoard.Letters[lastLetter.Y, lastLetter.X];
            _lineProvider.CreateLine(firstUnit.transform.position);
            _lineProvider.Append(lastUnit.transform.position);
            _lineProvider.FinishDraw(true, null, false);

        }
    }

    private void OnDestroy()
    {
        LevelView.NextLevelClicked -= LoadNextLevel;
        PlateView.OnReplayRequested -= CreateLevel;
        AdsController.Instance.HideBanners();
        AdsController.Instance = null;
    }

    private void LoadNextLevel()
    {
        LevelStateService.DeleteState();

        if (!Session.IsGameWon)
        {
            CreateLevel();
        }
        else
        {
            _levelView.ShowGameOver();
        }

    }

    [ContextMenu("Create Level")]
    public async void CreateLevel()
    {
        AdsController.Instance.RevealBanner();
        _inputHandler.FinishSelecting();
        var IsClassicGame = Session.IsClassicGame;

        if (!IsClassicGame)
            LevelStateService.DeleteState();

        _lineProvider.ResetState();
        Session.IsSelecting = true;

        // await ProgressService.LoadProgress();
        var season = ProgressService.Progress.Season;
        var episode = ProgressService.Progress.Episode;

        var level = ProgressService.Progress.Level;
        var gameData = ProgressService.Progress;

        KeitaroSender.SendLevelReached(gameData.Season, gameData.Episode, gameData.Level);
        if (level == 10)
            KeitaroSender.SendLevel10Reached(gameData.Season, gameData.Episode, gameData.Level);


        _levelData = GetLevelData(season, episode);



        //  _levelData = JsonConvert.DeserializeObject<LevelData>(_levelDataAsset.text);

        _gameBoard.BuildBoard(_levelData);
        var letterDistances = _gameBoard.GetLetterDistances();
        var directions = _gameBoard.GetDirectionVectors();
        _inputHandler.SetDirections(directions);
        _inputHandler.SetLetterDistances(letterDistances);

        _levelView.SetLevelData(_levelData);

        _levelLogic.SetData(_levelData);
        _abilityLogic.SetData(_levelData, _gameBoard, _levelView);
        StartCoroutine(SetLineSize());

        if (IsClassicGame)
            LoadState();
        else
        {
            LevelStateService.CreateState(_levelData);
            _levelLogic.SetTimeMode();
        }

        _levelView.InitProgressBar(ProgressService.Progress.Episode, ProgressService.Progress.TotalEpisodes);

        SetBg();



        if (Session.IsGameWon)
        {
            _levelView.ShowGameOver();
            _levelLogic.ToggleGameObjects(false);
        }
        else
        {
            AppMetricaService.SendLevelReached(gameData.Season, gameData.Episode, gameData.Level);
        }
    }

    private LevelData GetLevelData(int season, int episode)
    {
        var levelPath = Path.Combine(Application.persistentDataPath, "Levels", $"Season {season}", $"LevelData {episode}.json");
        var levelData = JsonConvert.DeserializeObject<LevelData>(File.ReadAllText(levelPath));
        return levelData;
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

        if (LevelStateService.LoadState(out var levelState))
        {
            _levelView.SetState(levelState);
            _gameBoard.SetState(levelState);
            _lineProvider.SetState(levelState, _gameBoard.Letters);
            _abilityLogic.SetState(levelState);
            _levelLogic.SetState(levelState);

            if (levelState.FoundWords.Count == _levelData.Words.Length)
            {
                Debug.Log($"Level done");

                _levelView.ShowFinishView(ProgressService.Progress.Episode, ProgressService.Progress.TotalEpisodes);
                _levelView.ShowNextLvlBtn();
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
