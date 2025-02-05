
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class LevelData
{

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
    public char this[int y, int x] => Matrix[y, x];
    [Preserve]
    public List<Point> FakeLetters;
    [Preserve]
    public List<Point> FirstLetters;

    [Preserve]
    public List<Point> LastLetters;




    [Preserve]
    public LevelData(string subject, List<string> words, char[,] matrix, int width, int height, List<Point> fakeLetters, List<Point> firstLetters, List<Point> lastLetters)
    {
        Subject = subject;
        Words = words;
        Matrix = matrix;
        Width = width;
        Height = height;
        FakeLetters = fakeLetters;
        FirstLetters = firstLetters;
        LastLetters = lastLetters;
    }

    [Preserve]
    public LevelData() { }

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