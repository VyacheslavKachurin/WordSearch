using UnityEngine;
using Newtonsoft.Json;

public class LevelBuilder : MonoBehaviour
{
    [SerializeField] TextAsset _levelDataAsset;

    [SerializeField] GameBoard _gameBoard;
    [SerializeField] LevelView _levelView;
    [SerializeField] LevelLogic _levelLogic;
    [SerializeField] AbilityLogic _abilityLogic;

    private LevelData _levelData;
    [SerializeField] private LineProvider _lineProvider;

    private void Start()
    {
        CreateLevel();
    }

    [ContextMenu("Create Level")]
    public void CreateLevel()
    {
        _levelData = JsonConvert.DeserializeObject<LevelData>(_levelDataAsset.text);

        _gameBoard.BuildBoard(_levelData);

        _levelView.SetLevelData(_levelData);
        _levelLogic.SetData(_levelData);
        _abilityLogic.SetData(_levelData, _gameBoard);
        LoadState();
    }

    public void LoadState()
    {
        if (LevelStateService.LoadState(out var levelState))
        {
            _levelView.SetState(levelState);
            _gameBoard.SetState(levelState);
            _lineProvider.SetState(levelState);
        }
        else
        {
            LevelStateService.CreateState(_levelData);
        }
    }
}
