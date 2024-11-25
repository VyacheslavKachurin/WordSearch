using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using System;

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

    private LevelData _levelData;

#if UNITY_EDITOR
    [SerializeField] private int _targetLevel;
    [SerializeField] private bool _loadTargetLevel = false;
    [SerializeField] private int _stage;
#endif


    private void Start()
    {
        CreateLevel();
        _adsController.LoadBanner();
        LevelView.NextLevelClicked += GoNextLevel;

    }

    private void OnDestroy()
    {
        LevelView.NextLevelClicked -= GoNextLevel;
    }


    [ContextMenu("Reset Progress")]
    private void ResetProgress()
    {
        Session.SetLastLevel(1);
    }

    private void GoNextLevel()
    {

        var nextLvl = Session.GetLastLevel() + 1;
        Session.SetLastLevel(nextLvl);
        LevelStateService.DeleteState();
        CreateLevel();
    }

    [ContextMenu("Create Level")]
    public void CreateLevel()
    {
        _lineProvider.ResetState();
        Session.IsSelecting = true;
        var level = Session.GetLastLevel();
        var stage = Session.GetLastStage();

#if UNITY_EDITOR
        if (_loadTargetLevel)
        {
            level = _targetLevel;
            stage = _stage;
        }
#endif
        _levelDataAsset = Resources.Load<TextAsset>($"LevelData/Stage {stage}/LevelData {level}");
        Debug.Log($"Loading textAsset: {stage} {level}");

        _levelData = JsonConvert.DeserializeObject<LevelData>(_levelDataAsset.text);

        _gameBoard.BuildBoard(_levelData);
        var letterDistances= _gameBoard.GetLetterDistances();
        var directions = _gameBoard.GetDirectionVectors();
        _inputHandler.SetDirections(directions);
        _inputHandler.SetLetterDistances(letterDistances);

        _levelView.SetLevelData(_levelData);
        _levelLogic.SetData(_levelData);
        _abilityLogic.SetData(_levelData, _gameBoard);
        StartCoroutine(SetLineSize());
        LoadState();

        SetBg();
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
