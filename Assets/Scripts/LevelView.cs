using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelView : MonoBehaviour
{
    public static event Action NextLevelClicked;
    public static event Action<Vector2> OnWordFound;

    private VisualElement _root;
    private Label _levelTheme;
    private VisualElement _wordsHolder;
    private VisualElement _targetWord;
    private Label _targetLetters;
    private Label _levelLbl;
    private VisualElement _visualsDiv;
    private Image _overlayFx;
    private const string WORD_STYLE = "word";
    private const string WORD_DIV_STYLE = "word-div";
    [SerializeField] float _letterAnimStyle = 70;

    private const string WORD_BIG = "word-big";
    const string WORD_GRAY = "word-gray";
    [SerializeField] private int _removeWordStyleDelay = 300;
    private Dictionary<string, Label> _words = new();
    private Dictionary<Ability, VisualElement> _abilityBtns;

    [SerializeField] float _letterAnimDuration = 0.5f;


    [SerializeField] private RenderTexture _renderTexture;
    public static float RootHeight;
    private NavigationRow _navRow;
    private VisualElement _finishView;
    private Button _nextLvlBtn;
    private VisualElement _progressFill;
    private Label _progressLbl;
    private VisualElement _progressBg;
    [SerializeField] private float _fillDelay = 0.01f;
    private Coroutine _progressBarCoroutine;
    private VisualElement _shopBg;
    private Button _removeAdsBtn;

    private const string FINISH_VIEW_IN = "finish-view-in";
    private const string NEXT_LVL_BTN_ON = "next-lvl-btn-on";


    private const string TARGET_WORD_LEFT = "target-word-left";
    private const string TARGET_WORD_RIGHT = "target-word-right";

    [SerializeField] private ParticleSystem _winFX;
    [SerializeField] private Vector2 _fakePos;
    [SerializeField] private float _fakeLblHeight = 38;

    [SerializeField] private Camera _fxCam;

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _levelTheme = _root.Q<Label>("level-theme");
        _wordsHolder = _root.Q<VisualElement>("words-holder");

        _targetWord = _root.Q<VisualElement>("target-word");
        _targetLetters = _targetWord.Q<Label>("target-letters");

        _levelLbl = _root.Q<Label>("level-lbl");
        _visualsDiv = _root.Q<VisualElement>("visuals-div");
        _overlayFx = _root.Q<Image>("overlay-fx");

        InitRenderTexture();


        InitButtons();

        AbilityLogic.OnFakeLettersRemoved += DisableMagnet;
        LevelStateService.OnActiveFirstLetterRemoved += HandleFirstLettersRemoved;

        _root.RegisterCallbackOnce<GeometryChangedEvent>((e) =>
        {
            RootHeight = _root.layout.height;
        });

        _finishView = _root.Q<VisualElement>("finish-view");
        _nextLvlBtn = _finishView.Q<Button>("next-lvl-btn");
        _nextLvlBtn.clicked += HandleNextLvlBtn;

        _progressFill = _root.Q<VisualElement>("progress-fill");
        _progressLbl = _root.Q<Label>("progress-lbl");
        _progressBg = _root.Q<VisualElement>("progress-bg");


        _navRow = _root.Q<NavigationRow>();
        _navRow.InitBalance(AudioManager.Instance);
        //     _navRow.SetCoinsView(_root.Q<CoinsView>());
        _shopBg = _root.Q<VisualElement>("shop-bg");

        NavigationRow.OnShopBtnClicked += ShowShopBg;
        NavigationRow.OnShopHideClicked += HideShopBg;

        SetBackPicture();

        _removeAdsBtn = _root.Q<Button>("remove-ads-btn");

        if (Session.NoAds) RemoveAdsBtn();
        else _removeAdsBtn.clicked += InitNoAdsPurchase;
        Session.AdsRemoved += RemoveAdsBtn;

        var plateView = _root.Q<PlateView>();

        plateView.SubscribeToSettingsClick();

        _root.RegisterCallback<DetachFromPanelEvent>(e => Unsubscribe());

        LevelLogic.WordListUpdated += HandleWordListUpdated;
    }

    private void HandleWordListUpdated(int words)
    {
        if (words < 3)
        {
            _abilityBtns[Ability.Lighting].SetEnabled(false);
        }
    }

    private void OnDestroy()
    {


    }

    private void InitNoAdsPurchase()
    {
        AudioManager.Instance.PlaySound(Sound.Click);
        ShopView shopView = _root.Q<ShopView>();
        shopView.InitRemoveAds();
    }

    private void RemoveAdsBtn()
    {
        _removeAdsBtn.Toggle(false);
    }

    private void ShowShopBg()
    {
        _shopBg.Toggle(true);
    }

    public void SetBackPicture()
    {
        var bgController = GameObject.Find("BgController").GetComponent<BgController>();
        var bgStyle = new StyleBackground(bgController.GetBackView());
        _shopBg.style.backgroundImage = bgStyle;
        _finishView.style.backgroundImage = bgStyle;
    }



    private void HideShopBg()
    {
        _shopBg.Toggle(false);
    }

    public void ShowFinishView()
    {
        Debug.Log($"show finish view");
        _progressLbl.text = $"{LevelLogic.Step}/{LevelLogic.TotalSteps}";
        _finishView.Toggle(true);

        _finishView.RegisterCallbackOnce<TransitionEndEvent>(e =>
        {
            if (_progressBarCoroutine != null) return;
            _progressBarCoroutine = StartCoroutine(FillProgressBar(LevelLogic.Step, LevelLogic.TotalSteps));
        });

        _finishView.AddToClassList(FINISH_VIEW_IN);
        if (LevelLogic.Step == LevelLogic.TotalSteps)
            ShowStageCompleted();

    }

    [ContextMenu("Show Stage Completed")]
    private void ShowStageCompleted()
    {
        Debug.Log($"Show Stage Completed");
        AudioManager.Instance.PlaySound(Sound.StageCompleted);
        _winFX.Play();
        Session.LastStage++;
        //_nextLvlBtn.SetEnabled(false);

    }

    [ContextMenu("Show Finish View")]
    public void ShowFinish()
    {
        ShowFinishView();
    }



    private IEnumerator FillProgressBar(int step, int totalsteps)
    {
        var targetFill = 100 * ((float)step / (float)totalsteps);
        var currentFill = _progressFill.style.width.value.value;
        Debug.Log($"targetFill: {targetFill}");
        while (currentFill < targetFill)
        {
            currentFill += 1f;
            _progressFill.style.width = new StyleLength(new Length(currentFill, LengthUnit.Percent));
            yield return new WaitForSeconds(_fillDelay);
        }
        ShowNextLvlBtn();
    }

    private void ShowNextLvlBtn()
    {
        _nextLvlBtn.AddToClassList(NEXT_LVL_BTN_ON);
        _nextLvlBtn.SetEnabled(true);

    }

    private void HandleNextLvlBtn()
    {
        AudioManager.Instance.PlaySound(Sound.Click);
        NextLevelClicked?.Invoke();
        _finishView.Toggle(false);
        _finishView.RemoveFromClassList(FINISH_VIEW_IN);
        _progressBarCoroutine = null;
        _progressFill.style.width = new StyleLength(new Length(0, LengthUnit.Percent));
        SetBackPicture();



    }

    private void InitRenderTexture()
    {
        _overlayFx.SetRenderTexture(_fxCam);
    }

    private void HandleFirstLettersRemoved(int lettersLeft)
    {
        if (lettersLeft < 3 && _abilityBtns[Ability.Lighting].enabledInHierarchy)
            _abilityBtns[Ability.Lighting].SetEnabled(false);
        if (lettersLeft == 0 && _abilityBtns[Ability.Hint].enabledInHierarchy)
            _abilityBtns[Ability.Hint].SetEnabled(false);


    }

    private void DisableMagnet()
    {
        _abilityBtns[Ability.Magnet].SetEnabled(false);
    }

    [ContextMenu("Reset Btns")]
    private void ResetBtns()
    {
        foreach (var pair in _abilityBtns)
        {

            pair.Value.SetEnabled(true);
        }
    }

    public void SetLevelData(LevelData data)
    {
        _levelTheme.text = data.Subject;
        FillWords(data.Words);
        _levelLbl.text = $"Level: {data.Level}";

        ResetBtns();

        // country
        // level
        // words
    }

    public void AnimateWord(List<LetterUnit> letterUnits)
    {
        var key = new string(letterUnits.Select(x => x.Letter).ToArray());
        var word = _words[key];

        for (int i = 0; i < letterUnits.Count; i++)
        {
            var letter = letterUnits[i];
            var viewPos = Camera.main.WorldToScreenPoint(letter.transform.position);
            var letterLbl = new Label(letter.Letter.ToString());

            letterLbl.style.fontSize = new StyleLength(_letterAnimStyle);
            _visualsDiv.Add(letterLbl);
            letterLbl.style.position = Position.Absolute;

            var xStep = word.layout.width / (letterUnits.Count + 1);
            var index = i;

            letterLbl.RegisterCallbackOnce<GeometryChangedEvent>(e =>
            {
                var height = letterLbl.layout.height;
                var width = letterLbl.layout.width;


                letterLbl.style.top = RootHeight - (viewPos.y + height / 1.5f);
                letterLbl.style.left = viewPos.x - width / 2;

                Vector2 targetPos;
                targetPos.y = word.worldBound.position.y;

                targetPos.x = word.worldBound.position.x + xStep * index;

                MoveLetter(letterLbl, targetPos);
            });

        }


    }

    private void MoveLetter(Label letterLbl, Vector2 targetPos)
    {

        var startPos = letterLbl.layout.position;
        var endPos = targetPos;

        var x = endPos.x;
        var y = endPos.y - letterLbl.layout.height;
        var fontSize = 34;

        // Debug.Log($"Letter:{letterLbl.text} from {startPos} to {endPos} ");

        DOTween.To(() => letterLbl.style.left.value.value, x => letterLbl.style.left = x, endPos.x, _letterAnimDuration);
        DOTween.To(() => letterLbl.style.top.value.value, y => letterLbl.style.top = y, endPos.y, _letterAnimDuration);
        DOTween.To(() => letterLbl.style.fontSize.value.value, fontSize => letterLbl.style.fontSize = fontSize, fontSize, _letterAnimDuration).OnComplete(() =>
        {
            letterLbl.RemoveFromHierarchy();
            //  endPos.y -= letterLbl.layout.height / 3;
            endPos.x += letterLbl.layout.width * 2;
            endPos.y += letterLbl.layout.height / 2;

            var worldPos = letterLbl.GetWorldPosition(_root);
            OnWordFound?.Invoke(worldPos);
        });

    }

    [ContextMenu("InstantiateFakeFX")]
    private void PlayWordFoundFX()
    {
        _fakePos.y -= _fakeLblHeight;

        var worldPos = Camera.main.ScreenToWorldPoint(new Vector2(_fakePos.x, LevelView.RootHeight - _fakePos.y));
        worldPos.z = 0;

        // var worldPos = Camera.main.ScreenToWorldPoint(new Vector2(_fakePos.x, _fakePos.y+));
        // go.transform.position = worldPos;

        OnWordFound?.Invoke(worldPos);
    }


    private void InitButtons()
    {
        _abilityBtns = new()
        {
            {Ability.Lighting, _root.Q<AbilityBtn>("lighting-btn")},
            {Ability.Hint, _root.Q<AbilityBtn>("hint-btn")},
            {Ability.Magnet, _root.Q<AbilityBtn>("magnet-btn")},
            {Ability.Firework, _root.Q<AbilityBtn>("firework-btn")},
            {Ability.Ads, _root.Q<AbilityBtn>("ads-btn")}
        };
    }


    private void FillWords(List<string> words)
    {
        _wordsHolder.Clear();

        foreach (var word in words)
        {
            var div = new VisualElement();
            div.AddToClassList(WORD_DIV_STYLE);
            var label = new Label(word);
            label.AddToClassList(WORD_STYLE);
            div.Add(label);
            _wordsHolder.Add(div);
            _words[word] = label;
        }
    }

    public void AddLetter(char letter)
    {
        _targetLetters.text += letter;
    }

    public void RemoveLetter(char letter)
    {
        Debug.Log($"Level View remove letter is null: {letter == '\0'}");
        if (letter == '\0') return;
        _targetLetters.text = _targetLetters.text.Remove(_targetLetters.text.Length - 1);
    }

    public void ToggleWord(bool show, Color color = default)
    {
        _targetWord.Toggle(show);
        if (!show) _targetLetters.text = string.Empty;
        _targetWord.style.backgroundColor = color;
    }

    public async void HideWord(string word)
    {
        await Task.Delay((int)_letterAnimDuration * 1000);
        var label = _words[word];
        if (label.ClassListContains(WORD_GRAY)) return;

        label.AddToClassList(WORD_BIG);
        label.AddToClassList(WORD_GRAY);
        await Task.Delay(_removeWordStyleDelay);
        // RequestWordFX(label);
        label.RemoveFromClassList(WORD_BIG);
    }

    private void RequestWordFX(Label label)
    {
        var worldPos = label.worldTransform.GetPosition();
        var bounds = label.worldBound;
        var finalPos = Camera.main.ScreenToWorldPoint(worldPos + new Vector3(bounds.width / 2, -bounds.height / 4));
        OnWordFound?.Invoke(finalPos);

    }

    internal void SetState(LevelState levelState)
    {
        var totalWords = _words.Count;
        var foundWords = levelState.FoundWords.Count;
        var leftWords = totalWords - foundWords;
        var activeFirstLetters = levelState.FirstLetters.Count;

        if (leftWords >= 3 && activeFirstLetters >= 3)
            _abilityBtns[Ability.Lighting].SetEnabled(true);
        else
            _abilityBtns[Ability.Lighting].SetEnabled(false);

        if (leftWords == 0 || activeFirstLetters == 0)
            _abilityBtns[Ability.Hint].SetEnabled(false);
        else
            _abilityBtns[Ability.Hint].SetEnabled(true);

        if (levelState.FoundWords.Count > 0)
            foreach (var word in levelState.FoundWords)
                HideWord(word);




    }

    internal void AnimateHideWord(bool isWord)
    {

        if (isWord)
        {

            ToggleWord(false);
        }
        else
        {
            StartCoroutine(AnimatingHideWord());
        }
    }

    private IEnumerator AnimatingHideWord()
    {
        _targetWord.AddToClassList(TARGET_WORD_LEFT);
        yield return new WaitForSeconds(0.15f);
        _targetWord.RemoveFromClassList(TARGET_WORD_LEFT);
        _targetWord.AddToClassList(TARGET_WORD_RIGHT);
        yield return new WaitForSeconds(0.15f);
        _targetWord.RemoveFromClassList(TARGET_WORD_RIGHT);
        yield return new WaitForSeconds(0.15f);
        ToggleWord(false);

    }

    private void Unsubscribe()
    {
        _navRow.Unsubscribe();

        AbilityLogic.OnFakeLettersRemoved -= DisableMagnet;
        LevelStateService.OnActiveFirstLetterRemoved -= HandleFirstLettersRemoved;
        Session.AdsRemoved -= RemoveAdsBtn;
        NavigationRow.OnShopBtnClicked -= ShowShopBg;
        NavigationRow.OnShopHideClicked -= HideShopBg;
        RootHeight = 0;
        LevelLogic.WordListUpdated -= HandleWordListUpdated;

    }
}
