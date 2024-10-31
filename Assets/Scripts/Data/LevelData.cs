
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class LevelData
{
    [Preserve]
    public int Level;
    [Preserve]
    public string Subject;
    [Preserve]
    public List<string> Words;
    [Preserve]
    public char[,] Matrix;
    [Preserve]
    public int Width;
    [Preserve]
    public int Height;
    [Preserve]
    public int Stage;
    [Preserve]
    public int Step;
    [Preserve]
    public int TotalSteps;
    [Preserve]

    public char this[int y, int x] => Matrix[y, x];
    [Preserve]
    public List<Point> FakeLetters;
    [Preserve]
    public List<Point> FirstLetters;
    [Preserve]
    public LevelData(int level, string subject, List<string> words, char[,] matrix, int width, int height, int stage, int step, int totalSteps, List<Point> fakeLetters, List<Point> firstLetters)
    {
        Level = level;
        Subject = subject;
        Words = words;
        Matrix = matrix;
        Width = width;
        Height = height;
        Stage = stage;
        Step = step;
        TotalSteps = totalSteps;
        FakeLetters = fakeLetters;
        FirstLetters = firstLetters;
    }
    [Preserve]
    public LevelData() { }

}

[Preserve]
public enum Country { France, Spain, Portugal }
[Preserve]
public class Point
{
    [Preserve]
    public int X { get; set; }
    [Preserve]
    public int Y { get; set; }
    [Preserve]
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