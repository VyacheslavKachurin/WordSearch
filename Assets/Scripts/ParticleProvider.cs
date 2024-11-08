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


    [ContextMenu("Log particles")]
    private void LogParticles()
    {

        Debug.Log($"Active particles: {IsAnimating}");
    }

    private void RemoveWordFX()
    {
        IsAnimating = false;
    }

    void OnDestroy()
    {
        LevelView.OnWordFound -= PlayWordFoundFX;
        WordFX.OnAnimDone -= RemoveWordFX;
    }

    private void PlayWordFoundFX(Vector2 worldPos)
    {
        var fx = Instantiate(_wordFoundFX, worldPos, Quaternion.identity);
        fx.Play();
    }

}