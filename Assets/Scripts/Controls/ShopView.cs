using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ShopView : VisualElement, IShopItems
{

    private Button _removeAdsBtn;
    private VisualElement _itemsDiv;
    private VisualElement _noAdsDiv;

    public VisualElement BuyBtn = null;
    private VisualElement _networkDiv;
    private Button _restoreBtn;
    private VisualElement _removedAdsDiv;
    private VisualElement _adsOfferDiv;
    private VisualTreeAsset _purchaseBtnTemplate;
    private List<Button> _buyBtns = new();
    public static event Action<bool> OnShopClicked;

    public ShopView()
    {
        var template = Resources.Load<VisualTreeAsset>("ShopView");
        var instance = template.Instantiate();
        instance.style.flexGrow = 1;
        instance.pickingMode = PickingMode.Ignore;
        Add(instance);

        _purchaseBtnTemplate = Resources.Load<VisualTreeAsset>("PurchaseBtnTemplate");

        // style.position = Position.Absolute;
        style.flexGrow = 1;
        style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        style.width = new StyleLength(new Length(100, LengthUnit.Percent));


        _removeAdsBtn = this.Q<Button>("remove-ads-btn");
        _itemsDiv = this.Q<VisualElement>("items-div");

        _adsOfferDiv = this.Q<VisualElement>("ads-offer-div");

        _removeAdsBtn.clicked += HandleRemoveAdsClick;

        _noAdsDiv = this.Q<VisualElement>("ads-div");
        _removedAdsDiv = this.Q<VisualElement>("ads-removed-div");

        EventManager.AdsRemoved += HideAdsOffer;

        _networkDiv = this.Q<VisualElement>("network-div");
        _restoreBtn = this.Q<Button>("restore-purchase-btn");
        _restoreBtn.clicked += AskRestorePurchase;

        this.RegisterCallback<DetachFromPanelEvent>((evt) => Unsubscribe());
    }


    private void HandleRemoveAdsClick()
    {
        EventManager.TriggerEvent(Event.RemoveAdsRequest);
    }

    public void AddItem(int index, int coinsAmount, string price)
    {
        var template = _purchaseBtnTemplate;
        var instance = template.Instantiate();
        instance.Q<Label>("coins-lbl").text = coinsAmount.ToString();
#if !UNITY_EDITOR
        Debug.Log($"Price: {price}");
#endif
        instance.Q<Label>("price-lbl").text = price;
        _itemsDiv.Add(instance);

        _buyBtns.Add(instance.Q<Button>());
        _buyBtns[index].clicked += () =>
        {
            var pointer = index;
            var btn = _buyBtns[pointer];
            BuyBtn = btn;
            RequestPurchase(pointer);
        };
    }

    private void RequestPurchase(int index)
    {
        EventManager.TriggerPurchase(index);
    }


    private void AskRestorePurchase()
    {
        EventManager.TriggerEvent(Event.RestoreClick);
    }

    private void Unsubscribe()
    {
        EventManager.AdsRemoved -= HideAdsOffer;
        _removeAdsBtn.clicked -= HandleRemoveAdsClick;
    }

    public void InitRemoveAds()
    {
        EventManager.TriggerEvent(Event.RemoveAdsRequest);
    }


    public void HideAdsOffer()
    {
        _adsOfferDiv.Toggle(false);
        _removedAdsDiv.Toggle(true);
    }


    public void Show()
    {
        if (this.style.display == DisplayStyle.Flex) return;

        AudioManager.Instance.PlaySound(Sound.WindOpen);
        this.Toggle(true);
        OnShopClicked?.Invoke(true);
    }

    internal void Hide()
    {
        this.Toggle(false);
        OnShopClicked?.Invoke(false);
    }

    public void SetNoAds(string price)
    {
        _removeAdsBtn.text = $"Remove Ads {price}";
    }
}


public interface IShopItems
{
    public void AddItem(int index, int coinsAmount, string price);
    public void SetNoAds(string price);
}