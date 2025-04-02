using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using System.Collections;
using System.Collections.Specialized;
using UnityEngine.SceneManagement;


public class LevelView : MonoBehaviour, IAdsRequest
{
    public static event Action NextLevelClicked;
    public static event Action<Vector2> OnWordFound;
    public static event Action OnBackClicked;

    public static event Action FinishLevelClicked;

    private VisualElement _root;
    private Label _levelTheme;
    private VisualElement _wordsHolder;
    private VisualElement _targetWord;
    private PlateView _plateView;
    private Button _settingsBtn;
    private Label _targetLetters;
    private Label _levelLbl;
    private VisualElement _visualsDiv;
    private Image _overlayFx;
    private VisualElement _giftPic;
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
    private VisualElement _finishView;
    private Button _nextLvlBtn;
    private VisualElement _progressFill;
    private VisualElement _progressBack;
    private Label _progressLbl;
    private VisualElement _cup;
    private VisualElement _progressDiv;
    private VisualElement _stampDiv;
    private VisualElement _stampPic;
    private Label _stampTitleLbl;
    private VisualElement _levelView;
    [SerializeField] private float _fillDelay = 0.1f;
    [SerializeField] private float _fillAdd = 1f;
    private bool _progressBarCoroutine;
    private Button _removeAdsBtn;

    private const string NEXT_LVL_BTN_ON = "next-lvl-btn-on";

    private const string TARGET_WORD_LEFT = "target-word-left";
    private const string TARGET_WORD_RIGHT = "target-word-right";

    [SerializeField] private Vector2 _fakePos;
    [SerializeField] private float _fakeLblHeight = 38;

    [SerializeField] private Camera _fxCam;

    CoinsView _coinsView;
    private VisualElement _gameOverView;
    [SerializeField] private CoinsFX_Handler _coinsFX_Handler;
    [SerializeField] private int _prizeCoinsDelay = 100;
    [SerializeField] private float _rewardDelay = 0.5f;
    [SerializeField] private Vector2 _startPos;
    [SerializeField] private int _fxScale = 2;
    private ShopView _shopView;
    private Button _checkUpdatesBtn;
    private Button _adsBtn;
    private Button _backBtn;
    private VisualElement _blurPnl;
    private ShopBtn _shopBtn;

    private VisualElement _topRow;

    public static event Action<bool> OnGameViewToggle;

    public IShopItems ShopItems
    {
        get { return _shopView; }
        set { _shopView = value as ShopView; }
    }

    const string FINISH_VIEW = "finish-view";
    const string HIDE_STYLE = "hide-style";

    private bool _isSliderDone;
    private VisualElement _classicDiv;
    private VisualElement _timeDiv;
    private Label _timeWordLbl;
    private VisualElement _timeFill;
    [SerializeField] private Color _blinkColor = new(255, 86, 79);

    private bool _isBlinking;
    [SerializeField] private int _playAwardDelay = 2000;
    [SerializeField] private int _sliderFillDelay = 1;
    private Button _finishBtn;
    private MessageView _messageView;

    public bool IsBlinking
    {
        get { return _isBlinking; }
    }

    public static event Action<bool> OnPauseNeeded;

    public static event Action OnSettingsClicked;

