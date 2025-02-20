using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLogic : MonoBehaviour
{
    [SerializeField] LevelView _levelView;
    [SerializeField] LineProvider _lineProvider;
    [SerializeField] private int _prizeAmount = 25;
    [SerializeField] private ParticleSystem _winFX;
    [SerializeField] AdsController _adsController;

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

    [SerializeField] private float _msForWord = 8000f;
    private bool _isTimerRunning;
    private Coroutine _timerCoroutine;
    [SerializeField] private float _timerStep = 0.1f;
    private string _targetWord;
    [SerializeField] private float _pauseTimerDelay = 5;
    private float _timeLeft;
    [SerializeField] private int _finishDelay = 2000;

    private void Start()
    {
        InputHandler.OnInputStop += CheckWord;
        InputTrigger.OnLetterEnter += HandleLetterEnter;
        InputHandler.OnTriggerMove += HandleTriggerMove;
        InputHandler.OnLetterDeselect += HandleLetterDeselect;

        _audio = AudioManager.Instance;
        // _inputHandler.SetLetterUnits(_tryWordLetterUnits);
        _shouldCheckForFinish = true;

        LevelView.OnGameViewToggle += ToggleGameObjects;
        LevelView.OnBackClicked += HandleBackClick;
        AbilityLogic.OnFreezeRequested += FreezeTimer;
        LevelView.OnSettingsClicked += HandleSettingsClick;
        PlateView.OnCloseClicked += HandleCloseClicked;
        ShopView.OnShopClicked += HandleShopClick;
        LevelView.OnPauseNeeded += HandlePauseNeeded;

        LevelView.FinishLevelClicked += FinishLevelImmediate;

        if (Services.HasFullAccess())
        {
            _levelView.ShowControlBtns();
        }

    }

    private void OnDestroy()
    {
        InputHandler.OnInputStop -= CheckWord;
        InputTrigger.OnLetterEnter -= HandleLetterEnter;
        InputHandler.OnTriggerMove -= HandleTriggerMove;
        InputHandler.OnLetterDeselect -= HandleLetterDeselect;
        LevelView.OnGameViewToggle -= ToggleGameObjects;
        AbilityLogic.OnFreezeRequested -= FreezeTimer;
        LevelView.OnSettingsClicked -= HandleSettingsClick;
        ShopView.OnShopClicked -= HandleShopClick;
        LevelView.OnPauseNeeded -= HandlePauseNeeded;
        LevelView.OnBackClicked -= HandleBackClick;
        PlateView.OnCloseClicked -= HandleCloseClicked;
        LevelView.FinishLevelClicked -= FinishLevelImmediate;
        _adsController.HideBanner();

    }

    private void HandlePauseNeeded(bool obj)
    {
        if (obj) PauseTimer();
        else ContinueTimer();
    }

    private void HandleShopClick(bool obj)
    {
        if (obj) PauseTimer();
        else ContinueTimer();
    }

    private void HandleCloseClicked()
    {
        ContinueTimer();
    }

    private void HandleSettingsClick()
    {
        Session.IsSelecting = false;
        PauseTimer();
        _levelView.ShowSettings();
    }

    private void FreezeTimer()
    {
        StartCoroutine(FreezingTimer());
    }

    private IEnumerator FreezingTimer()
    {
        PauseTimer();
        yield return new WaitForSeconds(_pauseTimerDelay);
        AudioManager.Instance.PlaySound(Sound.TimerContinue);
        _levelView.BlinkTimer();
        yield return new WaitUntil(() => !_levelView.IsBlinking);

        ContinueTimer();
    }
    private void PauseTimer()
    {
        _isTimerRunning = false;
    }

    private void ContinueTimer()
    {
        _isTimerRunning = true;
    }

    public void ToggleGameObjects(bool isVisible)
    {
        if (_canvas == null) _canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        _canvas.sortingOrder = isVisible ? 0 : 999;
    }




    [ContextMenu("Check if level done")]
    private async void CheckFinishConditionAsync()
    {
        if (!_shouldCheckForFinish) return;
        if (_words.Count == 0)
        {
            if (Session.IsClassicGame) await Task.Delay(_finishDelay);
            FinishLevel();
        }
        else if (!Session.IsClassicGame)
        {
            SetTimeWord();
            ResetTimer();
            if (!_isTimerRunning)
                return;
            StopTimer();
            StartTimer();
        }
    }


    private async void FinishLevel()
    {

        if (!Session.IsClassicGame)
            StopTimer();
        Session.IsSelecting = false;
        var finishedEpisode = ProgressService.Progress.Episode;
        var totalEpisodes = ProgressService.Progress.TotalEpisodes;

#if UNITY_EDITOR
        if (_useTestData)
        {
            finishedEpisode = _editorEpisode;
            totalEpisodes = _editorTotalEpisodes;
        }
#endif
        var isStageCompleted = finishedEpisode == totalEpisodes;
        ToggleGameObjects(false);
        await _levelView.ShowFinishView(finishedEpisode, totalEpisodes);

        if (isStageCompleted)
        {
            var season = ProgressService.Progress.Season;
            var title = ProgressService.GetStampTitle(season);
            var stampPic = Extensions.GetTexture(season + 1);
            await _levelView.ShowStageFinish(_prizeAmount, stampPic, title);
            AudioManager.Instance.PlaySound(Sound.StageCompleted);
            Balance.AddBalance(_prizeAmount);
            _winFX.Play();
        }
        await ProgressService.FinishLevel();
        _levelView.ShowNextLvlBtn();
        _shouldCheckForFinish = false;

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

    private async void HandleBackClick()
    {
        AudioManager.Instance.PlaySound(Sound.Click);

        var isShopOpen = _levelView.IsShopOpen;
        if (isShopOpen)
        {
            Session.IsSelecting = true;

            await _levelView.HideShopView();
            ToggleGameObjects(true);
        }
        else
        {
            var adsController = AdsController.Instance;
            adsController.HideBanner();
            LevelStateService.SaveState();
            await Resources.UnloadUnusedAssets();
            SceneManager.LoadScene(0);
        }
    }




    private void SaveState()
    {
        if (Session.IsClassicGame)
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

        var isWord = Session.IsClassicGame ? _words.Contains(_tryWord) : _tryWord == _targetWord;

        _lineProvider.FinishDraw(isWord, _tryWordLetterUnits);
        var sound = isWord ? Sound.WordFound : Sound.WrongWord;

        _levelView.AnimateHideWord(isWord);
        _audio.PlaySound(sound);
        _isFirstLetter = true;
        if (isWord)
        {
            RemoveWord();
            LevelStateService.AddFoundWord(_tryWord, _tryWordLetterUnits, LineProvider.LastColor);

        }
        else
            foreach (var letter in _tryWordLetterUnits)
                letter.AnimateSelection(false);

        _tryWordLetterUnits.Clear();

        _tryWord = string.Empty;


    }

    [ContextMenu("Add Word")]
    private void AddWord()
    {
        LevelStateService.AddWord(_tryWord);
    }

    private void RemoveWord()
    {
        _levelView.HideWord(_tryWord);
        _words.Remove(_tryWord);
        UpdateLightingAbility();

        foreach (var letter in _tryWordLetterUnits)
            letter.Disable();

        var linePositions = new List<Vector2>() { _tryWordLetterUnits[0].transform.position, _tryWordLetterUnits[^1].transform.position };


        if (Session.IsClassicGame)
        {
            _levelView.AnimateWord(_tryWordLetterUnits);
        }
        CheckFinishConditionAsync(); //TODO: should be done outside of this method

    }

    private void UpdateLightingAbility()
    {
        if (_words.Count < 3)
            _levelView.DisableLighting();
    }


    private void HandleLetterEnter(LetterUnit letter)
    {
        letter.AnimateSelection(true);
        letter.AnimateScale();
        if (_isFirstLetter)
        {
            var revealedLetter = LevelStateService.State.RevealedFirstLetters.FirstOrDefault(x => x.Point == letter.Point);
            var color = revealedLetter == null ? default : revealedLetter.Color;
            color = _lineProvider.CreateLine(letter.transform.position, color);
            _tryWord = letter.Letter.ToString();
            _tryWordLetterUnits.Add(letter);
            _isFirstLetter = false;
            _levelView.AddLetter(letter.Letter);
            _levelView.ToggleWord(true, color);
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
        ToggleGameObjects(true);
        var gameData = ProgressService.Progress;
        _words = levelData.Words.Select(x => x.Word).ToList();
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
            _words.Remove(word.Word);
        }
    }

    public void SetTimeMode()
    {
        _levelView.SetTimeMode();

        Session.IsSelecting = true;
        SetTimeWord();
        StartTimer();
    }

    private void SetTimeWord()
    {
        _targetWord = GetRandomWord();
        _levelView.SetTimeWord(_targetWord);
    }

    private string GetRandomWord()
    {
        var rIndex = UnityEngine.Random.Range(0, _words.Count);
        return _words[rIndex];
    }

    private void StartTimer()
    {
        ContinueTimer();
        _timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    private void StopTimer()
    {
        PauseTimer();
        StopCoroutine(_timerCoroutine);
        _timerCoroutine = null;
    }

    private IEnumerator TimerCoroutine()
    {
        _levelView.UpdateTimer(100);
        _timeLeft = _msForWord;

        while (true)
        {
            while (!_isTimerRunning)
            {
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(_timerStep/3);
            _timeLeft -= 1000 * _timerStep/3;
            var percent = (_timeLeft / _msForWord) * 100;
            _levelView.UpdateTimer(percent);
            CheckTimeCondition(_timeLeft);

        }
    }

    private void ResetTimer()
    {
        _timeLeft = _msForWord;
        _levelView.UpdateTimer(100);
    }

    private void CheckTimeCondition(float timeLeft)
    {
        if (timeLeft <= 0)
        {
            _isTimerRunning = false;
            _levelView.UpdateTimer(0);
            StopTimer();
            _levelView.ShowTimeOver();
            Session.IsSelecting = false;
            _lineProvider.FinishDraw(false);
        }
    }
}
