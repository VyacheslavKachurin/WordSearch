using System;
using UnityEngine;

[CreateAssetMenu]
public class ColorData : ScriptableObject
{
    public Color[] Colors;

    internal Color GetRandom()
    {

        var color = Colors[UnityEngine.Random.Range(0, Colors.Length)];
        if (LineProvider.LastColor == color) return GetRandom();
        else LineProvider.LastColor = color;
        return color;
    }
}