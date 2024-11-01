using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuView : MonoBehaviour
{
    [SerializeField] private float _hideBoxDelay = 0.005f;

    [SerializeField] private string _levelScene = "LevelScene";

    private VisualElement _root;
    private FadePanel _fadePnl;
    private VisualElement _giftDiv;
    private VisualElement _giftView;

    private List<Button> _giftBtns;

    private const string GIFT_ANIM_IN = "gift-anim-in";
    private const string GIFT_ANIM_OUT = "gift-anim-out";

    private const string BOX_HIDE = "box-hide";
    private const string BOX_WIN = "box-win";

    private NavigationRow _navRow;

    [SerializeField] private int _minPrize = 20;
    [SerializeField] private int _maxPrize = 50;
    private Button _classicBtn;
    private Button _giftBtn;
    [SerializeField] private int _giftCoinsDelay = 100;
    private VisualElement _grantedDiv;
    private VisualElement _interactionDiv;
    private Label _newGiftLbl;
    private Button _giftCloseBtn;
    private VisualElement _shopBg;
    private Button _adsBtn;

    private void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;

        _fadePnl = _root.Q<FadePanel>();
        _giftDiv = _root.Q<VisualElement>("gift-div");
        _giftView = _root.Q<VisualElement>("gift-view");
        _navRow = _root.Q<NavigationRow>();

        SetGiftBtns();
        _navRow.SetCoinsAnim(_root.Q<CoinsAnim>());
        _navRow.InitBalance(AudioManager.Instance);

        _classicBtn = _root.Q<Button>("classic-btn");
        _classicBtn.clicked += HandleClassicBtn;

        _giftBtn = _root.Q<Button>("gift-btn");
        _giftBtn.clicked += ShowGift;

        _grantedDiv = _root.Q<VisualElement>("granted-div");
        _interactionDiv = _root.Q<VisualElement>("interaction-div");
        _newGiftLbl = _root.Q<Label>("new-gift-lbl");

        _giftCloseBtn = _root.Q<Button>("gift-close-btn");
        _giftCloseBtn.clicked += () => HideGift();

        _shopBg = _root.Q<VisualElement>("shop-bg");

        NavigationRow.OnShopBtnClicked += ShowShopBg;
        NavigationRow.OnShopHideClicked += HideShopBg;

        _adsBtn = _root.Q<Button>("ads-btn");
        _adsBtn.clicked += BuyNoAds;

        Session.AdsRemoved += HideAdsBtn;


        if (Session.NoAds) HideAdsBtn();

        TryRemoveBanner();
        SetBackPicture();

        EnableSettingsBtn();

        _root.RegisterCallback<DetachFromPanelEvent>((evt) =>
       {
           Unsubscribe();
       });

    }

    private void Unsubscribe()
    {
        NavigationRow.OnShopBtnClicked -= ShowShopBg;
        NavigationRow.OnShopHideClicked -= HideShopBg;
    }

    private void EnableSettingsBtn()
    {
        var plateView = _root.Q<PlateView>();
        plateView.SubscribeToSettingsClick();
    }

    public void SetBackPicture()
    {
        var bg = _root.Q<VisualElement>("bg");
        var bgController = GameObject.Find("BGController").GetComponent<BgController>();
        var bgStyle = new StyleBackground(bgController.GetBackView());
        bg.style.backgroundImage = bgStyle;

        _shopBg.style.backgroundImage = bgStyle;
    }

    [ContextMenu("Remove banner")]
    private void TryRemoveBanner()
    {
        var adsControllerObj = GameObject.Find("AdsController");
        if (!adsControllerObj) return;
        var adsController = adsControllerObj.GetComponent<AdsController>();

        if (adsController) adsController.RemoveBanner();
    }

    private void HideAdsBtn()
    {
        _adsBtn.style.visibility = Visibility.Hidden;
    }

    private void BuyNoAds()
    {
        ShopView shopView = _root.Q<ShopView>();
        shopView.InitRemoveAds();
    }

    private void HandleClassicBtn()
    {
        SceneManager.LoadScene(_levelScene);
    }

    void OnDestroy()
    {
        _navRow?.Unsubscribe();
    }

    private void SetGiftBtns()
    {
        _giftBtns = _giftView.Query<Button>().ToList();

        foreach (var btn in _giftBtns)
        {
            btn.RegisterCallback<ClickEvent>(HandleGiftBtn);
            btn.AddToClassList(BOX_HIDE);
        }

    }

    private void HandleGiftBtn(ClickEvent evt)
    {
        AudioManager.Instance.PlaySound(Sound.Click);
        var btn = evt.currentTarget as Button;

        var otherBtns = _giftBtns.FindAll(x => x != btn);
        foreach (var b in otherBtns)
        {
            b.AddToClassList(BOX_HIDE);
        }
        btn.RegisterCallbackOnce<TransitionEndEvent>(UnpackGift);
        btn.AddToClassList(BOX_WIN);
    }

    private void UnpackGift(TransitionEndEvent evt)
    {
        var randomPrize = UnityEngine.Random.Range(_minPrize, _maxPrize + 1);
        var element = evt.target as VisualElement;
        var pos = element.worldBound.position;//+ new Vector2(element.worldBound.width / 2, element.worldBound.height / 2);

        _navRow.AnimateAddCoins(pos, randomPrize, () => HideGift(element));
        Balance.AddBalance(randomPrize, _giftCoinsDelay);
        AudioManager.Instance.PlaySound(Sound.Coins);
        Session.WasGiftReceived = true;
    }


    [ContextMenu("Show gift panel")]
    public void ShowGift()
    {
        _giftDiv.RemoveFromClassList(GIFT_ANIM_OUT);
        _fadePnl.Toggle(true);
        _giftView.Toggle(true);
        _giftDiv.Toggle(true);
        _giftDiv.AddToClassList(GIFT_ANIM_IN);
        AudioManager.Instance.PlaySound(Sound.WindOpen);


        if (Session.WasGiftReceived)
        {
            _interactionDiv.Toggle(false);
            _grantedDiv.Toggle(true);
            _newGiftLbl.text = $"come back in {Session.GiftTimeLeft()}";
        }
        else
        {
            _interactionDiv.Toggle(true);
        }

        foreach (var btn in _giftBtns)
            btn.RemoveFromClassList(BOX_HIDE);


    }

    private void HideShopBg()
    {
        _shopBg.Toggle(false);
    }

    private void ShowShopBg()
    {
        _shopBg.Toggle(true);
    }

    public void HideGift(VisualElement element = null)
    {

        _giftDiv.RegisterCallbackOnce<TransitionEndEvent>(e =>
        {
            _giftView.Toggle(false);
            _giftDiv.Toggle(false);
            //_giftDiv.RemoveFromClassList(GIFT_ANIM_IN);
            //_giftDiv.RemoveFromClassList(GIFT_ANIM_OUT);
            _fadePnl.Toggle(false);
            element?.RemoveFromClassList(BOX_WIN);


        });

        _giftDiv.AddToClassList(GIFT_ANIM_OUT);



        foreach (var btn in _giftBtns)
        {
            btn.style.opacity = 1;
            btn.style.scale = new StyleScale(Vector3.one);
            btn.Toggle(true);
        }

    }
}