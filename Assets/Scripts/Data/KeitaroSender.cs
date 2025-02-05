using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public static class KeitaroSender
{
    public const string URL = "https://kvantekh.kyiv.ua/BGnSNmMx";
    public const string USER_ID_KEY = "user_id";

    public static void SendInstall()
    {
        var url = URL + "?action=install";
        SendRequest(url);
    }

    public static void SendPurchase(string purchaseType)
    {
        var url = URL + "?action=purchase" + "&purchase_type=" + purchaseType;
        SendRequest(url);
    }

    internal static void SendAbility(Ability ability)
    {
        var url = URL + "?action=ability_used" + "&ability=" + ability.ToString();
        SendRequest(url);
    }

    internal static void SendLevelReached(int season, int episode, int level)
    {
        var url = URL + "?action=level_reached" + "&season=" + season + "&episode=" + episode + "&level=" + level;
        SendRequest(url);
    }

    private static string GetUserId()
    {
        var userId = PlayerPrefs.GetString(USER_ID_KEY, "");
        if (string.IsNullOrEmpty(userId))
        {
            userId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(USER_ID_KEY, userId);
        }
        return userId;
    }

    private static async void SendRequest(string url)
    {
        #if !UNITY_EDITOR
        var userId = GetUserId();
        url += "&user_id=" + userId;
        Debug.Log($"Sending request: {url} with user id: {userId}");
        HttpClient client = new();
        var result = await client.GetAsync(url);
        Debug.Log($"Result: {result.StatusCode}, {result.Content.ReadAsStringAsync().Result}");
        client.Dispose();
        #endif
    }

}

public enum PurchaseType
{
    Money250,
    Money750,
    Money1600,
    Money3600,
    Money8000,
    NoAds
}