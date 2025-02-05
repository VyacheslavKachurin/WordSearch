using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Backtrace.Unity;
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
    private IShopItems _shopItems;
    [SerializeField] private MenuView _menuView;
    [SerializeField] private LevelView _levelView;
    [SerializeField] BacktraceClient _backtraceClient;

    public static event Action FirePurchaseFailed;

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
        try
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            _catalog = JsonUtility.FromJson<ProductCatalog>(Resources.Load<TextAsset>("IAPProductCatalog").text);
            foreach (var item in _catalog.allProducts)
            {
                builder.AddProduct(item.id, item.type);
            }
            UnityPurchasing.Initialize(this, builder);
        }
        catch (Exception e)
        {
            _backtraceClient.Send(e);
        }
    }


    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        try
        {
            this._controller = controller;
            this._extensions = extensions;
            if (_shopItems != null)
                FillUpShopItems(_shopItems);
            else
            {
                StartCoroutine(FillingUpShopItems(_shopItems));
            }
        }
        catch (Exception e)
        {
            _backtraceClient.Send(e);
        }
    }

    private IEnumerator FillingUpShopItems(IShopItems shopItems)
    {
        while (_shopItems == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        FillUpShopItems(shopItems);
        RestorePurchases();

    }

    public void RestorePurchases()
    {
        try
        {
            _appleExtensions = _extensions.GetExtension<IAppleExtensions>();
            _appleExtensions.RestoreTransactions(TransactionsCallback);
        }
        catch (Exception e)
        {
            _backtraceClient.Send(e);
        }
    }
    private async Task InitUGS()
    {
        await UGS.Init();
    }

    private void TransactionsCallback(bool result, string message)
    {
        try
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
        catch (Exception e)
        {
            _backtraceClient.Send(e);
        }
    }


    public void BuyCoins(int i)
    {
        try
        {
            var product = _catalog.allProducts.ToList()[i];

            _controller.InitiatePurchase(product.id);
        }
        catch (Exception e)
        {
            _backtraceClient.Send(e);
        }
    }

    public void RemoveAds()
    {
        try
        {
            Debug.Log($"try initiate purchase remove ads");
            _controller.InitiatePurchase("no_ads");
        }
        catch (Exception e)
        {
            _backtraceClient.Send(e);
        }
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
        _backtraceClient.Send(new Exception($"IAP Initialize Failed: {error}"));
    }

    /// <summary>
    /// Called when a purchase completes.
    ///
    /// May be called at any time after OnInitialized().
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        try
        {
            var item = _catalog.allProducts.ToList().Find(x => x.id == e.purchasedProduct.definition.id);
            var payout = item.Payouts[0].quantity; Debug.Log($"IAP Purchase payout: {payout}");

            if (item.type == ProductType.Consumable)
            {
                Balance.AddBalance(payout);
                KeitaroSender.SendPurchase(item.id);
                OnPurchasedCoins?.Invoke((int)payout);
            }
            else
            {
                Session.NoAds = true;
                var adsController = AdsController.Instance;
                KeitaroSender.SendPurchase(item.id);
                adsController?.RemoveBanner();
            }

            AudioManager.Instance.PlaySound(Sound.Coins);
        }
        catch (Exception ex)
        {
            _backtraceClient.Send(ex);
        }


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
        _backtraceClient.Send(new Exception($"IAP Purchase Failed: {p}"));
        FirePurchaseFailed?.Invoke();
    }

    /// <summary>
    /// Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product i, PurchaseFailureDescription p)
    {
        Debug.Log($"IAP Purchase Failed: {p.reason} - {p.message}");
        _backtraceClient.Send(new Exception($"IAP Purchase Failed: {p.reason} - {p.message}"));
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log($"IAP Initialize Failed: {error} - {message}");
        _backtraceClient.Send(new Exception($"IAP Initialize Failed: {error} - {message}"));
    }

    internal void LogStoreItems()
    {
        foreach (var item in _controller.products.all)
        {
            var a = item;
            Debug.Log(item.definition.id);
        }
    }


    private void FillUpShopItems(IShopItems shopItems)
    {
        shopItems ??= GetShopItems();
        var items = _catalog.allValidProducts.Where(x => x.type == ProductType.Consumable).ToList();
        var itemsController = _controller.products.all.Where(x => x.definition.type == ProductType.Consumable).ToList();

        for (int i = 0; i < items.Count; i++)
        {
            var coinsAmount = (int)items[i].Payouts[0].quantity;
            var price = itemsController[i].metadata.localizedPriceString;
            var index = i;
            shopItems.AddItem(index, coinsAmount, price);
        }

        var noAds = _controller.products.all.FirstOrDefault(x => x.definition.id == "no_ads");
        if (noAds != null)
        {
            var price = noAds.metadata.localizedPriceString;
            shopItems.SetNoAds(price);
        }
        else Debug.Log($"No Ads not found");
    }

    private IShopItems GetShopItems()
    {
        if (_menuView != null)
            return _menuView.ShopItems;
        else if (_levelView != null)
            return _levelView.ShopItems;
        else
        {
            Debug.Log($"ShopItems not found");
            return null;
        }
    }

    internal void InjectShopItems(IShopItems shopItems)
    {
        _shopItems = shopItems;
    }
}