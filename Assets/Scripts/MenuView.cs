using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuView : MonoBehaviour
{
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
    private Button _giftsViewBtn;
    [SerializeField] private int _giftCoinsDelay = 100;
    private VisualElement _grantedDiv;
    private VisualElement _interactionDiv;
    private Label _newGiftLbl;
    private Button _giftCloseBtn;
    private VisualElement _shopBg;
    private Button _adsBtn;
    private CoinsView _coinsView;
    [SerializeField] private Vector2 _fakeGiftPos;
    [SerializeField] private CoinsFX_Handler _coinsFX_Handler;
    [SerializeField] private float _hideGiftDelay = 0.5f;
    [SerializeField] private Camera _fxCam;
    private Image _overlayFx;

    private void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;

        _fadePnl = _root.Q<FadePanel>();
        _giftDiv = _root.Q<VisualElement>("gift-div");
        _giftView = _root.Q<VisualElement>("gift-view");
        _navRow = _root.Q<NavigationRow>();


        _coinsView = _root.Q<CoinsView>();

        _navRow.InitBalance(AudioManager.Instance);

        _classicBtn = _root.Q<Button>("classic-btn");
        _classicBtn.clicked += HandleClassicBtn;

        _giftsViewBtn = _root.Q<Button>("gift-btn");
        _giftsViewBtn.clicked += ShowGiftView;

        _grantedDiv = _root.Q<VisualElement>("granted-div");
        _interactionDiv = _root.Q<VisualElement>("interaction-div");
        _newGiftLbl = _root.Q<Label>("new-gift-lbl");

        _giftCloseBtn = _root.Q<Button>("gift-close-btn");
        _giftCloseBtn.clicked += () => StartCoroutine(HidingGift(true));

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

        NavigationRow.CoinsPicResolved += SetCoinsAnimation;

        SetGiftBtns();

        _overlayFx = _root.Q<Image>("overlay-fx");
        _overlayFx.SetRenderTexture(_fxCam);
    }

    private void SetCoinsAnimation(VisualElement target)
    {
        var worldPos = target.GetWorldPosition(_root);
        _coinsFX_Handler.SetForceField(worldPos);
    }


    [ContextMenu("Create Sprite")]
    private void CreateSprite()
    {
        var sprite = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        var go = new GameObject();
        go.AddComponent<SpriteRenderer>().sprite = sprite;
        go.transform.position = _navRow.GetCoinsPicPos();
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
        AudioManager.Instance.PlaySound(Sound.Click);
        SceneManager.LoadScene(_levelScene);
    }

    void OnDestroy()
    {
        _navRow?.Unsubscribe();

        NavigationRow.OnShopBtnClicked -= ShowShopBg;
        NavigationRow.OnShopHideClicked -= HideShopBg;
        NavigationRow.CoinsPicResolved -= SetCoinsAnimation;
    }

    private void SetGiftBtns()
    {
        _giftBtns = _interactionDiv.Query<Button>().ToList();

        foreach (var btn in _giftBtns)
        {
            btn.RegisterCallback<ClickEvent>(HandleGiftClicked);
            btn.AddToClassList(BOX_HIDE);
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

    private void HandleGiftClicked(ClickEvent evt)
    {
        Debug.Log($"Handle gift btn");
        AudioManager.Instance.PlaySound(Sound.Click);
        var btn = evt.currentTarget as Button;

        var otherBtns = _giftBtns.FindAll(x => x != btn);
        foreach (var b in otherBtns)
        {
            b.AddToClassList(BOX_HIDE);
        }
        btn.RegisterCallbackOnce<TransitionEndEvent>(UnpackGift);

        btn.AddToClassList(BOX_WIN);

        foreach (var b in _giftBtns)
        {
            b.UnregisterCallback<ClickEvent>(HandleGiftClicked);
        }
    }

    

    private void UnpackGift(TransitionEndEvent evt)
    {
        Debug.Log($"Unpack gift");
        var randomPrize = UnityEngine.Random.Range(_minPrize, _maxPrize + 1);
        var element = evt.target as VisualElement;
        var pos = element.worldBound.position;
        Debug.Log($"Unpack gift pos: {pos}");
        _coinsView.ShowCoinsLbl(pos, randomPrize);
        Balance.AddBalance(randomPrize, _giftCoinsDelay);
        AudioManager.Instance.PlaySound(Sound.Coins);
        Session.WasGiftReceived = true;
        StartCoroutine(HidingGift(false));

        var btn = evt.target as VisualElement;
        var worldPos = btn.GetWorldPosition(_root);
        _coinsFX_Handler.PlayCoinsFX(worldPos);
    }



    [ContextMenu("Show gift view")]
    public void ShowGiftView()
    {
        //_giftDiv.RemoveFromClassList(GIFT_ANIM_OUT);
        _fadePnl.Toggle(true);
        _giftView.Toggle(true);
        //_giftDiv.Toggle(true);
        _giftDiv.AddToClassList(GIFT_ANIM_IN);
        AudioManager.Instance.PlaySound(Sound.WindOpen);

        if (Session.WasGiftReceived)
        {
            _interactionDiv.Toggle(false);
            _grantedDiv.Toggle(true);
            _newGiftLbl.text = $"come back in {Session.GiftTimeLeft()}";
            _giftCloseBtn.Toggle(true);
        }
        else
        {
            _interactionDiv.Toggle(true);
            _grantedDiv.Toggle(false);
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

    public IEnumerator HidingGift(bool isInstant = false)
    {
        var delayTime = isInstant ? 0 : _hideGiftDelay;
        yield return new WaitForSeconds(delayTime);
        AudioManager.Instance.PlaySound(Sound.WindClose);
        _giftDiv.RegisterCallbackOnce<TransitionEndEvent>(e =>
        {
            _giftView.Toggle(false);
            _giftDiv.Toggle(false);
            //_giftDiv.RemoveFromClassList(GIFT_ANIM_IN);
            //_giftDiv.RemoveFromClassList(GIFT_ANIM_OUT);
            _fadePnl.Toggle(false);

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