using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class LevelLogic : MonoBehaviour
{


    [SerializeField] LevelView _levelView;
    [SerializeField] LineProvider _lineProvider;
    [SerializeField] ColorData _colorData;
    [SerializeField] InputHandler _inputHandler;

    private AudioManager _audio;
    private bool _isFirstLetter = true;
    private string _tryWord = string.Empty;
    private List<LetterUnit> _tryWordLetterUnits = new();


    private List<string> _words;
    private Direction _direction;
    private Color _randomColor;
    [SerializeField] private LayerMask _ignoreRaycastLayer;


    private void Start()
    {
        //  InputHandler.OnLetterHover += HandleLetterHover;
        // InputHandler.OnPointerDrag += HandleDrag;
        InputHandler.OnInputStop += CheckWord;


        InputTrigger.OnLetterEnter += HandleLetterEnter;
        //InputTrigger.OnLetterExit += HandleLetterExit;
        InputHandler.OnInputDrag += HandleDrag;

        AbilityBtn.OnAbilityClicked += HandleAbility;
        _audio = AudioManager.Instance;

        _inputHandler.SetLetterUnits(_tryWordLetterUnits);

    }

    private void HandleLetterExit(LetterUnit letter)
    {


    }

    private void HandleAbility(Ability ability, int price)
    {
        Debug.Log($"Ability clicked: {ability}, {price}");
    }

    private void HandleDrag(Vector2 point)
    {
        _lineProvider.Draw(point);
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
        if (isWord)
        {

            _levelView.HideWord(_tryWord);
            _words.Remove(_tryWord);

            foreach (var letter in _tryWordLetterUnits)
                letter.gameObject.layer = _ignoreRaycastLayer;
        }

        _tryWordLetterUnits.Clear();

        _tryWord = string.Empty;

    }

    private void HandleLetterEnter(LetterUnit letter)
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
            _audio.PlayLetter(_tryWordLetterUnits.Count);
        }
        else
        {
            if (_tryWordLetterUnits.Count == 0) return;

            if (_tryWordLetterUnits.Contains(letter))
            {
                Debug.Log($"Letter already added: {letter.Letter}");
                return;
            }

            Debug.Log($"adding letter: {letter.Letter}");
            _tryWord += letter.Letter.ToString();
            _tryWordLetterUnits.Add(letter);
            _lineProvider.Append(letter.transform.position);

            _levelView.AddLetter(letter.Letter);
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