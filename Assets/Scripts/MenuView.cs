using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Backtrace.Unity;
using Backtrace.Unity.Model;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuView : MonoBehaviour
{
    public static event Action<Vector2, Vector2> AwardRequested;
    public static event Action RemoveAdsClicked;
    public static event Action CircleClicked;

    private VisualElement _root;
    private Label _levelNumberLbl;
    private VisualElement _sliderFill;
    private Button _timeBtn;
    private Button _collectionsBtn;

    private VisualElement _blurPnl;
    private VisualElement _giftDiv;
    private VisualElement _giftView;

    private List<Button> _giftPickBtns;

    private const string PANEL_ANIM_IN = "panel-anim-in";
    private const string GIFT_PICK_HIDE = "gift-pick-hide";
    private const string GIFT_PICK_WIN = "gift-pick-win";


    private Button _classicBtn;
    private Button _giftBtn;
    private Button _settingsBtn;

    private VisualElement _grantedDiv;
    private VisualElement _interactionDiv;
    private Label _newGiftTimerLbl;
    private Button _giftCloseBtn;
    private Button _adsBtn;
    private CoinsView _coinsView;
    private PlateView _plateView;

    [SerializeField] private float _hideGiftDelay = 0.5f;
    [SerializeField] private Camera _fxCam;
    private Image _overlayFx;
    private ShopView _shopView;

    public IShopItems ShopItems
    {
        get { return _shopView; }
        set { _shopView = value as ShopView; }
    }

    public static event Action<Vector2> OnCoinsFXRequested;


    // private IPrizeProvider _prizeProvider;
    private Button _clickedBtn;

    private ShopBtn _shopBtn;
    private VisualElement _backDiv;
    private Button _backBtn;
    private VisualElement _menuDiv;
    private VisualElement _collectionsView;
    private Button _collectionsNextBtn;
    private Button _collectionsBackBtn;
    public static event Action OnBackClicked;

    public static event Action OnCollectionsClicked;
    public static event Action<bool> PlayClicked;

    public VisualElement _currentView;
    private VisualElement _content;
    private List<VisualElement> _stampPages = new();
    private int _currentStampPage;

    [SerializeField] BacktraceClient _backtraceClient;
    private VisualElement _circle;
    private MessageView _messageView;
    private Label _messageLbl;

    public void Init()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _messageView = _root.Q<MessageView>();

        InitMenuDiv();
        InitGiftView();
        InitOverlayFx();
        InitShopBtn();
        InitShopView();
        InitBackBtn();
        InitCollectionsView();

        _blurPnl = _root.Q<VisualElement>("blur-panel");
        _coinsView = _root.Q<CoinsView>();

        _plateView = _root.Q<PlateView>();
        _plateView.SetBlur(_blurPnl);

        _menuDiv = _root.Q<VisualElement>("menu-div");


        EventManager.RemoveAdsClicked += ShowPurchaseMessage;
        EventManager.PurchaseClicked += ShowPurchaseMessage;
        EventManager.AdsRemoved += HideAdsBtn;

    }

    private void ShowPurchaseMessage(int obj)
    {
        ShowMessage("Processing purchase...");
    }

    private void ShowPurchaseMessage()
    {
        ShowMessage("Processing purchase...");
    }

    public void ShowMessage(string message)
    {
        _messageView.Toggle(true);
        _messageView.ShowMessage(message);
    }

    public void HideMessage()
    {
        _messageView.HideMessage();
    }

    private void InitCollectionsView()
    {
        _collectionsView = _root.Q("collections-view");
        _collectionsView.RemoveFromClassList("PANEL_ANIM");
        _collectionsNextBtn = _root.Q<Button>("next-btn");
        _collectionsBackBtn = _collectionsView.Q<Button>("back-btn");

        _collectionsNextBtn.clicked += OnCollectionNextClick;
        _collectionsBackBtn.clicked += OnCollectionBackClick;

        _collectionsBtn = _root.Q<Button>("collections-btn");
        _collectionsBtn.clicked += OnCollectionsClick;
        _content = _collectionsView.Q<VisualElement>("content");
    }

    private void OnCollectionsClick()
    {
        OnCollectionsClicked?.Invoke();
    }

    private void OnCollectionBackClick()
    {
        ShowPreviousStampPage();
        AudioManager.Instance.PlaySound(Sound.Click);
    }

    private void OnCollectionNextClick()
    {
        ShowNextStampPage();
        AudioManager.Instance.PlaySound(Sound.Click);
    }

    private void InitBackBtn()
    {
        _backDiv = _root.Q<VisualElement>("back-div");
        _backBtn = _root.Q<Button>("back-btn");
        _backBtn.clicked += HandleBackClick;

    }

    public void ShowBackBtn(bool show = true) => _backDiv.Toggle(show);



    private void HandleBackClick()
    {
        OnBackClicked?.Invoke();
    }

    public async void HideCollectionsView()
    {
        _collectionsView.RemoveFromClassList(PANEL_ANIM_IN);
        await Task.Delay(600);
        _collectionsView.Toggle(false);

        while (_collectionsView.resolvedStyle.width > 0)
            await Task.Yield();

        ShowBackBtn(false);
        ShowMenuView(true);

    }

    public void HideShopView()
    {
        _shopView.Hide();
        ShowBackBtn(false);
        ShowMenuView(true);
        _blurPnl.Toggle(false);
        AudioManager.Instance.PlaySound(Sound.WindClose);
    }

    public void ShowStampPage(int index)
    {
        _currentStampPage = index;
        if (index == 0) _collectionsBackBtn.Toggle(false);
        else _collectionsBackBtn.Toggle(true);

        if (index == _stampPages.Count - 1) _collectionsNextBtn.Toggle(false);
        else _collectionsNextBtn.Toggle(true);

        foreach (var stampPage in _stampPages)
            stampPage.Toggle(false);
        _stampPages[index].Toggle(true);
    }
    public void ShowNextStampPage()
    {
        ShowStampPage(_currentStampPage + 1);
    }

    public void ShowPreviousStampPage()
    {
        ShowStampPage(_currentStampPage - 1);
    }

    public void ShowMenuView(bool show) => _menuDiv.Toggle(show);

    private void InitShopView()
    {
        _shopView = _root.Q<ShopView>();
        ShopItems = _shopView;

    }

    public void InitShopBtn()
    {
        _shopBtn = _root.Q<ShopBtn>();
        _shopBtn.SetRoot(_root);
        ShopBtn.ShopClicked += ShowShopView;

    }


    private void ShowShopView()
    {
        _currentView = _shopView;
        ShowMenuView(false);
        _shopView.Show();
        ShowBackBtn(true);

    }

    private void InitOverlayFx()
    {
        _overlayFx = _root.Q<Image>("overlay-fx");
        _overlayFx.Toggle(true);
        _overlayFx.SetRenderTexture(_fxCam);
    }

    private void InitGiftView()
    {
        _giftView = _root.Q<VisualElement>("gift-view");
        _giftDiv = _root.Q<VisualElement>("gift-div");
        _giftDiv.RemoveFromClassList(PANEL_ANIM_IN);

        _giftCloseBtn = _root.Q<Button>("gift-close-btn");
        _giftCloseBtn.clicked += () => StartCoroutine(HidingGiftView(true));

        _interactionDiv = _root.Q<VisualElement>("interaction-div");
        _grantedDiv = _root.Q<VisualElement>("granted-div");

        InitGiftPicBtns();

        //last element
        _newGiftTimerLbl = _root.Q<Label>("new-gift-timer-lbl");
    }

    private void InitMenuDiv()
    {
        _adsBtn = _root.Q<Button>("ads-btn");
        _adsBtn.clicked += HandleRemoveAdsClick;

        _giftBtn = _root.Q<Button>("gift-btn");
        _giftBtn.clicked += ShowGiftView;

        _settingsBtn = _root.Q<Button>("settings-btn");
        _settingsBtn.clicked += ShowSettings;

        _levelNumberLbl = _root.Q<Label>("level-number-lbl");
        _sliderFill = _root.Q<VisualElement>("slider-fill");

        _classicBtn = _root.Q<Button>("classic-btn");
        _classicBtn.clicked += HandleClassicClick;

        _timeBtn = _root.Q<Button>("time-btn");
        _timeBtn.clicked += HandleTimeClick;

        _circle = _root.Q<VisualElement>("circle");
        _circle.RegisterCallback<ClickEvent>(evt =>
        {
            CircleClicked?.Invoke();
        });

    }

    private void HandleTimeClick()
    {
        PlayClicked?.Invoke(false);
    }

    private void ShowSettings()
    {
        // InitCrash();
        _plateView.ShowPlate(Plate.Settings);
    }

    private void InitCrash()
    {

        try
        {
            int[] a = new int[0];
            Debug.Log(a[1]);
        }
        catch (Exception e)
        {
            _backtraceClient.Send(e);
        }

    }

    public void SetBackPicture(int season)
    {
        var bg = _root.Q<VisualElement>("bg");
        var bgController = GameObject.Find("BGController").GetComponent<BgController>();
        var bgStyle = new StyleBackground(Extensions.GetTexture(season));
        if (bgStyle != null)
            bg.style.backgroundImage = bgStyle;

    }


    public void HideAdsBtn()
    {
        _adsBtn.style.visibility = Visibility.Hidden;
    }

    private void HandleRemoveAdsClick()
    {
        EventManager.TriggerEvent(EventType.RemoveAdsRequest);
    }

    private void HandleClassicClick()
    {
        PlayClicked?.Invoke(true);
    }

    void OnDestroy()
    {
        _adsBtn.clicked -= HandleRemoveAdsClick;
        ShopBtn.ShopClicked -= ShowShopView;
        EventManager.AdsRemoved -= HideAdsBtn;

        EventManager.RemoveAdsClicked -= ShowPurchaseMessage;
        EventManager.PurchaseClicked -= ShowPurchaseMessage;


    }


    public Vector2 ShowCoinsPopup(int payout)
    {
        var prize = payout;
        var element = _shopView.BuyBtn;
        var pos = element.worldBound.position;
        _coinsView.ShowCoinsLbl(pos, prize);

        var worldPos = element.GetWorldPosition(_root);
        _shopView.BuyBtn = null;
        return worldPos;
    }

    private void InitGiftPicBtns()
    {
        _giftPickBtns = _interactionDiv.Query<Button>().ToList();

        foreach (var btn in _giftPickBtns)
        {
            btn.RegisterCallback<ClickEvent>(HandleGiftPickClick);
            btn.AddToClassList(GIFT_PICK_HIDE);
        }
        _giftCloseBtn.Toggle(false);
    }

    [ContextMenu("Log dimensions")]
    private void LogDimensions()
    {
        var width = _root.worldBound.width;
        var height = _root.worldBound.height;
        Debug.Log($"MenuView width: {width} height: {height}");

    }

    private void HandleGiftPickClick(ClickEvent evt)
    {
        _clickedBtn = evt.currentTarget as Button;
        HideOtherGiftPicks(_clickedBtn);
        AudioManager.Instance.PlaySound(Sound.Click);

        _clickedBtn.RegisterCallbackOnce<TransitionEndEvent>(RequestAward);
        _clickedBtn.AddToClassList(GIFT_PICK_WIN);

        foreach (var b in _giftPickBtns)
        {
            b.UnregisterCallback<ClickEvent>(HandleGiftPickClick);
        }
    }


    private void HideOtherGiftPicks(Button clickedBtn)
    {
        var otherBtns = _giftPickBtns.FindAll(x => x != clickedBtn);
        foreach (var b in otherBtns)
        {
            b.AddToClassList(GIFT_PICK_HIDE);
        }
    }



    private void RequestAward(TransitionEndEvent evt)
    {
        //  var randomPrize = _randomPrize;
        var targetGiftPick = evt.target as VisualElement;
        var giftPickPos = targetGiftPick.worldBound.position;
        var worldPos = targetGiftPick.GetWorldPosition(_root);

        AwardRequested?.Invoke(giftPickPos, worldPos);

        //  StartCoroutine(HidingGiftView(false));
    }

    public void ShowCoinsPopup(Vector2 pos, int coins, Action callback)
    {
        _coinsView.ShowCoinsLbl(pos, coins, callback);
    }


    public void HideCoinsPopup()
    {
        _coinsView.HideAsync();
    }

    public void ShowCollectionsView()
    {
        _currentView = _collectionsView;
        _collectionsView.Toggle(true);
        _collectionsView.AddToClassList(PANEL_ANIM_IN);


    }

    public void ShowGiftView()
    {
        _blurPnl.Toggle(true);
        _blurPnl.PlaceBehind(_giftView);

        _giftView.Toggle(true);

        _giftDiv.AddToClassList(PANEL_ANIM_IN);
        AudioManager.Instance.PlaySound(Sound.WindOpen);

        if (Session.WasGiftReceived)
        {
            _interactionDiv.Toggle(false);
            _grantedDiv.Toggle(true);
            _newGiftTimerLbl.text = $"come back in {Session.GiftTimeLeft()}";
            _giftCloseBtn.Toggle(true);
        }
        else
        {
            _interactionDiv.Toggle(true);
            _grantedDiv.Toggle(false);
        }

        foreach (var btn in _giftPickBtns)
            btn.RemoveFromClassList(GIFT_PICK_HIDE);


    }

    public IEnumerator HidingGiftView(bool isInstant = false)
    {

        var delayTime = isInstant ? 0 : _hideGiftDelay;
        yield return new WaitForSeconds(delayTime);
        AudioManager.Instance.PlaySound(Sound.WindClose);
        _giftDiv.RegisterCallbackOnce<TransitionEndEvent>(e =>
        {
            _giftView.Toggle(false);

            _blurPnl.Toggle(false);

        });

        _giftDiv.RemoveFromClassList(PANEL_ANIM_IN);


        foreach (var btn in _giftPickBtns)
        {
            btn.style.opacity = 1;
            btn.style.scale = new StyleScale(Vector3.one);
            btn.Toggle(true);
        }

    }

    public void SetLevelData(int lastLevel, int lastEpisode, int lastTotalEpisodes)
    {
        HideMessage();
        _levelNumberLbl.text = lastLevel.ToString();
        float percentWidth = 0;
        if (lastEpisode > 1)
            percentWidth = (lastEpisode - 1) / (float)lastTotalEpisodes * 100;

        _sliderFill.style.width = new StyleLength(new Length(percentWidth, LengthUnit.Percent));


    }

    public ViewOpen GetCurrentView()
    {
        if (_currentView == _collectionsView)
            return ViewOpen.Collections;
        return ViewOpen.Shop;
    }

    internal void AddPage(VisualElement page)
    {
        _content.Add(page);
        _stampPages.Add(page);
    }

    public void PopulateCollectionsView(List<StampItem> seasons)
    {


        var index = 0;
        VisualElement page = null;
        VisualElement firstRow = null;
        VisualElement secondRow = null;
        VisualElement targetRow = null;

        for (int i = 0; i < seasons.Count; i++)
        {
            if (index == 0)
            {
                page = GetPage();
                firstRow = page.Q<VisualElement>("first-row");
                secondRow = page.Q<VisualElement>("second-row");
            }
            targetRow = index <= 3 ? firstRow : secondRow;
            var season = seasons[i];

            var stampItem = new PlaceStamp();
            var caption = season.Name;
            caption = caption.Replace(",", ",\n");
            stampItem.SetCaption(caption);

            var texture = Extensions.GetTexture(season.Season, true);
            stampItem.SetImage(texture);
            stampItem.Unlock(season.IsUnlocked);
            targetRow.Add(stampItem);
            index++;
            if (index == 8)
            {
                AddPage(page);
                index = 0;
            }

        }
        ShowStampPage(0);
    }



    private VisualElement GetPage()
    {
        var page = new VisualElement
        {
            name = "page"
        };
        page.style.flexGrow = 1;
        page.style.flexDirection = FlexDirection.Column;
        var row1 = GetRow("first-row");
        var row2 = GetRow("second-row");
        page.Add(row1);
        page.Add(row2);
        return page;
    }


    private VisualElement GetRow(string name)
    {
        var row = new VisualElement
        {
            name = name,
            style = { flexDirection = FlexDirection.Row }
        };
        row.AddToClassList("collections-row");
        return row;
    }



    internal void RequireConnection(bool isConnected)
    {
        if (!isConnected)
            _messageView.ShowMessage("You need to enable internet connection to play the game.");
        else _messageView.HideMessage();
    }

    internal void HideAdsOffer()
    {
        _shopView.HideAdsOffer();

    }
}

public enum ViewOpen { Shop, Collections }