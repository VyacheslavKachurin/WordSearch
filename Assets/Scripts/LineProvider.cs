using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineProvider : MonoBehaviour
{
    [SerializeField] LineRenderer _linePrefab;

    [SerializeField] float _lineRendererDrawStep = 0.1f;
    private LineRenderer _line = null;

    private float _lineSize;
    [SerializeField] private float _lineSizeMultiplier = 1.2f;
    private List<LineRenderer> _lines = new();

    public static Color? LastColor;

    private int _startDrawOrder = 10;
    private int _currentDrawOrder;

    private void Awake()
    {
        _currentDrawOrder = _startDrawOrder;
    }

    internal void Append(Vector2 point)
    {
        if (_line == null) return;
        _line.SetPosition(_line.positionCount - 1, point);
        _line.positionCount++;
        _line.SetPosition(_line.positionCount - 1, point);
    }


    internal void CreateLine(Vector3 point, Color color)
    {

        LineRenderer line;
        line = Instantiate(_linePrefab, point, Quaternion.identity);
        line.startWidth = _lineSize;
        line.endWidth = _lineSize;
        line.positionCount = 2;
        line.SetPosition(0, point);
        line.SetPosition(1, point);

        line.startColor = color;
        line.endColor = color;

        line.sortingOrder = _currentDrawOrder++;

        _lines.Add(line);
        _line = line;
    }

    internal void Draw(Vector2 point, Direction direction, Vector2 lastLetterPos, int letterCount)
    {
        if (_line == null) return;
        if (Vector2.Distance(_line.GetPosition(_line.positionCount - 1), point) < _lineRendererDrawStep) return;
        var lastPos = lastLetterPos;

        if (letterCount > 1)
            switch (direction)
            {
                case Direction.FromUpLeftToDownRight:
                    if (point.x < lastPos.x && point.y > lastPos.y)
                        return;
                    break;
                case Direction.FromUpRightToDownLeft:
                    if (point.x > lastPos.x && point.y > lastPos.y)
                        return;
                    break;
                case Direction.FromDownLeftToUpRight:
                    if (point.x < lastPos.x && point.y < lastPos.y)
                        return;
                    break;
                case Direction.FromDownRightToUpLeft:
                    if (point.x > lastPos.x && point.y < lastPos.y)
                        return;
                    break;
                default:
                    break;
            }

        _line.SetPosition(_line.positionCount - 1, point);

    }

    internal void FinishDraw(bool keepLine, List<LetterUnit> letters = null, bool correctLastPos = true)
    {
        if (!keepLine && _line != null)
        {
            _currentDrawOrder--;
            Destroy(_line.gameObject);
        }
        if (keepLine && correctLastPos) CorrectLastPosition(letters);
        _line = null;
    }

    public void RemovePoint(int letterCount, Vector2 triggerPos)
    {
        //      Debug.Log($"remove point");
        if (_line == null) return;
        _line.positionCount--;
        if (letterCount == 1)
        {

            _line.positionCount = 1;
            _line.positionCount = 2;
            _line.SetPosition(1, triggerPos);
        }
    }

    private Vector3[] GetAllPositions()
    {
        Vector3[] positions = new Vector3[_line.positionCount];
        _line.GetPositions(positions);
        return positions;
    }


    private void CorrectLastPosition(List<LetterUnit> letters)
    {
        if (_line.positionCount > letters.Count) _line.positionCount = letters.Count;
        _line.SetPosition(_line.positionCount - 1, letters[^1].transform.position);
    }

    internal void SetState(LevelState levelState, LetterUnit[,] letters)
    {

        for (int i = 0; i < levelState.Lines.Count; i++)
        {

            var line = Instantiate(_linePrefab);
            line.positionCount = 2;
            line.SetPosition(0, levelState.Lines[i].Positions[0]);
            line.SetPosition(1, levelState.Lines[i].Positions[1]);
            line.startColor = levelState.Lines[i].color;
            line.endColor = levelState.Lines[i].color;
            line.sortingOrder = _currentDrawOrder++;
            _lines.Add(line);
        }

        for (int i = 0; i < levelState.OpenLetters.Count; i++)
        {
            var point = levelState.OpenLetters[i];
            var vector = new Vector2(point.X, point.Y);

            // if (!_lines.Any(x => x.GetPosition(0) == (Vector3)vector)) continue;
            // open letters are the ones that found and the ones that revealed with abilitys
            //we need to skip the ones that found
            var isThere = levelState.FoundLetters.Any(x => x.X == point.X && x.Y == point.Y);
            if (isThere)
            {
                Debug.Log($"Found letter: {point}");
                continue;
            }

            var letter = letters[point.Y, point.X];
            CreateLine(letter.transform.position, letter.GetColor());

        }
    }

    internal void SetLineSize(float letterSize)
    {
        _lineSize = letterSize * _lineSizeMultiplier;

        for (int i = 0; i < _lines.Count; i++)
        {
            _lines[i].startWidth = _lineSize;
            _lines[i].endWidth = _lineSize;
        }
    }

    [ContextMenu("Reset State")]
    public void ResetState()
    {
        Debug.Log($"Reset state called");
        if (_lines.Count == 0) return;
        foreach (var line in _lines)
        {
            if (line == null) continue;
            Destroy(line.gameObject);
        }
        _currentDrawOrder = _startDrawOrder;
        _lines.Clear();
    }

    [ContextMenu("Log Lines")]
    private void LogLines()
    {
        Debug.Log($"Lines Count: {_lines.Count}");
    }
}