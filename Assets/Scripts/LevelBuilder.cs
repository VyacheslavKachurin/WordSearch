using UnityEngine;
using Newtonsoft.Json;
using System.Collections;

public class LevelBuilder : MonoBehaviour
{
    TextAsset _levelDataAsset;
    [SerializeField] GameBoard _gameBoard;
    [SerializeField] LevelView _levelView;
    [SerializeField] LevelLogic _levelLogic;
    [SerializeField] AbilityLogic _abilityLogic;
    [SerializeField] AdsController _adsController;

    private LevelData _levelData;
    [SerializeField] private LineProvider _lineProvider;

    private void Start()
    {
        CreateLevel();
        _adsController.LoadBanner();
        LevelView.NextLevelClicked += () => CreateLevel();

    }


    [ContextMenu("Reset Progress")]
    private void ResetProgress()
    {
        Session.SetLastLevel(1);
    }

    [ContextMenu("Create Level")]
    public void CreateLevel()
    {
       
        var level = Session.GetLastLevel();
        _levelDataAsset = Resources.Load<TextAsset>($"LevelData/LevelData {level}");
        _levelData = JsonConvert.DeserializeObject<LevelData>(_levelDataAsset.text);

        _gameBoard.BuildBoard(_levelData);

        _levelView.SetLevelData(_levelData);
        _levelLogic.SetData(_levelData);
        _abilityLogic.SetData(_levelData, _gameBoard);
        LoadState();

        StartCoroutine(SetLineSize());
    }

    private IEnumerator SetLineSize()
    {
        _lineProvider.ResetState();
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
        }
        else
        {
            LevelStateService.CreateState(_levelData);
        }
    }
}
