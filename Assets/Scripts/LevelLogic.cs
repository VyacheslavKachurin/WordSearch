using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SingularityGroup.HotReload;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLogic : MonoBehaviour
{
    public static event Action<List<Point>> OnWordFound;

    [SerializeField] LevelView _levelView;
    [SerializeField] LineProvider _lineProvider;

    [SerializeField] InputHandler _inputHandler;

    private AudioManager _audio;
    private bool _isFirstLetter = true;
    private string _tryWord = string.Empty;
    private List<LetterUnit> _tryWordLetterUnits = new();


    private List<string> _words;

    [SerializeField] private LayerMask _ignoreRaycastLayer;
    [SerializeField] private string _menuScene = "MenuScene";

#if UNITY_EDITOR
    [SerializeField] private bool _canContinue = false;
#endif

    public static int Stage;
    public static int Step;
    public static int TotalSteps;



    private bool _shouldCheckForFinish = true;

    private void Start()
    {
        InputHandler.OnInputStop += CheckWord;
        InputTrigger.OnLetterEnter += HandleLetterEnter;
        InputHandler.OnInputDrag += HandleDrag;
        InputHandler.OnLetterDeselect += HandleLetterDeselect;
        // WordFX.OnAnimDone += CheckIfLevelDone;


        _audio = AudioManager.Instance;
        _inputHandler.SetLetterUnits(_tryWordLetterUnits);
        NavigationRow.OnBackBtnClicked += HandleBackBtn;
        _shouldCheckForFinish = true;

    }

    private void OnDestroy()
    {
        InputHandler.OnInputStop -= CheckWord;
        InputTrigger.OnLetterEnter -= HandleLetterEnter;
        InputHandler.OnInputDrag -= HandleDrag;
        InputHandler.OnLetterDeselect -= HandleLetterDeselect;
        //  WordFX.OnAnimDone -= CheckIfLevelDone;
        NavigationRow.OnBackBtnClicked -= HandleBackBtn;
    }


    [ContextMenu("Check if level done")]
    private void IsLevelDone()
    {
#if UNITY_EDITOR
        if (!_canContinue) return;
#endif
        if (!_shouldCheckForFinish) return;
        if (_words.Count == 0)
        {
            Debug.Log($"No words");
            StartCoroutine(LevelDoneCoroutine());
        }
    }

    [ContextMenu("Log words left")]
    private void LogWordsLeft()
    {
        Debug.Log($"words left: {_words.Count}");
    }

    private IEnumerator LevelDoneCoroutine()
    {
        Debug.Log($"waiting until all particles are done, active particles: {ParticleProvider.ActiveParticles}");
        yield return new WaitUntil(() => ParticleProvider.ActiveParticles == 0);
        _levelView.ShowFinishView();
        _shouldCheckForFinish = false;
    }

    private void OnApplicationPause(bool pause)
    {

        SaveState();
    }

    private void HandleBackBtn()
    {
        SaveState();
        SceneManager.LoadScene(_menuScene, LoadSceneMode.Single);
    }

    [ContextMenu("Save State")]
    private void SaveState()
    {
        LevelStateService.SaveState();
    }

    private void HandleLetterDeselect(LetterUnit letter)
    {
        _tryWordLetterUnits.Remove(letter);
        _tryWord = _tryWord.Substring(0, _tryWord.Length - 1);
        Debug.Log($"tryWord deselect letter: {_tryWord}");
        _levelView.RemoveLetter(letter.Letter);
        _audio.PlayLetter(_tryWordLetterUnits.Count);
        _lineProvider.RemovePoint();
        letter.AnimateSelection(false);
    }


    private void HandleDrag(Vector2 point)
    {
        _lineProvider.Draw(point);
    }

    private void CheckWord()
    {
        if (_tryWord.Length == 0) return;

        var isWord = _words.Contains(_tryWord);
        _lineProvider.FinishDraw(isWord, _tryWordLetterUnits);
        var sound = isWord ? Sound.WordFound : Sound.WrongWord;

        _levelView.AnimateHideWord(isWord);
        _audio.PlaySound(sound);
        _isFirstLetter = true;
        if (isWord) RemoveWord();
        else
            foreach (var letter in _tryWordLetterUnits)
                letter.AnimateSelection(false);

        _tryWordLetterUnits.Clear();

        _tryWord = string.Empty;

    }

    private void RemoveWord()
    {
        _levelView.HideWord(_tryWord);
        _words.Remove(_tryWord);

        foreach (var letter in _tryWordLetterUnits)
            letter.Disable();

        OnWordFound?.Invoke(_tryWordLetterUnits.Select(x => x.Point).ToList());
        var linePositions = new List<Vector2>() { _tryWordLetterUnits[0].transform.position, _tryWordLetterUnits[^1].transform.position };

        var lineState = new LineState(linePositions, _tryWordLetterUnits[0].GetColor());
        LevelStateService.AddFoundWord(_tryWord, _tryWordLetterUnits, lineState);

        _levelView.AnimateWord(_tryWordLetterUnits);
        Debug.Log($"calling CheckIfLevelDone");
        IsLevelDone();

    }

    private void HandleLetterEnter(LetterUnit letter)
    {
        letter.AnimateSelection(true);
        letter.AnimateScale();
        if (_isFirstLetter)
        {
            var lineColor = letter.GetColor();
            _lineProvider.CreateLine(letter.transform.position, lineColor);
            _tryWord = letter.Letter.ToString();
            _tryWordLetterUnits.Add(letter);
            _isFirstLetter = false;
            _levelView.AddLetter(letter.Letter);
            _levelView.ToggleWord(true, lineColor);
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

            //            Debug.Log($"adding letter: {letter.Letter}");
            _tryWord += letter.Letter.ToString();
            _tryWordLetterUnits.Add(letter);
            _lineProvider.Append(letter.transform.position);
            _audio.PlayLetter(_tryWordLetterUnits.Count);
            _levelView.AddLetter(letter.Letter);
        }
    }

    [ContextMenu("Complete Level")]
    private void CompleteLevel()
    {
        _words.Clear();
        IsLevelDone();
    }

    [ContextMenu("set stage to 1")]
    private void SetStageTo1()
    {
        Session.LastStage = 1;
    }

    internal void SetData(LevelData levelData)
    {
        _words = levelData.Words.ToList();
        Stage = levelData.Stage;
        Step = levelData.Step;
        TotalSteps = levelData.TotalSteps;
        _shouldCheckForFinish = true;

    }
}
public enum Direction
{
    Up, Down, Left, Right,
    None,
    Diagonal
}