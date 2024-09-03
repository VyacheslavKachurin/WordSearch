using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelLogic : MonoBehaviour
{
    [SerializeField] LineRenderer _linePrefab;
    [SerializeField] float _lineRendererDistance = 1f;

    [SerializeField] LevelView _levelView;

    private AudioManager _audio;
    private bool _isFirstLetter = true;
    private string _word = string.Empty;
    private List<LetterUnit> _tryWord = new List<LetterUnit>();
    private LineRenderer _line = null;

    private List<string> _words;

    private void Start()
    {
        InputHandler.OnLetterClick += HandleLetterClick;
        InputHandler.OnDrag += HandleDrag;
        InputHandler.OnInputStop += CheckWord;
        _audio = AudioManager.Instance;

    }

    private void HandleDrag(Direction dir, Vector2 point)
    {
        if (Vector2.Distance(_line.GetPosition(_line.positionCount - 1), point) > _lineRendererDistance)
        {
            Vector2 newPoint;
            switch (dir)
            {
                case Direction.Left:
                case Direction.Right:
                    newPoint = new Vector2(point.x, _line.GetPosition(_line.positionCount - 1).y);
                    break;
                case Direction.Up:
                case Direction.Down:
                    newPoint = new Vector2(_line.GetPosition(_line.positionCount - 1).x, point.y);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }
            _line.SetPosition(_line.positionCount - 1, newPoint);
        }
    }

    private void CheckWord()
    {


        var isWord = _words.Contains(_word);
        var sound = isWord ? Sound.Found : Sound.Error;
        _audio.PlaySound(sound);
        _isFirstLetter = true;
        if (!isWord)
        {
            Destroy(_line);
            _line = null;
            return;
        }
        else
        {
            _levelView.HideWord(_word);
            _words.Remove(_word);
        }




        _tryWord.Clear();
        Debug.Log($"Word: {_word}");



        _word = string.Empty;





    }

    private void HandleLetterClick(LetterUnit letter)
    {
        if (_isFirstLetter)
        {
            _line = Instantiate(_linePrefab, letter.transform.position, Quaternion.identity);
            _line.positionCount = 2;
            _line.SetPosition(0, letter.transform.position);
            _line.SetPosition(1, letter.transform.position);
            _word = letter.Letter.ToString();
            _tryWord.Add(letter);
            _isFirstLetter = false;
        }
        else
        {
            if (_tryWord.Contains(letter))
                return;
            _word += letter.Letter.ToString();
            _tryWord.Add(letter);
            _line.SetPosition(_line.positionCount - 1, letter.transform.position);
            _line.positionCount++;
            _line.SetPosition(_line.positionCount - 1, letter.transform.position);
        }

        if (_word.Length == 1) return;
        _audio.PlayLetter(_line.positionCount - 1);
    }

    internal void SetData(LevelData levelData)
    {
        _words = levelData.Words.ToList();
    }
}
