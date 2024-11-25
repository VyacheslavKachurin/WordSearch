using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ShopView : VisualElement
{
    private List<Button> _buyBtns;
    private Button _noAdsBtn;
    private VisualElement _adsDiv;
    private NavigationRow _navRow;

    public static event Action OnRemoveAdsClicked;
    public static event Action<int> OnPurchaseInit;

    public VisualElement BuyBtn = null;


    public ShopView()
    {
        var template = Resources.Load<VisualTreeAsset>("ShopView");
        var instance = template.Instantiate();
        instance.style.flexGrow = 1;
        instance.pickingMode = PickingMode.Ignore;
        Add(instance);
        style.position = Position.Absolute;
        style.flexGrow = 1;
        style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        style.width = new StyleLength(new Length(100, LengthUnit.Percent));

        NavigationRow.OnShopBtnClicked += ShowShopView;
        NavigationRow.OnShopHideClicked += HideShopView;
        _noAdsBtn = this.Q<Button>("no-ads-btn");
        _noAdsBtn.clicked += () =>
        {
            Debug.Log($"Remove Ads Btn Clicked ");
            OnRemoveAdsClicked?.Invoke();
        };

        _buyBtns = this.Query<Button>("shop-item").ToList();

        for (int i = 0; i < _buyBtns.Count; i++)
        {
            var index = i;
            _buyBtns[index].clicked += () =>
            {
                BuyBtn = _buyBtns[index];
                OnPurchaseInit?.Invoke(index);
            };

        }
        _adsDiv = this.Q<VisualElement>("ads-div");

        if (Session.NoAds) HideAdsOffer();
        Session.AdsRemoved += HideAdsOffer;

        this.RegisterCallback<DetachFromPanelEvent>((evt) => Unsubscribe());
       
        
    }

    private void Unsubscribe()
    {
        NavigationRow.OnShopBtnClicked -= ShowShopView;
        NavigationRow.OnShopHideClicked -= ShowShopView;
        Session.AdsRemoved -= HideAdsOffer;
    }

    public void InitRemoveAds()
    {
        OnRemoveAdsClicked?.Invoke();
    }


    public void HideAdsOffer()
    {
        _adsDiv.Toggle(false);
    }

    private void HideShopView()
    {
        this.Toggle(false);
        Session.IsSelecting = true;

    }


    private void ShowShopView()
    {
        if (this.style.display == DisplayStyle.Flex) return;
        Session.IsSelecting = false;
        this.Toggle(true);

    }
}