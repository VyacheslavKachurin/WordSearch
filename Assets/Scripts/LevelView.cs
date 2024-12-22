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
    private Label _progressLbl;
    private VisualElement _cup;
    private VisualElement _levelView;
    [SerializeField] private float _fillDelay = 0.1f;
    [SerializeField] private float _fillAdd = 0.1f;
    private Coroutine _progressBarCoroutine;
    private Button _removeAdsBtn;

    private const string NEXT_LVL_BTN_ON = "next-lvl-btn-on";


    private const string TARGET_WORD_LEFT = "target-word-left";
    private const string TARGET_WORD_RIGHT = "target-word-right";

    [SerializeField] private ParticleSystem _winFX;
    [SerializeField] private Vector2 _fakePos;
    [SerializeField] private float _fakeLblHeight = 38;

    [SerializeField] private Camera _fxCam;

    [SerializeField] private int _prizeAmount = 25;
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
    const string FINISH_VIEW_HIDE = "finish-view-hide";

    private bool _isSliderDone;

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

        if (Session.NoAds) RemoveAdsBtn();
        else _removeAdsBtn.clicked += InitNoAdsPurchase;
        Session.AdsRemoved += RemoveAdsBtn;

        _plateView = _root.Q<PlateView>();
        _plateView.Toggle(true);

        _settingsBtn = _root.Q<Button>("settings-btn");
        _settingsBtn.clicked += ShowSettings;

        _root.RegisterCallback<DetachFromPanelEvent>(e => Unsubscribe());

        AdsController.RewardedAdWatched += RewardForAds;

        IAPManager.OnPurchasedCoins += AnimatePurchasedCoins;

        _adsBtn = _root.Q<Button>("ads-btn");
        _adsBtn.clicked += RequestAds;

        _backBtn = _root.Q<Button>("back-btn");
        _backBtn.clicked += () => OnBackClicked?.Invoke();
        _blurPnl = _root.Q<VisualElement>("blur-pnl");
        _plateView.SetBlur(_blurPnl);

        InitShopBtn();
        InitFinishView();
    }

    private void InitFinishView()
    {
        _finishView = _root.Q<VisualElement>("finish-view");
        _finishView.AddToClassList(FINISH_VIEW_HIDE);
        _nextLvlBtn = _finishView.Q<Button>("next-lvl-btn");
        _nextLvlBtn.RemoveFromClassList(NEXT_LVL_BTN_ON);
        _nextLvlBtn.clicked += HandleNextLvlClick;

        _progressFill = _root.Q<VisualElement>("progress-fill");
        _progressLbl = _root.Q<Label>("progress-lbl");
        _cup = _root.Q<VisualElement>("cup");
    }


    private void ShowSettings()
    {
        Session.IsSelecting = false;
        _plateView.ShowPlate(Plate.Settings);
        _plateView.PlaceInFront(_shopView);
        _blurPnl.PlaceBehind(_plateView);
    }

    private void InitShopBtn()
    {

        _shopBtn = _root.Q<ShopBtn>();
        _shopBtn.SetRoot(_root);
        _shopBtn.InitBalance();
        ShopBtn.ShopClicked += ShowShopView;
        _shopBtn.RegisterCallback<GeometryChangedEvent>(SetCoinsAnimation);
    }

    private void ShowShopView()
    {
        Session.IsSelecting = false;
        Debug.Log($"Show Shop View");
        _levelView.Toggle(false);
        _shopView.Show();
        OnGameViewToggle?.Invoke(false);

    }

    public void RequestAds()
    {
        Session.IsSelecting = false;
        _plateView.ShowPlate(Plate.Ads);
    }

    public void ShowGameOver()
    {
        _levelView.Toggle(false);

        _finishView.Toggle(false);

        _gameOverView.Toggle(true);
        _checkUpdatesBtn.clicked += () => Application.OpenURL($"itms-apps://itunes.apple.com/app/id{Session.AppId}"); ;
    }

    private void AnimatePurchasedCoins(int payout)
    {
        Debug.Log($"Animate Purchased Coins: {payout}");
        PlayAward(payout, _shopView.BuyBtn, _shopView.BuyBtn, true);
        _coinsView.HideAsync();
        _shopView.BuyBtn = null;
    }

    private void RewardForAds()
    {
        PlayAward(Session.RewardAmount, _adsBtn, _adsBtn);
        Balance.AddBalance(Session.RewardAmount);
        Session.IsSelecting = true;
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

        AdsController.RewardedAdWatched -= RewardForAds;
        IAPManager.OnPurchasedCoins -= AnimatePurchasedCoins;

        ShopBtn.ShopClicked -= ShowShopView;

    }

    [ContextMenu("Play coins FX")]
    public void PlayCoinsFX()
    {
        _coinsFX_Handler.PlayCoinsFX(_startPos, _fxScale);
    }


    private void PlayAward(int prizeAmount, VisualElement coinsLblRefPos, VisualElement fxStartPos, bool showLbl = true)
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



    /*
        public void SetBackPicture()
        {
            var bgController = GameObject.Find("BgController").GetComponent<BgController>();
            var bgStyle = new StyleBackground(bgController.GetBackView());
            _shopBg.style.backgroundImage = bgStyle;
            _finishView.style.backgroundImage = bgStyle;
        }
        */


    public async void ShowFinishView(int episode, int totalEpisodes, bool isStageCompleted = false)
    {
        _isSliderDone = false;
        _levelView.Toggle(false);

        Debug.Log($"show finish view");
        _progressLbl.text = $"{episode}/{totalEpisodes}";

        _finishView.Toggle(true);
        await Task.Delay(100);

        _finishView.RemoveFromClassList(FINISH_VIEW_HIDE);

        while (_finishView.resolvedStyle.opacity < 0.9f)
            await Task.Yield();
        AnimateProgressBar(episode, totalEpisodes);

        while (!_isSliderDone)
            await Task.Yield();

        if (isStageCompleted)
            ShowStageFinish();

        ShowNextLvlBtn();
        _progressBarCoroutine = null;

    }

    private void AnimateProgressBar(int episode, int totalEpisodes)
    {
        if (_progressBarCoroutine != null) return;
        _progressBarCoroutine = StartCoroutine(FillProgressBar(episode, totalEpisodes));

    }

    public void SetProgressBar(int episode, int totalEpisodes)
    {
        var currentFill = 100 * (((float)episode - 1) / (float)totalEpisodes);
        _progressFill.style.width = new StyleLength(new Length(currentFill, LengthUnit.Percent));
        _nextLvlBtn.RemoveFromClassList(NEXT_LVL_BTN_ON);
    }

    private IEnumerator FillProgressBar(int episode, int totalEpisodes)
    {
        Debug.Log($"Started filling progress bar");

        var targetFill = 100 * ((float)episode / (float)totalEpisodes);
        var currentFill = _progressFill.style.width.value.value;

        while (_finishView.resolvedStyle.opacity < 0.9f)
        {
            yield return new WaitForEndOfFrame();
            Debug.Log($"Opacity: {_finishView.resolvedStyle.opacity}");
        }

        while (_progressFill.style.width.value.value < targetFill)
        {
            currentFill += _fillAdd;
            _progressFill.style.width = new StyleLength(new Length(currentFill, LengthUnit.Percent));
            yield return new WaitForSeconds(_fillDelay);
        }
        _isSliderDone = true;
    }

    private void ShowStageFinish()
    {
        AudioManager.Instance.PlaySound(Sound.StageCompleted);
        _winFX.Play();
        PlayAward(_prizeAmount, _giftPic, _cup);
        Balance.AddBalance(_prizeAmount);
    }

    public void ShowNextLvlBtn()
    {
        _nextLvlBtn.AddToClassList(NEXT_LVL_BTN_ON);
        _nextLvlBtn.SetEnabled(true);
    }

    private void HandleNextLvlClick()
    {
        AudioManager.Instance.PlaySound(Sound.Click);
        NextLevelClicked?.Invoke();

    }

    public void HideFinishView()
    {
        _finishView.Toggle(false);
        _finishView.AddToClassList(FINISH_VIEW_HIDE);
        _progressBarCoroutine = null;
        _progressFill.style.width = new StyleLength(new Length(0, LengthUnit.Percent));

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
        _finishView.Toggle(false);
        _finishView.AddToClassList(FINISH_VIEW_HIDE);
        _levelView.Toggle(true);
        FillWords(data.Words);
        _levelLbl.text = $"Level: {GameDataService.GameData.Level}";
        ResetBtns();
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
            {Ability.Firework, _root.Q<AbilityBtn>("firework-btn")},
           // {Ability.Ads, _root.Q<AbilityBtn>("ads-btn")}
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
        AbilityLogic.OnFakeLettersRemoved -= DisableMagnet;
        LevelStateService.OnActiveFirstLetterRemoved -= HandleFirstLettersRemoved;
        Session.AdsRemoved -= RemoveAdsBtn;
        RootHeight = 0;

        _adsBtn.clicked -= RequestAds;

    }



    internal bool HideShopView()
    {
        if (_shopView.style.display == DisplayStyle.Flex)
        {
            _levelView.Toggle(true);
            _shopView.Hide();
            return true;
        }
        return false;
    }
}

public interface IAdsRequest
{
    public void RequestAds();
}