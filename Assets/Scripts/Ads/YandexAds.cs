using System;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

public class YandexAds : MonoBehaviour, IAdsProvider
{
    // public static event Action RewardedAdWatched;
    private Banner _banner;
    [SerializeField] private string _testUnitId = "demo-banner-yandex";
    [SerializeField] private string _realUnitId = "R-M-14293027-1";
    [SerializeField] private bool _isTestMode = true;
    private string _adUnitId;

    private RewardedAdLoader rewardedAdLoader;
    private RewardedAd rewardedAd;


    void Awake()
    {
        _adUnitId = _isTestMode ? _testUnitId : _realUnitId;

        BannerAdSize bannerAdSize = BannerAdSize.FixedSize(GetScreenWidthDp(), GetScreenHeightDp());
        _banner = new Banner(_adUnitId, bannerAdSize, AdPosition.BottomCenter);

        _banner.OnAdLoaded += HandleAdLoaded;
        // Вызывается, если во время загрузки произошла ошибка
        _banner.OnAdFailedToLoad += HandleAdFailedToLoad;

        // Вызывается, когда приложение становится неактивным, так как пользователь кликнул на рекламу и сейчас перейдет в другое приложение (например, браузер).
        _banner.OnLeftApplication += HandleLeftApplication;

        // Вызывается, когда пользователь возвращается в приложение после клика
        _banner.OnReturnedToApplication += HandleReturnedToApplication;

        // Вызывается, когда пользователь кликнул на рекламу
        _banner.OnAdClicked += HandleAdClicked;

        // Вызывается, когда зарегистрирован показ
        _banner.OnImpression += HandleImpression;



        SetupLoader();

    }

    public void DestroyRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Show();
        }
        else
        {
            LoadRewardedAd();
        }
    }

    private void HandleRewarded(object sender, Reward e)
    {
        AdsEventManager.TriggerEvent(AdsEvent.RewardedAdWatched, this);
        DestroyRewardedAd();
        LoadRewardedAd();
    }

    private void HandleAdDismissed(object sender, EventArgs e)
    {
        Debug.Log($"AdDismissed event received");

        DestroyRewardedAd();
    }

    private void HandleAdFailedToShow(object sender, AdFailureEventArgs e)
    {
        Debug.Log($"AdFailedToShow event received with message: {e.Message}");
        DestroyRewardedAd();
    }

    private void HandleAdShown(object sender, EventArgs e)
    {
        Debug.Log($"AdShown event received");
    }

    private void SetupLoader()
    {
        rewardedAdLoader = new RewardedAdLoader();
        rewardedAdLoader.OnAdLoaded += HandleAdLoaded;
        rewardedAdLoader.OnAdFailedToLoad += HandleAdFailedToLoad;

    }

    public void LoadRewardedAd()
    {
        string _rewardedAdUnitId = "R-M-14293027-2";//"demo-rewarded-yandex"; 
        AdRequestConfiguration adRequestConfiguration = new AdRequestConfiguration.Builder(_rewardedAdUnitId).Build();
        rewardedAdLoader.LoadAd(adRequestConfiguration);
    }


    public void HandleAdLoaded(object sender, RewardedAdLoadedEventArgs args)
    {
        // Rewarded ad was loaded successfully. Now you can handle it.
        rewardedAd = args.RewardedAd;

        rewardedAd.OnAdClicked += HandleAdClicked;
        rewardedAd.OnAdShown += HandleAdShown;
        rewardedAd.OnAdFailedToShow += HandleAdFailedToShow;
        rewardedAd.OnAdImpression += HandleImpression;
        rewardedAd.OnAdDismissed += HandleAdDismissed;
        rewardedAd.OnRewarded += HandleRewarded;
    }

    public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log($"Ad {args.AdUnitId} failed for to load with {args.Message}");
        // Attempting to load new ad from the OnAdFailedToLoad event is strongly discouraged.
    }


    private int GetScreenWidthDp()
    {
        int screenWidth = (int)(Screen.safeArea.width * 0.8f);
        return ScreenUtils.ConvertPixelsToDp(screenWidth);
    }

    private int GetScreenHeightDp()
    {
        int screenHeight = (int)(Screen.safeArea.height * 0.08f);
        return ScreenUtils.ConvertPixelsToDp(screenHeight);
    }

    public void LoadBanner()
    {
        //   BannerAdSize bannerMaxSize = BannerAdSize.StickySize(GetScreenWidthDp());

        AdRequest request = new AdRequest.Builder().Build();
        _banner.LoadAd(request);
    }

    private void HandleAdLoaded(object sender, EventArgs args)
    {
        Debug.Log("AdLoaded event received");
        OnBannerLoaded();
    }

    private void HandleAdFailedToLoad(object sender, AdFailureEventArgs args)
    {
        Debug.Log($"AdFailedToLoad event received with message: {args.Message}");
        OnBannerLoadFailed();
    }

    private void HandleLeftApplication(object sender, EventArgs args)
    {
        Debug.Log("LeftApplication event received");
    }

    private void HandleReturnedToApplication(object sender, EventArgs args)
    {
        Debug.Log("ReturnedToApplication event received");
    }

    private void HandleAdClicked(object sender, EventArgs args)
    {
        Debug.Log("AdClicked event received");
    }

    private void HandleImpression(object sender, ImpressionData impressionData)
    {
        var data = impressionData == null ? "null" : impressionData.rawData;
        Debug.Log($"HandleImpression event received with data: {data}");
    }

    public void ShowBanner()
    {
        _banner.Show();
    }

    public void HideBanner()
    {
        //_banner.Hide();
        _banner.Destroy();
    }

    public void OnBannerLoaded()
    {
        AdsEventManager.TriggerEvent(AdsEvent.BannerLoaded, this);
    }

    public void OnBannerLoadFailed()
    {
        AdsEventManager.TriggerEvent(AdsEvent.BannerFailed, this);
    }
}
