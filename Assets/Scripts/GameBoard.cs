using System;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField] LetterUnit _letterPrefab;

    [SerializeField] int _boardWidth;
    [SerializeField] int _boardHeight;

    [SerializeField] float _letterScale = 0.8f; //should vary with board size

    private LetterUnit[,] _letters;

    public LetterUnit[,] Letters => _letters;


    private void Start()
    {
        LevelLogic.OnWordFound += HandleWordFound;
    }

    private void OnDestroy(){
        LevelLogic.OnWordFound -= HandleWordFound;
    }

    [ContextMenu("Access Letters")]
    private void TryAccessLetters()
    {
        var position = Letters[1, 1].transform.position;
    }

    private void HandleWordFound(List<Point> list)
    {

    }

    [ContextMenu("Build Board")]
    public void BuildBoard(LevelData data)
    {
        ClearBoard();
        var boardSize = GetSize(data.Width, data.Height);
        _letters = new LetterUnit[data.Height, data.Width];

        var X = boardSize.X;
        var Y = boardSize.Y;

        for (int y = 0; y < data.Height; y++)
        {
            X = boardSize.X;

            for (int x = 0; x < data.Width; x++)
            {
                var letter = Instantiate(_letterPrefab, transform);
                letter.transform.localPosition = new Vector3(X, Y, 0);
                _letters[y, x] = letter;
                letter.SetLetter(data[y, x], new Point(x, y));
                letter.SetSize(boardSize.LetterSize, _letterScale);
                X += boardSize.LetterSize.x;
            }
            Y -= boardSize.LetterSize.y;
        }
        Debug.Log($"Game board built");

    }



    [ContextMenu("Clear Board")]
    public void ClearBoard()
    {
        if (_letters == null) return;
        foreach (var letter in _letters)
        {
            Destroy(letter.gameObject);
        }

        _letters = null;

    }

    [ContextMenu("Log Dimensions")]
    public BoardSize GetSize(int width, int height)
    {
        var renderer = GetComponent<SpriteRenderer>();
        return new BoardSize(renderer, width, height);
    }

    internal void SetState(LevelState levelState)
    {
        foreach (var letter in levelState.FoundLetters)
        {
            _letters[letter.Y, letter.X].AnimateSelection(true);
        }
        for (int i = 0; i < levelState.FoundLetters.Count; i++)
        {

            var point = levelState.FoundLetters[i];
            _letters[point.Y, point.X].Disable();
        }
    }

    public float GetLetterHeight()
    {
        return _letters[0, 0].GetComponent<Renderer>().bounds.size.y;
    }

}



public class BoardSize
{
    public float Width;
    public float Height;
    public Vector2 LetterSize;

    public float X => (-Width / 2) + LetterSize.x / 2;
    public float Y => (Height / 2) - LetterSize.y / 2;

    public BoardSize(SpriteRenderer renderer, int boardWidth, int boardHeight)
    {
        Width = renderer.bounds.size.x;
        Height = renderer.bounds.size.y;
        LetterSize = new Vector2(Width / boardWidth, Height / boardHeight);
    }
}