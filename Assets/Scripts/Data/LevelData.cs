using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class LevelData
{
    public string Subject { get; set; }
    public WordData[] Words { get; set; }
    public char[,] Matrix { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public List<Point> FakeLetters { get; set; }
    public char this[int x, int y] => Matrix[x, y];

    [Preserve]
    public LevelData(string subject, WordData[] words, char[,] matrix, int width, int height, List<Point> fakeLetters)
    {
        Subject = subject;
        Words = words;
        Matrix = matrix;
        Width = width;
        Height = height;
        FakeLetters = fakeLetters;
    }
}

[Preserve]
public class WordData
{
    [Preserve]
    public WordData(string word, Point firstLetter, Point lastLetter)
    {
        Word = word;
        FirstLetter = firstLetter;
        LastLetter = lastLetter;
    }

    public string Word { get; set; }
    public Point FirstLetter { get; set; }
    public Point LastLetter { get; set; }
}

public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(Point a, Point b)
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(Point a, Point b)
    {
        return a.X != b.X || a.Y != b.Y;
    }

    public override bool Equals(object obj)
    {
        if (obj is Point other)
        {
            return this == other;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode();
    }

}