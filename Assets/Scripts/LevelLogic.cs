using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelLogic : MonoBehaviour
{


    [SerializeField] LevelView _levelView;
    [SerializeField] LineProvider _lineProvider;
    [SerializeField] ColorData _colorData;

    private AudioManager _audio;
    private bool _isFirstLetter = true;
    private string _tryWord = string.Empty;
    private List<LetterUnit> _tryWordLetterUnits = new List<LetterUnit>();


    private List<string> _words;
    private Direction _direction;
    private Color _randomColor;
    [SerializeField] private LayerMask _ignoreRaycastLayer;

    private void Start()
    {
        InputHandler.OnLetterHover += HandleLetterHover;
        InputHandler.OnPointerDrag += HandleDrag;
        InputHandler.OnInputStop += CheckWord;

        AbilityBtn.OnAbilityClicked += HandleAbility;
        _audio = AudioManager.Instance;

    }

    private void HandleAbility(Ability ability, int price)
    {
        Debug.Log($"Ability clicked: {ability}, {price}");
    }

    private void HandleDrag(Vector2 point)
    {
        _lineProvider.Draw(point, GetDirection(), GetNextDiagonalPoint(point));

    }

    private Vector2 GetNextDiagonalPoint(Vector2 point)
    {
        if (_tryWordLetterUnits.Count < 2) return point;
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
        if (_tryWord.Length == 0) return;
        Debug.Log($"Check word: {_tryWord}");
        _levelView.ToggleWord(false);

        var isWord = _words.Contains(_tryWord);
        _lineProvider.FinishDraw(isWord, _tryWordLetterUnits);
        var sound = isWord ? Sound.Found : Sound.Error;
        _audio.PlaySound(sound);
        _isFirstLetter = true;
        if (!isWord)
        {
            _tryWordLetterUnits.Clear();
            return;
        }
        else
        {
            _levelView.HideWord(_tryWord);
            _words.Remove(_tryWord);

            foreach (var letter in _tryWordLetterUnits)
                letter.gameObject.layer = _ignoreRaycastLayer;
        }

        _tryWordLetterUnits.Clear();

        _tryWord = string.Empty;

    }

    private void HandleLetterHover(LetterUnit letter)
    {
        if (_isFirstLetter)
        {
            _randomColor = _colorData.GetRandom();
            _lineProvider.CreateLine(letter.transform.position, _randomColor);

            _tryWord = letter.Letter.ToString();
            _tryWordLetterUnits.Add(letter);
            _isFirstLetter = false;
            _levelView.AddLetter(letter.Letter);
            _levelView.ToggleWord(true, _randomColor);
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
            _tryWord += letter.Letter.ToString();
            _tryWordLetterUnits.Add(letter);
            _lineProvider.Append(letter.transform.position);

            _levelView.AddLetter(letter.Letter);
        }

        _audio.PlayLetter(_tryWordLetterUnits.Count);
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