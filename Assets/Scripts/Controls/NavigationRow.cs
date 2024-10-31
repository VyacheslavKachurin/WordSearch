using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[UxmlElement]
public partial class NavigationRow : VisualElement
{
    public static event Action OnBackBtnClicked;
    public static event Action OnShopBtnClicked;
    public static event Action OnSettingsClicked;
    public static event Action OnShopHideClicked;

    private Label _coinsLbl;
    private Button _shopBtn;
    private Button _settingsBtn;
    private Label _levelLbl;
    private VisualElement _levelDiv;
    private Button _backBtn;
    public Label CoinsLbl => _coinsLbl;
    private VisualElement _coinsAnimDiv;

    private CoinsFX _coinsFX;

    private CoinsAnim _coinsAnim;
    private AudioManager _audioManager;
    private List<Button> _btns;

    [UxmlAttribute]
    public bool IsLevelVisible
    {
        get => _levelDiv.style.display == DisplayStyle.Flex;
        set => _levelDiv.Toggle(value);
    }

    [UxmlAttribute]
    public bool IsBackActive
    {
        get => _backBtn.style.display == DisplayStyle.Flex;
        set => _backBtn.Toggle(value);
    }

    [UxmlAttribute]
    public bool IsSettingsVisible
    {
        get => _settingsBtn.style.visibility == Visibility.Visible;
        set => _settingsBtn.style.visibility = value ? Visibility.Visible : Visibility.Hidden;
    }


    public NavigationRow()
    {
        var template = Resources.Load<VisualTreeAsset>("NavigationTemplate");
        var instance = template.Instantiate();
        instance.style.flexGrow = 1;
        instance.pickingMode = PickingMode.Ignore;
        instance.style.flexGrow = 1;
        this.style.position = Position.Absolute;
        this.style.flexGrow = 1;
        Add(instance);

        _backBtn = this.Q<Button>("back-btn");
        _shopBtn = this.Q<Button>("shop-btn");

        _coinsLbl = this.Q<Label>("coins-lbl");
        _settingsBtn = this.Q<Button>("settings-btn");
        _levelLbl = this.Q<Label>("level-lbl");
        _levelDiv = this.Q<VisualElement>("level-div");

        _backBtn.clicked += HandleBackBtn;
        _shopBtn.clicked += HandleShopBtn;

        _btns = new List<Button> { _backBtn, _shopBtn, _settingsBtn };

        this.RegisterCallback<DetachFromPanelEvent>((evt) =>
        {
            Unsubscribe();
        });





    }


    public void InitBalance(AudioManager audioManager)
    {
        _audioManager = audioManager;
        SetBalance(Balance.GetBalance());
        Balance.OnBalanceChanged += SetBalance;
        foreach (var btn in _btns)
        {
            btn.RegisterCallback<ClickEvent>(MakeBtnSound);
        }

        PlateView.OnAnimateCoinsRequested += AnimateAddCoins;
        _settingsBtn.clicked += HandleSettingsBtn;
    }

    private void MakeBtnSound(ClickEvent evt)
    {
        _audioManager.PlaySound(Sound.Click);
    }

    public void Unsubscribe()
    {
        Balance.OnBalanceChanged -= SetBalance;
        foreach (var btn in _btns)
        {
            btn.UnregisterCallback<ClickEvent>(MakeBtnSound);
        }

        _backBtn.clicked -= HandleBackBtn;
        _shopBtn.clicked -= HandleShopBtn;

        foreach (var btn in _btns)
        {
            btn.RegisterCallback<ClickEvent>(MakeBtnSound);
        }

        PlateView.OnAnimateCoinsRequested -= AnimateAddCoins;
        _settingsBtn.clicked -= HandleSettingsBtn;
        CoinsAnim.CoinsShown -= ShowCoinsFX;
    }



    public void SetCoinsAnim(CoinsAnim coinsAnim)
    {
        _coinsAnim = coinsAnim;

        _coinsFX = GameObject.Find("CoinsFX").GetComponent<CoinsFX>();

        _shopBtn.RegisterCallbackOnce<GeometryChangedEvent>(e =>
        {
            Debug.Log($"e == null : {e == null}");
            Debug.Log($"coins fx == null : {_coinsFX == null}");
            Debug.Log($"shop btn == null : {_shopBtn == null}");
            var shopPos = _shopBtn.worldTransform.GetPosition();
            var coinsBounds = _shopBtn.worldBound;
            //      Debug.Log($"Coins Pos: {coinsPos} \n Coins Bounds: {coinsBounds}");
            var worldPos = Camera.main.ScreenToWorldPoint(shopPos + new Vector3(0, coinsBounds.height / 2));

            //            Debug.Log($"screen to world Point: {worldPos}");
            _coinsFX.SetForceField(worldPos);

            Debug.Log($"shop pos == null : {_shopBtn == null}");
            Debug.Log($"coins bounds=null : {coinsBounds == null}");
            Debug.Log($"world pos =null : {worldPos == null}");

        });


    }

    private void HandleSettingsBtn()
    {
        OnSettingsClicked?.Invoke();

    }

    private void HandleShopBtn()
    {
        OnShopBtnClicked?.Invoke();
        IsSettingsVisible = false;
        IsBackActive = true;
    }

    private void HandleBackBtn()
    {
        if (IsSettingsVisible)
            OnBackBtnClicked?.Invoke();
        else
        {
            IsSettingsVisible = true;
            OnShopHideClicked?.Invoke();
            Session.IsSelecting = true;
        }
    }

    public void SetLevel(string level)
    {
        _levelDiv.Toggle(true);
        _levelLbl.text = level;
    }

    public void SetBalance(decimal balance)
    {
        string text = balance < 1000 ? balance.ToString("F0") : (balance / 1000).ToString("0.#") + " K";
        _coinsLbl.text = text;
    }

    private void ShowCoinsFX(Vector2 startPos, int amount)
    {
        _coinsFX.CreateAnim(startPos, amount);
    }

    public void AnimateAddCoins(Vector2 startPos, int amount, Action callback = null)
    {
        _coinsAnim.ShowCoinsLbl(startPos, amount, callback);

    }


}
