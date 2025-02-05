using System;
using UnityEngine;

public static class Session
{
    public static event Action<bool> OnMusicChange;
    public static event Action<bool> OnSoundChange;

    public static event Action AdsRemoved;

    private const string LAST_CLASSIC_LEVEL = "last-classic-level";
    private const string IS_FIRST_TIME = "is-first-time";

    private const string IS_MUSIC_ON = "is-music-on";
    private const string IS_SOUND_ON = "is-sound-on";

    public static string TIMESTAMP_KEY = "timestamp";

    public static bool IsClassicGame;

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
    private const string IS_GAME_WON = "is-game-won";
    public static bool IsGameWon
    {
        get
        {
            return PlayerPrefs.GetInt(IS_GAME_WON, 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(IS_GAME_WON, value ? 1 : 0);
            if (value) AppMetricaService.SendGamePassed();
        }
    }

    /*
        public static int LastStage
        {
            get
            {
                return PlayerPrefs.GetInt(STAGE_KEY, 1);
            }
            set
            {
                PlayerPrefs.SetInt(STAGE_KEY, value);
            }
        }
        */

    public static bool IsSelecting = false;
    public static int RewardAmount = 25;
    /*
        private static int LastClassicLevel
        {
            get
            {
                return PlayerPrefs.GetInt(LAST_CLASSIC_LEVEL, 1);
            }
            set
            {
                PlayerPrefs.SetInt(LAST_CLASSIC_LEVEL, value);
            }
        }
        */

    /*
        public static int GetLastLevel()
        {
            return LastClassicLevel;
        }
        */

    /*
        public static void SetLastLevel(int level)
        {
            LastClassicLevel = level;
        }
        */

    public static bool IsMusicOn
    {
        get
        {
            return PlayerPrefs.GetInt(IS_MUSIC_ON, 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(IS_MUSIC_ON, value ? 1 : 0);
            OnMusicChange?.Invoke(value);
        }
    }

    public static bool IsSoundOn
    {
        get
        {
            return PlayerPrefs.GetInt(IS_SOUND_ON, 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(IS_SOUND_ON, value ? 1 : 0);
            OnSoundChange?.Invoke(value);
        }
    }

    public static bool WasGiftReceived
    {
        get
        {
            return PlayerPrefs.GetString(TIMESTAMP_KEY) == DateTime.Now.ToString("MM-dd");
        }
        set
        {
            PlayerPrefs.SetString(TIMESTAMP_KEY, DateTime.Now.ToString("MM-dd"));
        }
    }

    public static void ClearGift()
    {
        PlayerPrefs.DeleteKey(TIMESTAMP_KEY);
    }

    public static bool NoAds
    {
        get
        {
            return PlayerPrefs.GetInt("no_ads", 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("no_ads", value ? 1 : 0);
            if (value) AdsRemoved?.Invoke();
        }
    }

    public static string AppId = "6736460315";

    public static string GiftTimeLeft()
    {
        DateTime now = DateTime.Now;
        DateTime tomorrow = now.AddDays(1).Date;
        TimeSpan timeLeft = tomorrow - now;
        var answer = $"{timeLeft.Hours}h {timeLeft.Minutes}m {timeLeft.Seconds}s";

        return answer;
    }

    /*
        internal static int GetLastStage()
        {
            return LastStage;
        }
        */
}