using System;
using System.Collections.Generic;
using UnityEngine;

public class LineProvider : MonoBehaviour
{
    [SerializeField] LineRenderer _linePrefab;

    [SerializeField] float _lineRendererDrawStep = 0.1f;
    private LineRenderer _line = null;

    private float _lineSize;
    [SerializeField] private float _lineSizeMultiplier = 1.2f;
    private List<LineRenderer> _lines = new();

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

        _lines.Add(line);
        _line = line;
    }

    internal void Draw(Vector2 point)
    {
        if (_line == null) return;
        if (Vector2.Distance(_line.GetPosition(_line.positionCount - 1), point) > _lineRendererDrawStep)
            _line.SetPosition(_line.positionCount - 1, point);

    }

    internal void FinishDraw(bool keepLine, List<LetterUnit> letters = null)
    {
        if (!keepLine && _line != null) Destroy(_line.gameObject);
        if (keepLine) CorrectLastPosition(letters);
        _line = null;
    }

    public void RemovePoint()
    {
        if (_line == null) return;
        _line.positionCount--;
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
        }

        for (int i = 0; i < levelState.OpenLetters.Count; i++)
        {
            var point = levelState.OpenLetters[i];
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
    internal void ResetState()
    {
        Debug.Log($"Reset state called");
        if (_lines.Count == 0) return;
        foreach (var line in _lines)
        {
            if (line == null) continue;
            Destroy(line.gameObject);
        }
        _lines.Clear();
    }
}