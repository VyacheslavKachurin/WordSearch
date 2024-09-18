using System.Collections.Generic;
using Vector2 = UnityEngine.Vector2;
using Color = UnityEngine.Color;

[System.Serializable]
public class LineState
{
    public List<Vector2> Positions;
    public Color color;

    public LineState(List<Vector2> points, Color color)
    {
        Positions = points;
        this.color = color;
    }
}