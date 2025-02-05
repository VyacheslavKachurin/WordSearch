using UnityEngine;

public class RevealedLetter
{
    public Point Point { get; set; }
    public Color Color { get; set; }

    public RevealedLetter(Point point, Color color)
    {
        Point = point;
        Color = color;
    }
}