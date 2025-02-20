using System.Collections.Generic;
using UnityEngine;

public class FoundWord
{
    public string Word { get; set; }
    public List<Point> Points { get; set; }
    public Color Color { get; set; }

    public FoundWord(string word, List<Point> points, Color color)
    {
        Word = word;
        Points = points;
        Color = color;
    }
}