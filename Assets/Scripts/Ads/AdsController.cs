using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdsController : MonoBehaviour
{
    public static AdsController Instance;


    // [SerializeField] private YandexAds _yandexAds;
    [SerializeField] private GoogleAds _googleAds;
    // [SerializeField] private UnityAds _unityAds;

    private List<IAdsProvider> _adsProviders = new List<IAdsProvider>();

    private bool _isBannerShowing;
    public bool IsInterstitialShown => _googleAds.IsInterstitialShown;


    void Awake()
    {
        Instance = this;
        if (ProgressService.AdsRemoved)
        {
            return;
        }


        AdsEventManager.OnBannerLoaded += HandleBannerLoaded;
        AdsEventManager.OnBannerLoadFailed += HandleBannerFailedToLoad;

        // _adsProviders.Add(_unityAds);
        // _adsProviders.Add(_yandexAds);
        _adsProviders.Add(_googleAds);

        EventManager.AdsRemoved += HideBanners;
        // _unityAds.InitializeAds();

        //  _yandexAds.LoadBanner();
        _googleAds.LoadBanner();


    }


    private void Start()
    {
        _googleAds.LoadInterstitialAd();

    }

    private void HandleBannerFailedToLoad(IAdsProvider provider)
    {
        // var nextProvider = _adsProviders.Find(x => x != provider);
        var nextProvider = _adsProviders[0];
        nextProvider.LoadBanner();
    }

    private void HandleBannerLoaded(IAdsProvider provider)
    {
        if (ProgressService.AdsRemoved || SceneManager.GetActiveScene().buildIndex == 0)
        {
            HideBanners();
        }
        else if (_isBannerShowing)
        {
            return;
        }
        else
        {
            _isBannerShowing = true;
            provider.ShowBanner();
            //StartTimer(provider);

        }

    }

    private async void StartTimer(IAdsProvider provider)
    {
        var nextProvider = _adsProviders.Find(x => x != provider);
        await Task.Delay(3000);
        nextProvider.LoadBanner();
        _isBannerShowing = false;
        provider.HideBanner();

    }


    public void HideBanners()
    {
        foreach (var provider in _adsProviders)
        {
            provider.HideBanner();
        }
    }

    internal void ShowRewardedAd()
    {

        _googleAds.ShowRewardedAd();
    }



    void OnDestroy()
    {
        HideBanners();
    }

    internal bool IsInterstitialAdReady()
    {
        return _googleAds.IsInterstitialAdReady();
    }

    internal void ShowInterstitialAd()
    {
        _googleAds.ShowInterstitialAd();
    }

    internal void RevealBanner()
    {
        _googleAds.ShowBanner();
    }
}

public interface IAdsProvider
{
    public void OnBannerLoaded();
    public void OnBannerLoadFailed();

    public void ShowBanner();
    public void HideBanner();
    public void LoadBanner();
}

