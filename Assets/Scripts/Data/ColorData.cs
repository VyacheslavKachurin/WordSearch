using System;
using UnityEngine;

[CreateAssetMenu]
public class ColorData : ScriptableObject
{
    public Color[] Colors;

    internal Color GetRandom()
    {
        return Colors[UnityEngine.Random.Range(0, Colors.Length)];
    }
}