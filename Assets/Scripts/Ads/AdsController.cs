using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdsController : MonoBehaviour
{
    public static AdsController Instance;

    [SerializeField] private YandexAds _yandexAds;
    // [SerializeField] private UnityAds _unityAds;

    private List<IAdsProvider> _adsProviders = new List<IAdsProvider>();

    private bool _isBannerShowing;



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
        _adsProviders.Add(_yandexAds);

        EventManager.AdsRemoved += HideBanners;
        // _unityAds.InitializeAds();

        _yandexAds.LoadBanner();


    }

    private void Start()
    {
        _yandexAds.LoadRewardedAd();
    }

    private void HandleBannerFailedToLoad(IAdsProvider provider)
    {
        var nextProvider = _adsProviders.Find(x => x != provider);
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
            // StartTimer(provider);

        }

    }

    private async void StartTimer(IAdsProvider provider)
    {
        var nextProvider = _adsProviders.Find(x => x != provider);
        nextProvider.LoadBanner();
        await Task.Delay(3000);
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
        _yandexAds.ShowRewardedAd();
    }



    void OnDestroy()
    {
        HideBanners();
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

public static class AdsEventManager
{
    private static Dictionary<AdsEvent, Action<IAdsProvider>> _events = new Dictionary<AdsEvent, Action<IAdsProvider>>();

    public static event Action<IAdsProvider> OnBannerLoaded;
    public static event Action<IAdsProvider> OnBannerLoadFailed;

    static AdsEventManager()
    {
        _events.Add(AdsEvent.BannerLoaded, provider => OnBannerLoaded?.Invoke(provider));
        _events.Add(AdsEvent.BannerFailed, provider => OnBannerLoadFailed?.Invoke(provider));
    }

    internal static void TriggerEvent(AdsEvent bannerLoaded, IAdsProvider provider)
    {
        _events[bannerLoaded].Invoke(provider);
    }
}

public enum AdsEvent
{
    BannerLoaded,
    BannerFailed
}