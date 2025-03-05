using System;
using Io.AppMetrica;

public static class AppMetricaService
{
    public static void SendAbilityEvent(Ability ability)
    {
        if (!AppMetrica.IsActivated()) return;
        string data = "{\"ability\":\"" + ability.ToString() + "\"}";
        AppMetrica.ReportEvent("Used ability", data);
        KeitaroSender.SendAbility(ability);
    }

    public static void SendLevelReached(int season, int episode, int level)
    {
        if (!AppMetrica.IsActivated()) return;
        string data = "{\"season\":\"" + season + "\",\"episode\":\"" + episode + "\",\"level\":\"" + level + "\"}";
        AppMetrica.ReportEvent("Level reached", data);
       // KeitaroSender.SendLevelReached(season, episode, level);
    }

    public static void SendGamePassed()
    {
        if (!AppMetrica.IsActivated()) return;
        AppMetrica.ReportEvent("Game passed");
    }
}
