using System;
using System.Collections;
using System.Collections.Generic;
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
    public static event Action OnRestoreClicked;

    public VisualElement BuyBtn = null;
    private VisualElement _networkDiv;
    private Button _restoreBtn;
    private VisualElement _removedAdsDiv;

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

        _buyBtns = this.Query<Button>("shop-item").ToList();

        for (int i = 0; i < _buyBtns.Count; i++)
        {
            var index = i;
            _buyBtns[index].clicked += () =>
            {
                BuyBtn = _buyBtns[index];
                Services.IsNetworkAvailable((result) =>
                {
                    if (result)
                        OnPurchaseInit?.Invoke(index);
                    else
                    {
                        RequireNetwork();
                    }
                });

            };

        }
        _adsDiv = this.Q<VisualElement>("ads-div");
        _removedAdsDiv = this.Q<VisualElement>("removed-ads-div");
        if (Session.NoAds) HideAdsOffer();
        Session.AdsRemoved += HideAdsOffer;

        this.RegisterCallback<DetachFromPanelEvent>((evt) => Unsubscribe());
        _networkDiv = this.Q<VisualElement>("network-div");
        _restoreBtn = this.Q<Button>("restore-btn");
        _restoreBtn.clicked += AskRestorePurchase;
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
        _removedAdsDiv.Toggle(true);
    }

    private void HideShopView()
    {
        this.Toggle(false);
        Session.IsSelecting = true;

    }

    private void RequireNetwork()
    {
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


    private void ShowShopView()
    {
        if (this.style.display == DisplayStyle.Flex) return;
        Session.IsSelecting = false;
        this.Toggle(true);

    }
}