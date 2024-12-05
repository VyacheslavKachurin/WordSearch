using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Net.NetworkInformation;
using Ping = System.Net.NetworkInformation.Ping;


public class Services : MonoBehaviour
{

    private IAPManager _iAPManager;

    private void Awake()
    {
        InitPurchases();
    }

    [ContextMenu("Restore Purchases")]
    private void RestorePurchases()
    {
        _iAPManager.RestorePurchases();
    }

    public static bool IsNetworkAvailable()
    {
        try
        {
            using Ping ping = new Ping();
            string host = "google.com"; // Can use any reliable host
            PingReply reply = ping.Send(host, 1000); // Timeout of 1000 milliseconds
            return reply.Status == IPStatus.Success;
        }
        catch (Exception)
        {
            return false;
        }
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
        ShopView.OnRestoreClicked += RestorePurchases;
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
        ShopView.OnRestoreClicked -= RestorePurchases;

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
        GameDataService.DeleteGame();
        Session.IsGameWon = false;
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