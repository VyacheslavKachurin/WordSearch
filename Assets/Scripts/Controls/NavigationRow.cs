using System;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class NavigationRow : VisualElement
{
    public static event Action OnBackBtnClicked;
    public static event Action OnShopBtnClicked;
    public static event Action OnSettingsBtnClicked;

    private Button _coinsLbl;
    private Button _shopBtn;
    private Button _settingsBtn;
    private Label _levelLbl;
    private VisualElement _levelDiv;
    private Button _backBtn;

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

        _coinsLbl = this.Q<Button>("coins-lbl");
        _settingsBtn = this.Q<Button>("settings-btn");
        _levelLbl = this.Q<Label>("level-lbl");
        _levelDiv = this.Q<VisualElement>("level-div");

        _backBtn.clicked += HandleBackBtn;
        _shopBtn.clicked += HandleShopBtn;
        _settingsBtn.clicked += HandleSettingsBtn;



    }

    private void HandleSettingsBtn()
    {
        OnSettingsBtnClicked?.Invoke();
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

}
