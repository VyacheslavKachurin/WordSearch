using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleProvider : MonoBehaviour
{
    [SerializeField] private ParticleSystem _wordFoundFX;

    public static bool IsAnimating;

    void Start()
    {
        LevelView.OnWordFound += PlayWordFoundFX;
        WordFX.OnAnimDone += RemoveWordFX;

    }


    private void RemoveWordFX()
    {
        IsAnimating = false;
        Debug.Log($"animating done");
    }

    void OnDestroy()
    {
        LevelView.OnWordFound -= PlayWordFoundFX;
        WordFX.OnAnimDone -= RemoveWordFX;
        IsAnimating = false;
    }

    private void PlayWordFoundFX(Vector2 worldPos)
    {
        var fx = Instantiate(_wordFoundFX, worldPos, Quaternion.identity);
        fx.Play();
    }

}