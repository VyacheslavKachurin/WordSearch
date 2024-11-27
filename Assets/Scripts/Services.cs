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

    [ContextMenu("Delete State")]
    private void DeleteState()
    {
        LevelStateService.DeleteState();
    }

    [ContextMenu("Init Purchases")]
    public async void InitPurchases()
    {
        await InitUGS();
        InitIAPManager();
        LogStoreItems();


        ShopView.OnPurchaseInit += BuyCoins;
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

    private void OnDestroy()
    {
        ShopView.OnPurchaseInit -= BuyCoins;
        ShopView.OnRemoveAdsClicked -= RemoveAds;

    }

    private void RemoveAds()
    {
        Debug.Log($"Remove Ads");
        _iAPManager.RemoveAds();
    }

    private void InitIAPManager()
    {
        _iAPManager = new IAPManager();
    }

    [ContextMenu("Remove ads purchase")]
    public void RemoveAdsPurchase()
    {
        Session.NoAds = false;
    }

    [ContextMenu("Clear Level Progress")]
    public void ClearLevelProgress()
    {
       // Session.SetLastLevel(1);
    }

    [ContextMenu("Clear coins data")]
    public void ClearCoinsData()
    {
        Balance.ClearBalance();
    }

    [ContextMenu("Clear All Data")]
    public void ClearAllData()
    {
        ClearLevelProgress();
        ClearCoinsData();
        RemoveAdsPurchase();
        ClearGiftData();
       // Session.LastStage = 1;
       // Session.SetLastLevel(1);
        DeleteState();
        GameDataService.ClearProgress();
    }

    [ContextMenu("Clear Gift Data")]
    public void ClearGiftData()
    {
        Session.ClearGift();
    }


    private async Task InitUGS()
    {
        await UGS.Init();
    }
}