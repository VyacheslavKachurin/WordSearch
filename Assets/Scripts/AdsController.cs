using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsController : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsShowListener, IUnityAdsLoadListener
{
    [SerializeField] string _iOSGameId = "5713743";
    [SerializeField] bool _testMode = true;
    private string _gameId;

    [SerializeField] string _interstitialID = "Interstitial_iOS";
    [SerializeField] string _rewardedID = "Rewarded_iOS";
    [SerializeField] string _bannerID = "Banner_iOS";
    [SerializeField] BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;

    private bool _rewardedAdLoaded = false;

    void Awake()
    {
        InitializeAds();
    }

    public void InitializeAds()
    {

        _gameId = _iOSGameId;
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }
    }

    public void LoadBanner()
    {
        Advertisement.Banner.SetPosition(_bannerPosition);
        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.Load(_bannerID, options);

    }

    private void OnBannerError(string message)
    {
        Debug.Log($"Banner Error: {message}");
    }

    private void OnBannerLoaded()
    {
        Debug.Log($"Banner Loaded");
        BannerOptions options = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };

        Advertisement.Banner.Show(_bannerID);
    }

    private void OnBannerShown()
    {
        Debug.Log($"Banner Shown");
    }

    private void OnBannerHidden()
    {
        Debug.Log($"Banner Hidden");
    }

    private void OnBannerClicked()
    {
        Debug.Log($"Banner Clicked");
    }

    public void LoadInterstitialAd()
    {
        Advertisement.Load(_interstitialID, this);
    }

    public void ShowInterstitialAd()
    {
        Debug.Log($"Showing Ad: {_interstitialID}");
        Advertisement.Show(_interstitialID, this);
    }

    public void LoadRewardedAd()
    {
        Advertisement.Load(_rewardedID, this);
        Debug.Log($"Loading Ad: {_rewardedID}");
    }


    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log($"OnUnityAdsShowFailure: {placementId} - {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log($"OnUnityAdsShowStart: {placementId}");
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log($"OnUnityAdsShowClick :{placementId}");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"OnUnityAdsShowComplete: {placementId} - {showCompletionState.ToString()}");

        if (placementId == _rewardedID && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log($"Reward Ad Completed; Grant a reward");
        }
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log($"OnUnityAdsAdLoaded: {placementId}");
        if (placementId == _rewardedID) _rewardedAdLoaded = true;
    }

    public void ShowRewardedAd()
    {
        if (_rewardedAdLoaded)
        {
            Advertisement.Show(_rewardedID,this);
            _rewardedAdLoaded = false;
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"OnUnityAdsFailedToLoad: {placementId} - {error.ToString()} - {message}");
    }
}