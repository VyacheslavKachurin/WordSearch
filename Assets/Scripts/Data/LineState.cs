using System.Collections.Generic;
using Vector2 = UnityEngine.Vector2;
using Color = UnityEngine.Color;
using UnityEngine.Scripting;

[System.Serializable]
[Preserve]
public class LineState
{
    [Preserve]
    public List<Vector2> Positions;
    [Preserve]
    public Color color;
    [Preserve]

    public LineState(List<Vector2> points, Color color)
    {
        Positions = points;
        this.color = color;
    }
}