using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

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
        return Services.UserId;
    }

    private static async void SendRequest(string url)
    {
#if !UNITY_EDITORF
        try
        {
            var userId = GetUserId();
            url += "&user_id=" + userId;
            Debug.Log($"Sending request: {url} with user id: {userId}");
            using UnityWebRequest www = UnityWebRequest.Get(url);
            www.timeout = 5;
            await www.SendWebRequest();
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to send request: {url}, error: {e}");
        }
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