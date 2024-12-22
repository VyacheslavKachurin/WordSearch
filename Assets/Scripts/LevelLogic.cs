using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLogic : MonoBehaviour
{
    public static event Action<List<Point>> OnWordFound;

    [SerializeField] LevelView _levelView;
    [SerializeField] LineProvider _lineProvider;

    private AudioManager _audio;
    private bool _isFirstLetter = true;
    private string _tryWord = string.Empty;
    private List<LetterUnit> _tryWordLetterUnits = new();

    private List<string> _words;


#if UNITY_EDITOR
    [SerializeField] private bool _useTestData = false;
#endif

    // public static int Stage;
    public static int Step;
    public static int TotalSteps;


    private bool _shouldCheckForFinish = true;

    [SerializeField] Canvas _canvas;

#if UNITY_EDITOR
    [SerializeField] private int _editorEpisode;
    [SerializeField] private int _editorTotalEpisodes;
#endif

    private void Start()
    {
        InputHandler.OnInputStop += CheckWord;
        InputTrigger.OnLetterEnter += HandleLetterEnter;
        InputHandler.OnTriggerMove += HandleTriggerMove;
        InputHandler.OnLetterDeselect += HandleLetterDeselect;

        _audio = AudioManager.Instance;
        // _inputHandler.SetLetterUnits(_tryWordLetterUnits);
        _shouldCheckForFinish = true;

        LevelView.OnGameViewToggle += HideGameObjects;
        LevelView.OnBackClicked += HandleBackClick;

    }

    public void HideGameObjects(bool isVisible)
    {
        _canvas.sortingOrder = isVisible ? 0 : 999;
    }

    private void OnDestroy()
    {
        InputHandler.OnInputStop -= CheckWord;
        InputTrigger.OnLetterEnter -= HandleLetterEnter;
        InputHandler.OnTriggerMove -= HandleTriggerMove;
        InputHandler.OnLetterDeselect -= HandleLetterDeselect;
        LevelView.OnGameViewToggle -= HideGameObjects;
    }


    [ContextMenu("Check if level done")]
    private void CheckFinishCondition()
    {
        if (!_shouldCheckForFinish) return;
        if (_words.Count == 0)
            FinishLevel();
    }


    private async void FinishLevel()
    {
        Session.IsSelecting = false;

        var episode = GameDataService.GameData.Episode;
        var totalEpisodes = GameDataService.GameData.TotalEpisodes;

#if UNITY_EDITOR
        if (_useTestData)
        {
            episode = _editorEpisode;
            totalEpisodes = _editorTotalEpisodes;
        }
#endif
        var isStageCompleted = episode == totalEpisodes;

        _levelView.ShowFinishView(episode, totalEpisodes, isStageCompleted);

        _shouldCheckForFinish = false;
        HideGameObjects(false);

    }

    [ContextMenu("Finish Level")]
    private void FinishLevelImmediate()
    {
        FinishLevel();
    }


    private void OnApplicationPause(bool pause)
    {
        SaveState();
    }

    private void HandleBackClick()
    {
        AudioManager.Instance.PlaySound(Sound.Click);

        var isShopOpen = _levelView.HideShopView();
        if (isShopOpen)
        {
            Session.IsSelecting = true;
            HideGameObjects(true);
        }
        else
        {
            var adsController = AdsController.Instance;
            adsController.RemoveBanner();
            LevelStateService.SaveState();
            SceneManager.LoadScene(0);
        }
    }



    [ContextMenu("Save State")]
    private void SaveState()
    {
        LevelStateService.SaveState();
    }

    private void HandleLetterDeselect(LetterUnit letter, Vector2 triggerPos)
    {
        //        Debug.Log($"Handle letter deselect: {letter.Letter}");
        //Debug.Log($"letter is null : {letter == null}");
        // Debug.Log($"Level Logic handle letter deselecte");
        //  Debug.Log($"letter deselect: {letter.Letter}");
        _tryWordLetterUnits.Remove(letter);
        _tryWord = _tryWord.Substring(0, _tryWord.Length - 1);
        // Debug.Log($"tryWord deselect letter: {_tryWord}");
        //Debug.Log($"Letter deselect: {letter.Letter}");
        _levelView.RemoveLetter(letter.Letter);
        _audio.PlayLetter(_tryWordLetterUnits.Count);
        _lineProvider.RemovePoint(_tryWordLetterUnits.Count, triggerPos);
        letter.AnimateSelection(false);
    }


    private void HandleTriggerMove(Vector2 touchPos, Direction direction)
    {
        if (_tryWordLetterUnits.Count == 0) return;
        var lastLetter = _tryWordLetterUnits[^1];
        _lineProvider.Draw(touchPos, direction, lastLetter.transform.position, _tryWordLetterUnits.Count);
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
        if (isWord)
        {
            LevelStateService.State.OpenLetters.Add(_tryWordLetterUnits[0].Point);
            RemoveWord();
            LevelStateService.State.FirstLetters.Remove(_tryWordLetterUnits[0].Point);
        }
        else
            foreach (var letter in _tryWordLetterUnits)
                letter.AnimateSelection(false);

        _tryWordLetterUnits.Clear();

        _tryWord = string.Empty;

    }

    [ContextMenu("Log Found Letters")]
    private void LogFoundLetters()
    {
        foreach (var point in LevelStateService.State.FoundLetters)
            Debug.Log($"Found Letters: {point.GetVector()}");
    }

    [ContextMenu("Log Open letters")]
    private void LogOpenLetters()
    {
        foreach (var point in LevelStateService.State.OpenLetters)
            Debug.Log($"Open Letters: {point.GetVector()}");
    }

    private void RemoveWord()
    {
        _levelView.HideWord(_tryWord);
        _words.Remove(_tryWord);


        UpdateLightingAbility();



        foreach (var letter in _tryWordLetterUnits)
            letter.Disable();

        OnWordFound?.Invoke(_tryWordLetterUnits.Select(x => x.Point).ToList());
        var linePositions = new List<Vector2>() { _tryWordLetterUnits[0].transform.position, _tryWordLetterUnits[^1].transform.position };

        var lineState = new LineState(linePositions, _tryWordLetterUnits[0].GetColor());
        LevelStateService.AddFoundWord(_tryWord, _tryWordLetterUnits, lineState);

        _levelView.AnimateWord(_tryWordLetterUnits);
        ParticleProvider.IsAnimating = true;
        CheckFinishCondition();

    }

    private void UpdateLightingAbility()
    {
        if (_words.Count < 3)
            _levelView.DisableLighting();
    }

    [ContextMenu("Log First Active letters")]
    private void LogFirstActiveLetters()
    {
        Debug.Log($"Active First Letters: {LevelStateService.State.FirstLetters.Count}");
        foreach (var letter in LevelStateService.State.FirstLetters)
            Debug.Log($"Letter: {letter.GetVector()}");
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
                //                Debug.Log($"Letter already added: {letter.Letter}");
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

    internal void SetData(LevelData levelData)
    {
        HideGameObjects(true);
        var gameData = GameDataService.GameData;
        _words = levelData.Words.ToList();
        // Stage = levelData.Stage;
        //   Step = levelData.Step;
        TotalSteps = gameData.TotalEpisodes;
        _shouldCheckForFinish = true;

    }

    internal void SetState(LevelState levelState)
    {
        for (int i = 0; i < levelState.FoundWords.Count; i++)
        {
            var word = levelState.FoundWords[i];
            _words.Remove(word);
        }

    }
}
