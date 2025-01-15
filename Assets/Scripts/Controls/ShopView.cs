using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ShopView : VisualElement, IShopItems
{
    // private List<Button> _buyBtns;
    private Button _removeAdsBtn;
    private VisualElement _itemsDiv;
    private VisualElement _noAdsDiv;

    public static event Action OnRemoveAdsClicked;
    public static event Action<int> OnPurchaseInit;
    public static event Action OnRestoreClicked;

    public VisualElement BuyBtn = null;
    private VisualElement _networkDiv;
    private Button _restoreBtn;
    private VisualElement _removedAdsDiv;

    private VisualElement _adsOfferDiv;


    private VisualTreeAsset _purchaseBtnTemplate;
    private List<Button> _buyBtns = new();

    public static event Action<bool> OnShopClicked;
    //public static event Action<int, Button> OnBtnClicked;

    public ShopView()
    {
        var template = Resources.Load<VisualTreeAsset>("ShopView");
        var instance = template.Instantiate();
        instance.style.flexGrow = 1;
        // instance.pickingMode = PickingMode.Ignore;
        Add(instance);

        _purchaseBtnTemplate = Resources.Load<VisualTreeAsset>("PurchaseBtnTemplate");

        // style.position = Position.Absolute;
        style.flexGrow = 1;
        style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        style.width = new StyleLength(new Length(100, LengthUnit.Percent));


        _removeAdsBtn = this.Q<Button>("remove-ads-btn");
        _itemsDiv = this.Q<VisualElement>("items-div");

        _adsOfferDiv = this.Q<VisualElement>("ads-offer-div");

        _removeAdsBtn.clicked += () =>
        {
            Debug.Log($"Remove Ads Btn Clicked ");
            Services.IsNetworkAvailable((result) =>
            {
                if (result)
                    OnRemoveAdsClicked?.Invoke();
                else
                {
                    RequireNetwork();
                }
            });
        };

        _noAdsDiv = this.Q<VisualElement>("ads-div");
        _removedAdsDiv = this.Q<VisualElement>("ads-removed-div");
        if (Session.NoAds) HideAdsOffer();
        Session.AdsRemoved += HideAdsOffer;

        this.RegisterCallback<DetachFromPanelEvent>((evt) => Unsubscribe());
        _networkDiv = this.Q<VisualElement>("network-div");
        _restoreBtn = this.Q<Button>("restore-purchase-btn");
        _restoreBtn.clicked += AskRestorePurchase;
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
            Services.IsNetworkAvailable((result) =>
            {
                if (result)
                {
                    OnPurchaseInit?.Invoke(index);

                }
                else
                {
                    RequireNetwork();
                }
            });


        };
    }



    private void AskRestorePurchase()
    {
        Services.IsNetworkAvailable((result) =>
            {
                if (result)
                    OnRestoreClicked?.Invoke();
                else
                {
                    RequireNetwork();
                }
            });


    }



    private void Unsubscribe()
    {

        Session.AdsRemoved -= HideAdsOffer;
    }

    public void InitRemoveAds()
    {
        OnRemoveAdsClicked?.Invoke();
    }


    public void HideAdsOffer()
    {
        _adsOfferDiv.Toggle(false);
        _removedAdsDiv.Toggle(true);
    }

    private void RequireNetwork()
    {
        Debug.Log($"Require Network");
        return;
        var go = AudioManager.Instance;
        go.PlaySound(Sound.WrongWord);
        go.StartCoroutine(NetworkCoroutine());
    }



    private IEnumerator NetworkCoroutine()
    {
        _networkDiv.Toggle(true);
        yield return new WaitForSeconds(3);
        _networkDiv.Toggle(false);
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