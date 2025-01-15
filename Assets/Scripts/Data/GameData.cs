using UnityEngine.Scripting;

public class GameData
{
    [Preserve]
    public int Season { get; set; }
    [Preserve]
    public int Level { get; set; }
    [Preserve]
    public int TotalEpisodes { get; set; }
    [Preserve]
    public int Episode { get; set; }
    [Preserve]
    public GameData(int season, int level, int totalEpisodes, int episode)
    {
        Season = season;
        Level = level;
        TotalEpisodes = totalEpisodes;
        Episode = episode;
    }

}