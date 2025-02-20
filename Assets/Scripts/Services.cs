using System;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;




public class Services : MonoBehaviour
{
    public static Services Instance;
    const string FULL_ACCESS_KEY = "FULL_ACCESS_KEY";
    const string USER_ID = "USER_ID";

    const string HAVE_RESOURCES = "HAVE_RESOURCES";
    public static string UserId;
    private static string _hostUrl;

    private void Awake()
    {
        Instance = this;
    }

    public static string GetHostUrl()
    {
        if (string.IsNullOrEmpty(_hostUrl))
        {
            var hostData = Resources.Load<HostData>("HostData");
            _hostUrl = hostData.Host;
        }
        return _hostUrl;
    }



    public static void IsNetworkAvailable(Action<bool> syncResult)
    {
        var result = false;
        Instance.StartCoroutine(CheckInternetConnection((r) =>
        {
            result = r;
            syncResult(result);
        }
        ));
    }


    public static IEnumerator CheckInternetConnection(Action<bool> syncResult)
    {
        string echoServer = GetHostUrl();

        bool result;
        using (var request = UnityWebRequest.Get(echoServer))
        {
            request.timeout = 3;
            yield return request.SendWebRequest();
            //  result = !request.isNetworkError && !request.isHttpError && request.responseCode == 200;
            result = request.result == UnityWebRequest.Result.Success;
        }
        syncResult(result);
    }

    [ContextMenu("Delete State")]
    private void DeleteState()
    {
        LevelStateService.DeleteState();
    }

    private void OnDestroy()
    {
        Instance = null;
    }


    [ContextMenu("Delete Have Resources")]
    private void DeleteHaveResources()
    {
        PlayerPrefs.DeleteKey(HAVE_RESOURCES);
    }

    [ContextMenu("Clear All Data")]
    public void ClearAllData()
    {

        ClearGiftData();
        DeleteState();
        ProgressService.ClearProgress();
        ProgressService.ClearProgress();
        Session.IsGameWon = false;
        Session.IsFirstTime = true;
        ProgressService.DeleteStampData();

    }

    public static void ClearLevelData()
    {
        Session.IsGameWon = false;
        Session.IsFirstTime = true;

        ProgressService.ClearProgress();
        ProgressService.ClearProgress();

        LevelStateService.DeleteState();
    }

    [ContextMenu("Clear Gift Data")]
    public void ClearGiftData()
    {
        Session.ClearGift();
    }

    public static void ExtendAccess()
    {
        PlayerPrefs.SetInt(FULL_ACCESS_KEY, 1);
    }

    public static bool HasFullAccess()
    {
        return PlayerPrefs.GetInt(FULL_ACCESS_KEY, 0) == 1;
    }

    internal static void LowerAccess()
    {
        PlayerPrefs.SetInt(FULL_ACCESS_KEY, 0);
    }

    internal static void InitUserId()
    {
        if (UserId != null) return;
        KeyChainJson storedJson = null;
        try
        {
            storedJson = JsonConvert.DeserializeObject<KeyChainJson>(KeyChain.BindGetKeyChainUser());
        }
        catch
        {
            storedJson = null;
        }

        var id = PlayerPrefs.GetString(USER_ID, storedJson?.uuid ?? "");
        if (string.IsNullOrWhiteSpace(id))
            id = CreateUserId();
        UserId = id;
    }

    private static string CreateUserId()
    {
        var uuid = SystemInfo.deviceUniqueIdentifier;
#if !UNITY_EDITOR
        KeyChain.BindSetKeyChainUser("0", uuid);
#endif
        PlayerPrefs.SetString(USER_ID, uuid);
        return uuid;
    }

    private static string GetUserProgress()
    {
        return "";
    }

    internal static void DeleteStoredFiles()
    {
        // LevelStateService.DeleteState();
        ProgressService.ClearProgress();
        ProgressService.ClearProgress();
    }

    public static async Task<bool> CheckConnection()
    {
        using (var request = UnityWebRequest.Get(GetHostUrl()))
        {
            request.timeout = 5;
            var result = request.SendWebRequest();
            while (!result.isDone)
                await Task.Yield();
            return request.result == UnityWebRequest.Result.Success;
        }
    }

    internal static bool HaveResources()
    {
        return PlayerPrefs.GetInt(HAVE_RESOURCES, 0) == 1;
    }

    internal static void SetHaveResources()
    {
        PlayerPrefs.SetInt(HAVE_RESOURCES, 1);
    }
}

[Preserve]
public class KeyChainJson
{
    [Preserve]
    public string userId { get; set; }
    [Preserve]
    public string uuid { get; set; }
    [Preserve]
    public KeyChainJson() { }

    [Preserve]
    public KeyChainJson(string userId, string uuid)
    {
        this.userId = userId;
        this.uuid = uuid;
    }
}