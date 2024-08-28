using UnityEngine;
using Newtonsoft.Json;

public class LevelBuilder : MonoBehaviour
{
    [SerializeField] TextAsset _levelDataAsset;

    [SerializeField] GameBoard _gameBoard;
    [SerializeField] LevelView _levelView;

    private LevelData _levelData;


    [ContextMenu("Create Level")]
    public void CreateLevel()
    {
        _levelData = JsonConvert.DeserializeObject<LevelData>(_levelDataAsset.text);

        _gameBoard.BuildBoard(_levelData);

        _levelView.SetLevelData(_levelData);

    }
}
