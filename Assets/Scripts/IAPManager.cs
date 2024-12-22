using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour, IDetailedStoreListener
{

    private IStoreController _controller;
    private IExtensionProvider _extensions;
    private ProductCatalog _catalog;
    public static event Action<int> OnPurchasedCoins;

    private IAppleExtensions _appleExtensions;

    private async void Awake()
    {
        await InitUGS();
        Init();

        ShopView.OnPurchaseInit += BuyCoins;
        ShopView.OnRemoveAdsClicked += RemoveAds;
        ShopView.OnRestoreClicked += RestorePurchases;
    }

    private void OnDestroy()
    {
        ShopView.OnPurchaseInit -= BuyCoins;
        ShopView.OnRemoveAdsClicked -= RemoveAds;
        ShopView.OnRestoreClicked -= RestorePurchases;

    }

    private void Init()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        _catalog = JsonUtility.FromJson<ProductCatalog>(Resources.Load<TextAsset>("IAPProductCatalog").text);
        foreach (var item in _catalog.allProducts)
        {
            builder.AddProduct(item.id, item.type);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this._controller = controller;
        this._extensions = extensions;

    }

    public void RestorePurchases()
    {
        _appleExtensions = _extensions.GetExtension<IAppleExtensions>();
        _appleExtensions.RestoreTransactions(TransactionsCallback);
    }
    private async Task InitUGS()
    {
        await UGS.Init();
    }

    private void TransactionsCallback(bool result, string message)
    {
        if (result)
        {
            _appleExtensions.RefreshAppReceipt((success) =>
            {
                Debug.Log($"Refresh App Receipt success: {success}");
            }, (error) =>
            {
                Debug.Log($"Refresh App Receipt error: {error}");
            }
            );
        }
    }


    public void BuyCoins(int i)
    {

        var product = _catalog.allProducts.ToList()[i];

        _controller.InitiatePurchase(product.id);
    }

    public void RemoveAds()
    {
        Debug.Log($"try initiate purchase remove ads");
        _controller.InitiatePurchase("no_ads");
    }

    /// <summary>
    /// Called when Unity IAP encounters an unrecoverable initialization error.
    ///
    /// Note that this will not be called if Internet is unavailable; Unity IAP
    /// will attempt initialization until it becomes available.
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log($"IAP Initialize Failed: {error}");
    }

    /// <summary>
    /// Called when a purchase completes.
    ///
    /// May be called at any time after OnInitialized().
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        var item = _catalog.allProducts.ToList().Find(x => x.id == e.purchasedProduct.definition.id);
        var payout = item.Payouts[0].quantity; Debug.Log($"IAP Purchase payout: {payout}");

        if (item.type == ProductType.Consumable)
        {
            Balance.AddBalance(payout);
            OnPurchasedCoins?.Invoke((int)payout);
        }
        else
        {
            Session.NoAds = true;
            var adsController = AdsController.Instance;

            adsController?.RemoveBanner();

        }

        AudioManager.Instance.PlaySound(Sound.Coins);


        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    /// Called when a purchase fails.
    /// IStoreListener.OnPurchaseFailed is deprecated,
    /// use IDetailedStoreListener.OnPurchaseFailed instead.
    /// </summary>
    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        Debug.Log($"IAP Purchase Failed: {p}");
    }

    /// <summary>
    /// Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product i, PurchaseFailureDescription p)
    {
        Debug.Log($"IAP Purchase Failed: {p.reason} - {p.message}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log($"IAP Initialize Failed: {error} - {message}");
    }

    internal void LogStoreItems()
    {
        foreach (var item in _controller.products.all)
        {
            var a = item;
            Debug.Log(item.definition.id);
        }
    }

    public void FillUpShopItems(IShopItems shopItems)
    {
        var items = _catalog.allValidProducts.Where(x => x.type == ProductType.Consumable).ToList();
        var itemsController = _controller.products.all.Where(x => x.definition.type == ProductType.Consumable).ToList();

        for (int i = 0; i < items.Count; i++)
        {
            var coinsAmount = (int)items[i].Payouts[0].quantity;
            var price = itemsController[i].metadata.localizedPriceString;
            var index = i;
            shopItems.AddItem(index, coinsAmount, price);
        }

    }



}