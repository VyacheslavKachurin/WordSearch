using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelLogic : MonoBehaviour
{
    [SerializeField] LineRenderer _linePrefab;
    [SerializeField] float _lineRendererDrawStep = 1f;

    [SerializeField] LevelView _levelView;

    private AudioManager _audio;
    private bool _isFirstLetter = true;
    private string _word = string.Empty;
    private List<LetterUnit> _tryWordLetterUnits = new List<LetterUnit>();
    private LineRenderer _line = null;

    private List<string> _words;

    private Direction _direction;

    private LevelData _levelData;

    private void Start()
    {
        InputHandler.OnLetterHover += HandleLetterHover;
        InputHandler.OnPointerDrag += HandleDrag;
        InputHandler.OnInputStop += CheckWord;
        _audio = AudioManager.Instance;

    }

    private void HandleDrag(Vector2 point)
    {
        if (Vector2.Distance(_line.GetPosition(_line.positionCount - 1), point) > _lineRendererDrawStep)
        {
            Vector2 newPoint;
            _direction = GetDirection();
            switch (_direction)
            {
                case Direction.Left:
                case Direction.Right:
                    newPoint = new Vector2(point.x, _line.GetPosition(_line.positionCount - 1).y);
                    break;
                case Direction.Up:
                case Direction.Down:
                    newPoint = new Vector2(_line.GetPosition(_line.positionCount - 1).x, point.y);
                    break;
                case Direction.Diagonal:

                    newPoint = GetNextDiagonalPoint(point);
                    break;
                default:
                    newPoint = point;
                    break;
            }
            _line.SetPosition(_line.positionCount - 1, newPoint);
        }
    }

    private Vector2 GetNextDiagonalPoint(Vector2 point)
    {
        var distance = Vector2.Distance(_tryWordLetterUnits[^1].transform.position, point);
        var fullDistance = Vector2.Distance(_tryWordLetterUnits[0].transform.position, _tryWordLetterUnits[1].transform.position);
        var ratio = distance / fullDistance;
        var stepVector = _tryWordLetterUnits[1].transform.position - _tryWordLetterUnits[0].transform.position;
        var nextPoint = Vector2.Lerp(_tryWordLetterUnits[^1].transform.position, _tryWordLetterUnits[^1].transform.position + stepVector, ratio);
        return nextPoint;

    }



    private Direction GetDirection()
    {
        if (_tryWordLetterUnits.Count < 2) return Direction.None;
        var secondLetter = _tryWordLetterUnits[1].transform.position;
        var firstLetter = _tryWordLetterUnits[0].transform.position;


        if (secondLetter.x > firstLetter.x && secondLetter.y == firstLetter.y) _direction = Direction.Right;
        else if (secondLetter.x < firstLetter.x && secondLetter.y == firstLetter.y) _direction = Direction.Left;
        else if (secondLetter.y > firstLetter.y && secondLetter.x == firstLetter.x) _direction = Direction.Up;
        else if (secondLetter.y < firstLetter.y && secondLetter.x == firstLetter.x) _direction = Direction.Down;

        else _direction = Direction.Diagonal;

        return _direction;
    }


    private void CheckWord()
    {
        Debug.Log($"Check word: {_word}");
        _levelView.ToggleWord(false);

        var isWord = _words.Contains(_word);
        var sound = isWord ? Sound.Found : Sound.Error;
        _audio.PlaySound(sound);
        _isFirstLetter = true;
        if (!isWord)
        {
            if (_line != null)
            {

                Destroy(_line.gameObject);
            }
            _line = null;
            _tryWordLetterUnits.Clear();
            return;
        }
        else
        {
            _levelView.HideWord(_word);
            _words.Remove(_word);
        }
        _tryWordLetterUnits.Clear();

        _word = string.Empty;

    }

    private void HandleLetterHover(LetterUnit letter)
    {
        if (_isFirstLetter)
        {
            _line = Instantiate(_linePrefab, letter.transform.position, Quaternion.identity);
            _line.positionCount = 2;
            _line.SetPosition(0, letter.transform.position);
            _line.SetPosition(1, letter.transform.position);
            _word = letter.Letter.ToString();
            _tryWordLetterUnits.Add(letter);
            _isFirstLetter = false;
            _levelView.AddLetter(letter.Letter);
            _levelView.ToggleWord(true);
        }
        else
        {
            if (_tryWordLetterUnits.Contains(letter))
            {
                Debug.Log($"Letter already added: {letter.Letter}");
                return;
            }

            if (!IsLetterOnDirection(letter))
            {
                Debug.Log($"direction is: {_direction}");
                Debug.Log($"Letter not on direction: {letter.Letter}");
                return;
            }
            _word += letter.Letter.ToString();
            _tryWordLetterUnits.Add(letter);
            _line.SetPosition(_line.positionCount - 1, letter.transform.position);
            _line.positionCount++;
            _line.SetPosition(_line.positionCount - 1, letter.transform.position);
            _levelView.AddLetter(letter.Letter);
        }

        if (_word.Length == 1)
        {

            return;
        }

        _audio.PlayLetter(_line.positionCount - 1);
    }

    private bool IsLetterOnDirection(LetterUnit newLetter)
    {
        if (_tryWordLetterUnits.Count < 2)
        {
            return true;
        }
        var lastLetter = _tryWordLetterUnits[^1].transform.position;
        var newLetterPos = newLetter.transform.position;
        var dir = _direction;
        switch (dir)
        {
            case Direction.Left:
                return newLetterPos.x < lastLetter.x && newLetterPos.y == lastLetter.y;
            case Direction.Right:
                return newLetterPos.x > lastLetter.x && newLetterPos.y == lastLetter.y;
            case Direction.Up:
                return newLetterPos.y > lastLetter.y && newLetterPos.x == lastLetter.x;
            case Direction.Down:
                return newLetterPos.y < lastLetter.y && newLetterPos.x == lastLetter.x;
            case Direction.Diagonal:
                return
                newLetterPos.y > lastLetter.y && newLetterPos.x < lastLetter.x ||
                newLetterPos.y > lastLetter.y && newLetterPos.x > lastLetter.x ||
                newLetterPos.y < lastLetter.y && newLetterPos.x < lastLetter.x ||
                newLetterPos.y < lastLetter.y && newLetterPos.x > lastLetter.x;
            default:
                throw new Exception("Cannot determine if letter is on direction");
        }
    }

    internal void SetData(LevelData levelData)
    {
        _words = levelData.Words.ToList();
    }
}
public enum Direction
{
    Up, Down, Left, Right,
    None,
    Diagonal
}