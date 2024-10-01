using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class NavigationRow : VisualElement
{
    public static event Action OnBackBtnClicked;
    public static event Action OnShopBtnClicked;
    public static event Action OnSettingsClicked;

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


    [UxmlAttribute]
    public bool IsLevelVisible
    {
        get => _levelDiv.style.display == DisplayStyle.Flex;
        set => _levelDiv.Toggle(value);
    }



    public NavigationRow()
    {
        var template = Resources.Load<VisualTreeAsset>("NavigationTemplate");
        Add(template.Instantiate());

        _backBtn = this.Q<Button>("back-btn");
        _shopBtn = this.Q<Button>("shop-btn");

        _coinsLbl = this.Q<Label>("coins-lbl");
        _settingsBtn = this.Q<Button>("settings-btn");
        _levelLbl = this.Q<Label>("level-lbl");
        _levelDiv = this.Q<VisualElement>("level-div");

        _backBtn.clicked += HandleBackBtn;
        _shopBtn.clicked += HandleShopBtn;
        _settingsBtn.clicked += HandleSettingsBtn;

        SetBalance(Balance.GetBalance());
        Balance.OnBalanceChanged += SetBalance;


    }

    public void SetCoinsAnim(CoinsAnim coinsAnim)
    {
        _coinsAnim = coinsAnim;
        CoinsAnim.CoinsShown += ShowCoinsFX;
        _coinsFX = _coinsFX != null ? _coinsFX : CoinsFX.Instance;

        _coinsLbl.RegisterCallbackOnce<GeometryChangedEvent>(e =>
        {
            var worldPos = _coinsLbl.worldTransform.GetPosition();
            Debug.Log($"World Pos: {worldPos}");
            Debug.Log($"screen to world Point: {Camera.main.ScreenToWorldPoint(worldPos)}");
            _coinsFX.SetForceField(Camera.main.ScreenToWorldPoint(worldPos));

        });


    }

    private void HandleSettingsBtn()
    {
        OnSettingsClicked?.Invoke();
        Debug.Log($"Settings clicked");
    }

    private void HandleShopBtn()
    {
        OnShopBtnClicked?.Invoke();
        Debug.Log($"Shop clicked");
    }



    private void HandleBackBtn()
    {
        OnBackBtnClicked?.Invoke();
        Debug.Log($"Back clicked");
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

    public void AnimateAddCoins(Vector2 startPos, int amount, Action callback)
    {
        _coinsAnim.ShowCoinsLbl(startPos, amount, callback);

    }

}
