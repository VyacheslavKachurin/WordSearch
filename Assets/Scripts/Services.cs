using System;
using System.Threading.Tasks;
using UnityEngine;

public class Services : MonoBehaviour
{
    private IAPManager _iAPManager;

    private void Awake()
    {
        InitPurchases();

    }

    [ContextMenu("Init Purchases")]
    public async void InitPurchases()
    {
        await InitUGS();
        InitIAPManager();
        LogStoreItems();


        ShopView.OnShopBtnClicked += BuyCoins;
        ShopView.OnRemoveAdsClicked += RemoveAds;
    }

    [ContextMenu("Log Store Items")]
    private void LogStoreItems()
    {
        //_iAPManager.LogStoreItems();
    }

    [ContextMenu("Buy Coins")]
    private void BuyCoins(int i)
    {
        _iAPManager.BuyCoins(i);
    }
    private void RemoveAds()
    {
       _iAPManager.RemoveAds();
    }

    private void InitIAPManager()
    {
        _iAPManager = new IAPManager();
    }

    private async Task InitUGS()
    {
        await UGS.Init();
    }
}