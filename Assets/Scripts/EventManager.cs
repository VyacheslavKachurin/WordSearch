using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class EventManager
{
    public static event Action RemoveAdsClicked;
    public static event Action AdsRemoved;
    public static event Action PurchaseFailed;
    public static event Action<int> PurchaseClicked;
    public static event Action RestoreClicked;

    public static event Action<IAdsProvider> BannerLoaded;
    public static event Action BannerFailed;

    private static Dictionary<EventType, Action> _events = new Dictionary<EventType, Action>();


    static EventManager()
    {
        _events = new Dictionary<EventType, Action>()
        {
            {EventType.RemoveAdsRequest,delegate() {RemoveAdsClicked?.Invoke();}},
            {EventType.AdsRemoved,delegate() {AdsRemoved?.Invoke();}},
            {EventType.PurchaseFailed,delegate() {PurchaseFailed?.Invoke();}},
            {EventType.RestoreClick,delegate() {RestoreClicked?.Invoke();}},

        };
    }

    internal static void TriggerEvent(EventType eventType)
    {
        _events[eventType].Invoke();
    }

    public static void TriggerPurchase(int index) => PurchaseClicked?.Invoke(index);

    public static async Task TriggerBannerDisplayed()
    {
        await Task.Delay(3000);
        TriggerEvent(EventType.BannerDisplayed);
    }

}

public enum EventType
{
    RemoveAdsRequest,
    AdsRemoved,
    PurchaseInit,
    RestoreClick,
    PurchaseFailed,
    BannerDisplayed,
}