    private void Awake()
    {

        _root = GetComponent<UIDocument>().rootVisualElement;

        _topRow = _root.Q<VisualElement>("top-row");
        _levelTheme = _root.Q<Label>("level-theme");
        _wordsHolder = _root.Q<VisualElement>("words-holder");

        _targetWord = _root.Q<VisualElement>("target-word");
        _targetWord.Toggle(false);
        _targetLetters = _targetWord.Q<Label>("target-letters");
        _targetLetters.text = "";


        _levelLbl = _root.Q<Label>("level-lbl");
        _visualsDiv = _root.Q<VisualElement>("visuals-div");
        _visualsDiv.Toggle(true);
        _overlayFx = _root.Q<Image>("overlay-fx");
        _overlayFx.Toggle(true);
        _giftPic = _root.Q<VisualElement>("giftPic");

        _coinsView = _root.Q<CoinsView>();
        _gameOverView = _root.Q<VisualElement>("game-over-view");
        _messageView = _root.Q<MessageView>();

        InitRenderTexture();


        InitButtons();

        AbilityLogic.OnFakeLettersRemoved += DisableMagnet;
        LevelStateService.OnActiveFirstLetterRemoved += HandleFirstLettersRemoved;

        _root.RegisterCallbackOnce<GeometryChangedEvent>((e) =>
        {
            RootHeight = _root.layout.height;
        });



        _levelView = _root.Q<VisualElement>("level-view");

        _shopView = _root.Q<ShopView>();

        _checkUpdatesBtn = _root.Q<Button>("check-updates-btn");
        _removeAdsBtn = _root.Q<Button>("remove-ads-btn");

        if (ProgressService.AdsRemoved) RemoveAdsBtn();
        else _removeAdsBtn.clicked += InitNoAdsPurchase;
        EventManager.AdsRemoved += RemoveAdsBtn;

        _plateView = _root.Q<PlateView>();
        _plateView.Toggle(true);

        _settingsBtn = _root.Q<Button>("settings-btn");
        _settingsBtn.clicked += HandleSettingsClick;

        _root.RegisterCallback<DetachFromPanelEvent>(e => Unsubscribe());

        AdsEventManager.RewardedAdWatched += RewardForAds;

        IAPManager.CoinPurchased += AnimatePurchasedCoins;

        _adsBtn = _root.Q<Button>("ads-btn");
        _adsBtn.clicked += RequestAds;

        _backBtn = _root.Q<Button>("back-btn");
        _backBtn.clicked += () => OnBackClicked?.Invoke();
        _blurPnl = _root.Q<VisualElement>("blur-pnl");
        _plateView.SetBlur(_blurPnl);

        InitShopBtn();
        InitFinishView();
        InitGameModes();


        EventManager.RemoveAdsClicked += ShowPurchaseMessage;
        EventManager.PurchaseClicked += ShowPurchaseMessage;
    }

    private void ShowPurchaseMessage(int obj)
    {
        _messageView.ShowMessage("Processing purchase...");

    }

    private void ShowPurchaseMessage()
    {
        _messageView.ShowMessage("Processing purchase...");
    }

    private void HandleFirstLettersRemoved(int amount)
    {
        if (amount < 3)
            _abilityBtns[Ability.Lighting].SetEnabled(false);
        if (amount == 0)
            _abilityBtns[Ability.Hint].SetEnabled(false);

    }

    public void ShowControlBtns()
    {
        _finishBtn = _root.Q<Button>("finish-btn");
        _finishBtn.Toggle(true);
        _finishBtn.clicked += () => FinishLevelClicked?.Invoke();
    }

    private void InitGameModes()
    {
        _classicDiv = _root.Q<VisualElement>("classic-div");
        _timeDiv = _root.Q<VisualElement>("time-div");
        _timeWordLbl = _root.Q<Label>("time-word-lbl");
        _timeFill = _root.Q<VisualElement>("time-fill");
    }

    public void UpdateTimer(float percent)
    {
        _timeFill.style.width = new StyleLength(new Length(percent, LengthUnit.Percent));

        //lerp it

    }

    private void InitFinishView()
    {
        _finishView = _root.Q<VisualElement>("finish-view");
        _finishView.AddToClassList(HIDE_STYLE);
        _nextLvlBtn = _finishView.Q<Button>("next-lvl-btn");
        _nextLvlBtn.RemoveFromClassList(NEXT_LVL_BTN_ON);
        _nextLvlBtn.clicked += HandleNextLvlClick;

        _progressFill = _root.Q<VisualElement>("progress-fill");
        _progressBack = _root.Q<VisualElement>("progress-back");
        _progressLbl = _root.Q<Label>("progress-lbl");
        _cup = _root.Q<VisualElement>("cup");

        _progressDiv = _root.Q<VisualElement>("progress-div");
        _stampDiv = _root.Q<VisualElement>("stamp-div");
        _stampPic = _root.Q<VisualElement>("stamp-pic");
        _stampTitleLbl = _root.Q<Label>("stamp-title-lbl");
    }


