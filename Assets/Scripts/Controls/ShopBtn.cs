
using System;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ShopBtn : Button
{
    public static event Action<Vector2> ShopCoinGeometryChanged;
    public static event Action ShopClicked;

    private Button _shopBtn;
    private Label _balanceLbl;
    private VisualElement _shopCoinIcon;
    private VisualElement _root;

    public ShopBtn()
    {
        var template = Resources.Load<VisualTreeAsset>("ShopBtnTemplate");
        var instance = template.Instantiate();
        instance.style.flexGrow = 0;
        Add(instance);
        InitElements();

        this.RegisterCallback<DetachFromPanelEvent>((evt) =>
        {
            if (Application.isPlaying)
                Unsubscribe();
        });
    }

    public void SetRoot(VisualElement root)
    {
        _root = root;
        if (Application.isPlaying)
            _shopCoinIcon.RegisterCallback<GeometryChangedEvent>(NotifyShopCoinGeometry);
        Balance.OnBalanceChanged += UpdateBalanceLbl;
        
    }


    private void Unsubscribe()
    {
        Balance.OnBalanceChanged -= UpdateBalanceLbl;
        _shopCoinIcon.UnregisterCallback<GeometryChangedEvent>(NotifyShopCoinGeometry);
        _shopBtn.clicked -= HandleShopClick;
    }

    private void InitElements()
    {
        _shopCoinIcon = this.Q<VisualElement>("shop-coin-icon");
        _balanceLbl = this.Q<Label>("balance-lbl");

        _shopBtn = this.Q<Button>("shop-btn");
        _shopBtn.clicked += HandleShopClick;


    }

    private void NotifyShopCoinGeometry(GeometryChangedEvent evt)
    {
        var element = evt.target as VisualElement;
        var worldPos = element.GetWorldPosition(_root);
        ShopCoinGeometryChanged?.Invoke(worldPos);

    }

    public void InitBalance(int balance)
    {
        UpdateBalanceLbl(balance);

    }


    private void HandleShopClick()
    {
        ShopClicked?.Invoke();
    }

    public void UpdateBalanceLbl(int balance)
    {
        string text = balance < 1000 ? balance.ToString("F0") : (balance / 1000).ToString("0.#") + " K";
        _balanceLbl.text = text;
    }

}