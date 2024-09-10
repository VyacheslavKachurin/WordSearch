using System;
using System.Collections.Generic;
using UnityEngine;

public class LineProvider : MonoBehaviour
{
    [SerializeField] LineRenderer _linePrefab;

    [SerializeField] float _lineRendererDrawStep = 0.1f;
    private LineRenderer _line = null;

    internal void Append(Vector2 point)
    {
        if (_line == null) return;
        _line.SetPosition(_line.positionCount - 1, point);
        _line.positionCount++;
        _line.SetPosition(_line.positionCount - 1, point);
    }


    internal void CreateLine(Vector3 point, Color color)
    {
        _line = Instantiate(_linePrefab, point, Quaternion.identity);
        _line.positionCount = 2;
        _line.SetPosition(0, point);
        _line.SetPosition(1, point);

        _line.startColor = color;
        _line.endColor = color;
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

}