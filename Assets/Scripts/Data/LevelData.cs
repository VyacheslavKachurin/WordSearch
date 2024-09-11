
using System.Collections.Generic;

public class LevelData
{
    public int Level;
    public string Theme;
    public List<string> Words;
    public char[,] Matrix;
    public int Width;
    public int Height;
    public Country Country;

    public char this[int y, int x] => Matrix[y, x];

    public List<Point> FakeLetters;
    public List<Point> FirstLetters;

}


public enum Country { France, Spain, Portugal }

public class Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}