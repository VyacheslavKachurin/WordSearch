using System;
using System.Collections.Generic;

public static class AdsEventManager
{
    private static Dictionary<AdsEvent, Action<IAdsProvider>> _events = new Dictionary<AdsEvent, Action<IAdsProvider>>();
    public static event Action<IAdsProvider> RewardedAdWatched; 
    public static event Action<IAdsProvider> OnBannerLoaded;
    public static event Action<IAdsProvider> OnBannerLoadFailed;
    public static event Action<IAdsProvider> InterstitialAdWatched;

    static AdsEventManager()
    {
        _events.Add(AdsEvent.BannerLoaded, provider => OnBannerLoaded?.Invoke(provider));
        _events.Add(AdsEvent.BannerFailed, provider => OnBannerLoadFailed?.Invoke(provider));
        _events.Add(AdsEvent.RewardedAdWatched, provider => RewardedAdWatched?.Invoke(provider));
        _events.Add(AdsEvent.InterstitialAdWatched, provider => InterstitialAdWatched?.Invoke(provider));
    }

    internal static void TriggerEvent(AdsEvent bannerLoaded, IAdsProvider provider)
    {
        _events[bannerLoaded].Invoke(provider);
    }
}

public enum AdsEvent
{
    BannerLoaded,
    BannerFailed,
    RewardedAdWatched,
    InterstitialAdWatched
}