using System;
using com.unity3d.mediation;
using UnityEngine;
using UnityEngine.UIElements;

public class AdsMenu : MonoBehaviour
{
    private VisualElement _root;
    private Label _coinsLbl;
    [SerializeField] string _appKey = "1fbed2a35";
    private int _coins = 0;

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _coinsLbl = _root.Q<Label>("coins-lbl");
        _root.Q<Button>("LoadBannerBtn").RegisterCallback<ClickEvent>(HandleLoadBanner);
        _root.Q<Button>("LoadRewardedBtn").RegisterCallback<ClickEvent>(LoadRewarded);
        _root.Q<Button>("LoadInterstitialBtn").RegisterCallback<ClickEvent>(LoadInterstitial);
        _root.Q<Button>("ShowBannerBtn").RegisterCallback<ClickEvent>(ShowBanner);
        _root.Q<Button>("ShowRewardedBtn").RegisterCallback<ClickEvent>(ShowRewarded);
        _root.Q<Button>("ShowInterstitialBtn").RegisterCallback<ClickEvent>(ShowInterstitial);

        InitIronSource();
    }

    private void InitIronSource()
    {
        IronSource.Agent.validateIntegration();
        IronSource.Agent.init(_appKey);
    }

    private void OnEnable()
    {
        IronSourceEvents.onSdkInitializationCompletedEvent += SDKInitialized;

        //Add AdInfo Banner Events
        IronSourceBannerEvents.onAdLoadedEvent += BannerOnAdLoadedEvent;
        IronSourceBannerEvents.onAdLoadFailedEvent += BannerOnAdLoadFailedEvent;
        IronSourceBannerEvents.onAdClickedEvent += BannerOnAdClickedEvent;
        IronSourceBannerEvents.onAdScreenPresentedEvent += BannerOnAdScreenPresentedEvent;
        IronSourceBannerEvents.onAdScreenDismissedEvent += BannerOnAdScreenDismissedEvent;
        IronSourceBannerEvents.onAdLeftApplicationEvent += BannerOnAdLeftApplicationEvent;

        //Add AdInfo Interstitial Events
        IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
        IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
        IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
        IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
        IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialOnAdShowSucceededEvent;
        IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
        IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;

        //Add AdInfo Rewarded Video Events
        IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;

    }

    /************* Interstitial AdInfo Delegates *************/
    // Invoked when the interstitial ad was loaded succesfully.
    void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo)
    {
    }
    // Invoked when the initialization process has failed.
    void InterstitialOnAdLoadFailed(IronSourceError ironSourceError)
    {
        Debug.Log($"unity-script: I got InterstitialOnAdLoadFailed With Error " + ironSourceError.getDescription());
    }
    // Invoked when the Interstitial Ad Unit has opened. This is the impression indication. 
    void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo)
    {
    }
    // Invoked when end user clicked on the interstitial ad
    void InterstitialOnAdClickedEvent(IronSourceAdInfo adInfo)
    {
    }
    // Invoked when the ad failed to show.
    void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
    {
    }
    // Invoked when the interstitial ad closed and the user went back to the application screen.
    void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo)
    {
    }
    // Invoked before the interstitial ad was opened, and before the InterstitialOnAdOpenedEvent is reported.
    // This callback is not supported by all networks, and we recommend using it only if  
    // it's supported by all networks you included in your build. 
    void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo adInfo)
    {
    }

    /************* Banner AdInfo Delegates *************/
    //Invoked once the banner has loaded
    void BannerOnAdLoadedEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log($"unity-script: I got BannerOnAdLoadedEvent With AdInfo " + adInfo.ToString());
    }
    //Invoked when the banner loading process has failed.
    void BannerOnAdLoadFailedEvent(IronSourceError ironSourceError)
    {
        Debug.Log($"unity-script: I got BannerOnAdLoadFailedEvent With Error " + ironSourceError.getDescription());
    }
    // Invoked when end user clicks on the banner ad
    void BannerOnAdClickedEvent(IronSourceAdInfo adInfo)
    {
    }
    //Notifies the presentation of a full screen content following user click
    void BannerOnAdScreenPresentedEvent(IronSourceAdInfo adInfo)
    {
    }
    //Notifies the presented screen has been dismissed
    void BannerOnAdScreenDismissedEvent(IronSourceAdInfo adInfo)
    {
    }
    //Invoked when the user leaves the app
    void BannerOnAdLeftApplicationEvent(IronSourceAdInfo adInfo)
    {
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        IronSource.Agent.onApplicationPause(pauseStatus);
    }


    /************* RewardedVideo AdInfo Delegates *************/
    // Indicates that there’s an available ad.
    // The adInfo object includes information about the ad that was loaded successfully
    // This replaces the RewardedVideoAvailabilityChangedEvent(true) event
    void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
    {
    }
    // Indicates that no ads are available to be displayed
    // This replaces the RewardedVideoAvailabilityChangedEvent(false) event
    void RewardedVideoOnAdUnavailable()
    {
        Debug.Log($"unity-script: I got RewardedVideoOnAdUnavailable");
    }
    // The Rewarded Video ad view has opened. Your activity will loose focus.
    void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
    {
    }
    // The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
    void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
    {
    }
    // The user completed to watch the video, and should be rewarded.
    // The placement parameter will include the reward data.
    // When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
    void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
    {
        _coins += 25;
        _coinsLbl.text = $"Coins: {_coins.ToString()}";
    }
    // The rewarded video ad was failed to show.
    void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
    {
        Debug.Log($"unity-script: I got RewardedVideoOnAdShowFailedEvent With Error " + error.getDescription());
    }
    // Invoked when the video ad was clicked.
    // This callback is not supported by all networks, and we recommend using it only if
    // it’s supported by all networks you included in your build.
    void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
    {
    }


    private void SDKInitialized()
    {
        print("SDK Initialized");
    }

    private void ShowInterstitial(ClickEvent evt)
    {
        if (IronSource.Agent.isInterstitialReady())
            IronSource.Agent.showInterstitial();
        else Debug.Log("unity-script: IronSource.Agent.isInterstitialReady - False");
    }


    private void ShowRewarded(ClickEvent evt)
    {
        if (IronSource.Agent.isRewardedVideoAvailable())
            IronSource.Agent.showRewardedVideo();
        else Debug.Log("unity-script: IronSource.Agent.isRewardedVideoAvailable - False");
    }

    private void ShowBanner(ClickEvent evt)
    {
       Debug.Log($"delete banner clicked");
    }

    private void LoadInterstitial(ClickEvent evt)
    {
        IronSource.Agent.loadInterstitial();
    }

    private void LoadRewarded(ClickEvent evt)
    {
        IronSource.Agent.loadRewardedVideo();
    }

    private void HandleLoadBanner(ClickEvent evt)
    {
        IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);

    }

    private void DestroyBanner()
    {
        IronSource.Agent.destroyBanner();
    }


}