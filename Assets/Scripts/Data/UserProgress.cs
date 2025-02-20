using UnityEngine.Scripting;

[Preserve]
public class UserProgress
{
    public int Level { get; set; }
    public int Season { get; set; }
    public int Episode { get; set; }
    public int TotalEpisodes { get; set; }
    public int Coins { get; set; }
    public bool AdsRemoved { get; set; }

    [Preserve]
    public UserProgress(int season = 1, int level = 1, int totalEpisodes = 4, int episode = 1, int coins = 300, bool adsRemoved = false)
    {
        Season = season;
        Level = level;
        TotalEpisodes = totalEpisodes;
        Episode = episode;
        Coins = coins;
        AdsRemoved = adsRemoved;
    }


}