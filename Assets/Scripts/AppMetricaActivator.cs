using Io.AppMetrica;
using Io.AppMetrica.Push;
using UnityEngine;

public static class AppMetricaActivator
{
    private const string APIKey = "cc6209ab-13e7-4cd3-94f6-815f82224135";
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Activate()
    {
        AppMetrica.Activate(new AppMetricaConfig(APIKey)
        {
            FirstActivationAsUpdate = !IsFirstLaunch(),
        });

        Debug.Log("AppMetrica activated");
        Session.IsFirstTime = false;
        AppMetricaPush.Activate();

    }

    private static bool IsFirstLaunch()
    {
        // Implement logic to detect whether the app is opening for the first time.
        // For example, you can check for files (settings, databases, and so on),
        // which the app creates on its first launch.

        return Session.IsFirstTime;

    }
}