using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;

public class AdsController : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsShowListener, IUnityAdsLoadListener
{
    public static event Action RewardedAdWatched;
    const string _iOSGameId = "5713743";
    const bool _testMode = false;
    private string _gameId;

    [SerializeField] string _interstitialID = "Interstitial_iOS";
    [SerializeField] string _rewardedID = "Rewarded_iOS";
    [SerializeField] string _bannerID = "Banner_iOS";
    [SerializeField] BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;

    private bool _rewardedAdLoaded = false;


    public static AdsController Instance;

    /*
        void Awake()
        {
            Init();
        }
        */
    void OnDestroy()
    {
        //RemoveBanner();
        Debug.Log($"Ads Controller Destroyed");
        if (Instance == this) Instance = null;
    }

    public void Init()
    {

        Debug.Log($"Ads Controller Init");
        if (Instance != null && Instance != this)
        {
            Debug.Log($"Ads Controller Destroyed");
            Destroy(this);
        }
        else
        {
            Instance = this;
            InitializeAds();

            DontDestroyOnLoad(this);
        }
        LoadBanner();
    }

    /*
        private void OnDestroy()
        {

            if (Instance == this) Instance = null;
        }
        */

    public void InitializeAds()
    {

        _gameId = _iOSGameId;
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }
        LoadRewardedAd();
        LoadBanner();
    }

    public void LoadBanner()
    {
        if (Session.NoAds) return;
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
        if (Session.NoAds)
        {
            RemoveBanner();
            return;
        }

        BannerOptions options = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };

        Advertisement.Banner.Show(_bannerID);
        //LoadBanner();
    }


    public void RemoveBanner()
    {
        if (Advertisement.Banner.isLoaded)
        {
            Advertisement.Banner.Hide();
        }
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
        if (Session.NoAds) return;
        Debug.Log($"Showing Ad: {_interstitialID}");
        Advertisement.Show(_interstitialID, this);
    }

    public void LoadRewardedAd()
    {
        // if (Session.NoAds) return;

        Advertisement.Load(_rewardedID, this);
        Debug.Log($"Loading Ad: {_rewardedID}");
    }


    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        LoadRewardedAd();
        LoadInterstitialAd();
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
            RewardedAdWatched?.Invoke();
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
            Debug.Log($"Showing Ad: {_rewardedID}");
            Advertisement.Show(_rewardedID, this);
            _rewardedAdLoaded = false;
            LoadRewardedAd();
        }
        else
        {
            Debug.Log($"No Ad Loaded: {_rewardedID}");
            LoadRewardedAd();
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"OnUnityAdsFailedToLoad: {placementId} - {error.ToString()} - {message}");
    }
}