using System;
using System.Collections.Generic;

public static class EventManager
{
    public static event Action RemoveAdsClicked;
    public static event Action AdsRemoved;
    public static event Action PurchaseFailed;
    public static event Action<int> PurchaseClicked;
    public static event Action RestoreClicked;

    private static Dictionary<Event, Action> _events = new Dictionary<Event, Action>();


    static EventManager()
    {
        _events = new Dictionary<Event, Action>()
        {
            {Event.RemoveAdsRequest,delegate() {RemoveAdsClicked?.Invoke();}},
            {Event.AdsRemoved,delegate() {AdsRemoved?.Invoke();}},
            {Event.PurchaseFailed,delegate() {PurchaseFailed?.Invoke();}},
            {Event.RestoreClick,delegate() {RestoreClicked?.Invoke();}}
        };
    }

    internal static void TriggerEvent(Event eventType)
    {
        _events[eventType].Invoke();
    }

    public static void TriggerPurchase(int index) => PurchaseClicked?.Invoke(index);
}

public enum Event
{
    RemoveAdsRequest,
    AdsRemoved,
    PurchaseInit,
    RestoreClick,
    PurchaseFailed
}
