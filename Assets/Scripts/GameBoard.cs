using System;
using System.Collections.Generic;

using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField] LetterUnit _letterPrefab;

    [SerializeField] float _letterScale = 0.8f; //should vary with board size

    private LetterUnit[,] _letters;

    public LetterUnit[,] Letters => _letters;

    [SerializeField] InputMock _inputMock;


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
                SetDirections(x, y, letter, data);

            }
            Y -= boardSize.LetterSize.y;
        }

    }

    [ContextMenu("SetTriggerPath")]
    private void SetTriggerPath()
    {
        _inputMock.SetFirstLetter(_letters[0, 0]);
        _inputMock.SetSecondLetter(_letters[0, 4]);

    }

    private void SetDirections(int x, int y, LetterUnit letter, LevelData data)
    {

        var possibleDirections = new List<Direction>();
        if (x > 0) possibleDirections.Add(Direction.Left);
        if (x < data.Width - 1) possibleDirections.Add(Direction.Right);
        if (y > 0) possibleDirections.Add(Direction.Up);
        if (y < data.Height - 1) possibleDirections.Add(Direction.Down);

        if (x > 0 && y > 0) possibleDirections.Add(Direction.FromDownRightToUpLeft);


        if (x < data.Width - 1 && y < data.Height - 1) possibleDirections.Add(Direction.FromUpLeftToDownRight);

        if (x > 0 && y < data.Height - 1) possibleDirections.Add(Direction.FromUpRightToDownLeft);

        if (x < data.Width - 1 && y > 0) possibleDirections.Add(Direction.FromDownLeftToUpRight);


        letter.SetPossibleDirections(possibleDirections);

    }



    [ContextMenu("Clear Board")]
    public void ClearBoard()
    {
        if (_letters == null) return;
        foreach (var letter in _letters)
        {
            if (letter == null) continue;
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




    [ContextMenu("Get Directions")]
    public Dictionary<Direction, Vector2> GetDirectionVectors()
    {
        //directions correct
        var upLeft = _letters[1, 1] - _letters[0, 0];
        var upRight = _letters[0, 1] - _letters[1, 0];
        var downLeft = _letters[1, 0] - _letters[0, 1];
        var downRight = _letters[0, 0] - _letters[1, 1];
        var dict = new Dictionary<Direction, Vector2>
        {
            { Direction.FromUpLeftToDownRight, upLeft},
            { Direction.FromDownLeftToUpRight, upRight },
            { Direction.FromUpRightToDownLeft, downLeft },
            { Direction.FromDownRightToUpLeft, downRight }
        };
        foreach (var pair in dict)
            pair.Value.Normalize();
        return dict;
    }

    internal List<float> GetLetterDistances()
    {
        var vertical = Vector2.Distance(_letters[1, 0].Pos, _letters[0, 0].Pos);
        var horizontal = Vector2.Distance(_letters[0, 1].Pos, _letters[0, 0].Pos);
        var diagonal = Vector2.Distance(_letters[1, 1].Pos, _letters[0, 0].Pos);
        return new List<float> { horizontal, vertical, diagonal };
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