    private void HandleSettingsClick()
    {
        OnSettingsClicked?.Invoke();
    }
    public void ShowSettings()
    {
        _plateView.ShowPlate(Plate.Settings);
        _plateView.PlaceInFront(_shopView);
        _blurPnl.PlaceBehind(_plateView);
    }

    private void InitShopBtn()
    {
        _shopBtn = _root.Q<ShopBtn>();
        _shopBtn.SetRoot(_root);
        _shopBtn.InitBalance(ProgressService.Progress.Coins);
        ShopBtn.ShopClicked += ShowShopView;
        _shopBtn.RegisterCallback<GeometryChangedEvent>(SetCoinsAnimation);
    }

    private async void ShowShopView()
    {
        Session.IsSelecting = false;
        _levelView.Toggle(false);
        while (_levelView.resolvedStyle.width > 0)
            await Task.Yield();
        _shopView.Show();
        OnGameViewToggle?.Invoke(false);

    }

    public void RequestAds()
    {
        Session.IsSelecting = false;
        _plateView.ShowPlate(Plate.Ads);
        OnPauseNeeded?.Invoke(true);
    }

    public void ShowGameOver()
    {

        _levelView.Toggle(false);

        _finishView.Toggle(false);

        _gameOverView.Toggle(true);
        _checkUpdatesBtn.clicked += () => Application.OpenURL($"itms-apps://itunes.apple.com/app/id{Session.AppId}");

    }

    private void AnimatePurchasedCoins(int payout)
    {
        _messageView.HideMessage();
        Debug.Log($"Animate Purchased Coins: {payout}");
        PlayAward((int)payout, _shopView.BuyBtn, _shopView.BuyBtn, true);
        _coinsView.HideAsync();
        _shopView.BuyBtn = null;

    }

    private void RewardForAds(IAdsProvider provider)
    {
        _messageView.HideMessage();
        PlayAward(Session.RewardAmount, _adsBtn, _adsBtn);
        Balance.AddBalance(Session.RewardAmount);
        Session.IsSelecting = true;
        OnPauseNeeded?.Invoke(false);
    }

    public void DisableLighting()
    {
        _abilityBtns[Ability.Lighting].SetEnabled(false);
    }

    private void SetCoinsAnimation(GeometryChangedEvent evt)
    {
        var target = evt.target as VisualElement;
        var worldPos = target.GetWorldPosition(_root);
        _coinsFX_Handler.SetForceField(worldPos, _fxScale);
    }

    private void OnDestroy()
    {

        AdsEventManager.RewardedAdWatched -= RewardForAds;
        IAPManager.CoinPurchased += AnimatePurchasedCoins;

        ShopBtn.ShopClicked -= ShowShopView;

    }

    [ContextMenu("Play coins FX")]
    public void PlayCoinsFX()
    {
        _coinsFX_Handler.PlayCoinsFX(_startPos, _fxScale);
    }


    public void PlayAward(int prizeAmount, VisualElement coinsLblRefPos, VisualElement fxStartPos, bool showLbl = true)
    {
        Debug.Log($"Play Award: {prizeAmount}");
        var prize = prizeAmount;
        var element = coinsLblRefPos;
        var pos = element.worldBound.position;
        if (showLbl) _coinsView.ShowCoinsLbl(pos, prize);

        AudioManager.Instance.PlaySound(Sound.Coins);
        var worldPos = fxStartPos.GetWorldPosition(_root);
        _coinsView.HideAsync();
        _coinsFX_Handler.PlayCoinsFX(worldPos, _fxScale);
    }



    private void InitNoAdsPurchase()
    {
        AudioManager.Instance.PlaySound(Sound.Click);
        _shopView.InitRemoveAds();
    }

