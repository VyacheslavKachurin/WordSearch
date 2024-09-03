
public class LevelData
{
    public int Level;
    public string Theme;
    public string[] Words;
    public char[,] Matrix;
    public int Width;
    public int Height;
    public Country Country;

    public char this[int y, int x] => Matrix[y, x];

}


public enum Country { France, Spain, Portugal }