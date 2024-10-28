
using System.Collections.Generic;

using UnityEngine;

public class LevelData
{
    public int Level;
    public string Subject;

    public List<string> Words;
    public char[,] Matrix;
    public int Width;
    public int Height;
    public string Country;

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

    public Vector2 GetVector()
    {
        return new Vector2(X, Y);
    }
}