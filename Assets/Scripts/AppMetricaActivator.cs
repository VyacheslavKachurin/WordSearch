using System.Threading.Tasks;
using Io.AppMetrica;
using Io.AppMetrica.Push;
using UnityEngine;

public static class AppMetricaActivator
{
    private const string APIKey = "cc6209ab-13e7-4cd3-94f6-815f82224135";
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Activate()
    {
        Services.InitUserId();

        AppMetrica.Activate(new AppMetricaConfig(APIKey)
        {
            FirstActivationAsUpdate = !Session.IsFirstTime
        });

        if (Session.IsFirstTime)
        {
            Session.IsFirstTime = false;
            KeitaroSender.SendInstall();
        }
    }
}