    private void RemoveAdsBtn()
    {
        _removeAdsBtn.Toggle(false);
    }



    public async Task ShowFinishView(int episode, int totalEpisodes)
    {
        Debug.Log($"show finish view");
        _backBtn.Toggle(false);

        AdsController.Instance.HideBanners();

        _progressDiv.Toggle(true);

        _isSliderDone = false;
        _levelView.Toggle(false);

        _progressLbl.text = $"{episode}/{totalEpisodes}";

        _finishView.Toggle(true);
        await Task.Delay(100);

        _finishView.RemoveFromClassList(HIDE_STYLE);

        while (_finishView.resolvedStyle.opacity < 0.9f)
            await Task.Yield();
        AnimateProgressBar(episode, totalEpisodes);

        while (!_isSliderDone)
            await Task.Yield();
        _progressBarCoroutine = false;

    }



    private void AnimateProgressBar(int episode, int totalEpisodes)
    {
        if (_progressBarCoroutine) return;
        //  _progressBarCoroutine = StartCoroutine(FillProgressBar(episode, totalEpisodes));
        FillProgressBar(episode, totalEpisodes);
        _progressBarCoroutine = true;

    }

    public void InitProgressBar(int episode, int totalEpisodes)
    {
        var currentFill = 100 * (((float)episode - 1) / (float)totalEpisodes);
        _progressFill.style.width = new StyleLength(new Length(currentFill, LengthUnit.Percent));
        _nextLvlBtn.RemoveFromClassList(NEXT_LVL_BTN_ON);
    }

    private async void FillProgressBar(int episode, int totalEpisodes)
    {

        _progressBarCoroutine = true;

        var targetFill = 100 * ((float)episode / (float)totalEpisodes);
        var currentFill = _progressFill.style.width.value.value;

        while (_finishView.resolvedStyle.opacity < 0.9f)
        {
            // yield return new WaitForEndOfFrame();
            await Task.Yield();
            Debug.Log($"Opacity: {_finishView.resolvedStyle.opacity}");
        }
        StyleLength newWidth = _progressFill.style.width;
        while (newWidth.value.value < targetFill)
        {
            currentFill += _fillAdd;

            newWidth = new StyleLength(new Length(currentFill, LengthUnit.Percent));
            _progressFill.style.width = newWidth;
            await Task.Delay(_sliderFillDelay);
        }
        _isSliderDone = true;
    }

    public async Task ShowStageFinish(int prizeAmount, Texture2D stampPic, string stampTitle)
    {
        PlayAward(prizeAmount, _giftPic, _cup);

        await Task.Delay(_playAwardDelay);

        _progressDiv.AddToClassList(HIDE_STYLE);
        _stampDiv.AddToClassList(HIDE_STYLE);
        while (_progressDiv.resolvedStyle.opacity > 0.1f)
            await Task.Yield();
        _progressDiv.Toggle(false);

        _stampDiv.Toggle(true);
        _stampDiv.RemoveFromClassList(HIDE_STYLE);
        _stampPic.style.backgroundImage = new StyleBackground(stampPic);
        _stampTitleLbl.text = stampTitle;

    }

    public void ShowNextLvlBtn()
    {
        _nextLvlBtn.AddToClassList(NEXT_LVL_BTN_ON);
        _nextLvlBtn.SetEnabled(true);
    }

    private async void HandleNextLvlClick()
    {
        AudioManager.Instance.PlaySound(Sound.Click);
        _stampDiv.Toggle(false);
        HideFinishView();
        while (_finishView.resolvedStyle.display == DisplayStyle.Flex)
            await Task.Yield();
        NextLevelClicked?.Invoke();
        _progressDiv.Toggle(true);
        _progressDiv.RemoveFromClassList(HIDE_STYLE);

    }

    public void HideFinishView()
    {
        _finishView.Toggle(false);
        _finishView.AddToClassList(HIDE_STYLE);
        _progressBarCoroutine = false;
        _progressFill.style.width = new StyleLength(new Length(0, LengthUnit.Percent));

    }

