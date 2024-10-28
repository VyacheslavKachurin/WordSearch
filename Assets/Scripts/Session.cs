using System;
using UnityEngine;

public static class Session
{

    private const string LAST_CLASSIC_LEVEL = "last-classic-level";
    private const string IS_FIRST_TIME = "is-first-time";
    public static bool IsFirstTime
    {
        get
        {
            return !PlayerPrefs.HasKey(IS_FIRST_TIME);
        }
        set
        {
            PlayerPrefs.SetInt(IS_FIRST_TIME, value ? 1 : 0);
        }
    }

    private static int LastClassicLevel
    {
        get
        {
            var level = 1;
            if (PlayerPrefs.HasKey(LAST_CLASSIC_LEVEL))
            {
                level = PlayerPrefs.GetInt(LAST_CLASSIC_LEVEL);
            }
            return level;
        }
        set
        {
            PlayerPrefs.SetInt(LAST_CLASSIC_LEVEL, value);
        }
    }

    public static int GetLastLevel()
    {
        return LastClassicLevel;
    }

    public static void SetLastLevel(int level)
    {
        LastClassicLevel = level;
    }
}