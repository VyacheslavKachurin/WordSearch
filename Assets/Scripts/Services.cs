using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;




public class Services : MonoBehaviour
{

    // private IAPManager _iAPManager;
    public static Services Instance;

    private void Awake()
    {
        Instance = this;
        // InitPurchases();
    }


    public static void IsNetworkAvailable(Action<bool> syncResult)
    {
        var result = false;
        Instance.StartCoroutine(CheckInternetConnection((r) =>
        {
            result = r;
            syncResult(result);
        }
        ));


    }


    public static IEnumerator CheckInternetConnection(Action<bool> syncResult)
    {
        const string echoServer = "https://google.com";

        bool result;
        using (var request = UnityWebRequest.Get(echoServer))
        {

            request.timeout = 3;
            yield return request.SendWebRequest();
            //  result = !request.isNetworkError && !request.isHttpError && request.responseCode == 200;
            result = request.result == UnityWebRequest.Result.Success;
        }
        syncResult(result);
    }

    [ContextMenu("Delete State")]
    private void DeleteState()
    {
        LevelStateService.DeleteState();
    }

    //[ContextMenu("Init Purchases")]
    // public async void InitPurchases()
    //   {
    //  await InitUGS();
    //    InitIAPManager();
    //  LogStoreItems();
    /*
            ShopView.OnPurchaseInit += BuyCoins;
            ShopView.OnRemoveAdsClicked += RemoveAds;
            ShopView.OnRestoreClicked += RestorePurchases;
            */
    //  }

    /*
        [ContextMenu("Log Store Items")]
        private void LogStoreItems()
        {
            _iAPManager.LogStoreItems();
        }
        */

    /*
        [ContextMenu("Buy Coins")]
        private void BuyCoins(int i)
        {
            _iAPManager.BuyCoins(i);
        }
        */

    private void OnDestroy()
    {
        /*
        ShopView.OnPurchaseInit -= BuyCoins;
        ShopView.OnRemoveAdsClicked -= RemoveAds;
        ShopView.OnRestoreClicked -= RestorePurchases;
        */
        Instance = null;

    }

    /*
        private void RemoveAds()
        {
            Debug.Log($"Remove Ads");
            _iAPManager.RemoveAds();
        }
        */

    /*
        private void InitIAPManager()
        {
            _iAPManager = new IAPManager();
        }
        */

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
        Session.IsFirstTime = true;
        GameDataService.DeleteStampData();

        
    }

    [ContextMenu("Clear Gift Data")]
    public void ClearGiftData()
    {
        Session.ClearGift();
    }



}