    private void InitRenderTexture()
    {
        _overlayFx.SetRenderTexture(_fxCam);
    }

    private void UpdateRevealButtons(int lettersLeft)
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
        _finishView.Toggle(false);
        _finishView.AddToClassList(HIDE_STYLE);
        _levelView.Toggle(true);
        if (Session.IsClassicGame)
        {
            var words = data.Words.Select(x => x.Word).ToList();
            FillWords(words);
            _abilityBtns[Ability.Freeze].Toggle(false);
        }


        _levelLbl.text = $"Level: {ProgressService.Progress.Level}";
        ResetBtns();
        _backBtn.Toggle(true);
        //SetBackPicture();

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
            {Ability.Freeze, _root.Q<AbilityBtn>("freeze-btn")}
        };
    }

    private void FillWords(List<string> words)
    {
        _wordsHolder.Clear();
        _classicDiv.Toggle(true);

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
        if (letter == '\0') return;
        var startIndex = _targetLetters.text.Length - 1;
        if (startIndex < 0) return;
        _targetLetters.text = _targetLetters.text.Remove(startIndex);
    }

    public void ToggleWord(bool show, Color color = default)
    {
        _targetWord.Toggle(show);
        if (!show) _targetLetters.text = string.Empty;
        _targetWord.style.backgroundColor = color;
    }

    public async void HideWord(string word)
    {
        if (!Session.IsClassicGame) return;
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
        _topRow.Toggle(true);
        _levelView.Toggle(true);
        var totalWords = _words.Count;
        var foundWords = levelState.FoundWords.Count;
        var leftWords = totalWords - foundWords;
        var activeFirstLetters = levelState.ActiveFirstLetters.Count;

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
                HideWord(word.Word);

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
        AbilityLogic.OnFakeLettersRemoved -= DisableMagnet;
        LevelStateService.OnActiveFirstLetterRemoved -= HandleFirstLettersRemoved;
        EventManager.AdsRemoved -= RemoveAdsBtn;
        RootHeight = 0;

        _adsBtn.clicked -= RequestAds;

    }


    public bool IsShopOpen => _shopView.style.display == DisplayStyle.Flex;

    internal async Task HideShopView()
    {
        _levelView.Toggle(true);
        while (_levelView.resolvedStyle.display != DisplayStyle.Flex)
            await Task.Yield();
        _shopView.Hide();
    }

    internal void SetTimeMode()
    {
        _classicDiv.Toggle(false);
        _timeDiv.Toggle(true);

        foreach (Button ability in _abilityBtns.Values.Cast<Button>())
        {
            ability.Toggle(false);
        }
        _abilityBtns[Ability.Freeze].Toggle(true);
        UpdateTimer(100);
    }

    internal void ShowTimeOver()
    {
        _plateView.ShowPlate(Plate.Timeout);
    }

    internal void SetTimeWord(string targetWord)
    {
        _timeWordLbl.text = targetWord;
    }

    public void BlinkTimer()
    {
        var defaultColor = _timeFill.resolvedStyle.backgroundColor;
        _timeFill.style.backgroundColor = defaultColor;

        StartCoroutine(BlinkTimerCoroutine(defaultColor, _blinkColor));
    }

    private IEnumerator BlinkTimerCoroutine(StyleColor defaultColor, Color blinkColor)
    {
        _isBlinking = true;
        var timeLeft = 0.5f;
        while (timeLeft >= 0)
        {
            _timeFill.style.backgroundColor = blinkColor;
            yield return new WaitForSeconds(0.1f);
            _timeFill.style.backgroundColor = defaultColor;
            yield return new WaitForSeconds(0.1f);
            timeLeft -= 0.2f;
        }
        _isBlinking = false;
        _timeFill.style.backgroundColor = defaultColor;
    }
}

public interface IAdsRequest
{
    public void RequestAds();
}