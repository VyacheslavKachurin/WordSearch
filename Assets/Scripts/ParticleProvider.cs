using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleProvider : MonoBehaviour
{
    [SerializeField] private ParticleSystem _wordFoundFX;

    public static int ActiveParticles { get; private set; } = 0;



    void Start()
    {
        LevelView.OnWordFound += PlayWordFoundFX;
        WordFX.OnAnimDone += RemoveWordFX;
        LevelLogic.OnWordFound += CountLetters;

    }

    private void CountLetters(List<Point> list)
    {
        ActiveParticles += list.Count;
    }

    private void RemoveWordFX()
    {
        ActiveParticles--;
    }

    void OnDestroy()
    {
        LevelView.OnWordFound -= PlayWordFoundFX;
        WordFX.OnAnimDone -= RemoveWordFX;
        LevelLogic.OnWordFound -= CountLetters;
    }

    private void PlayWordFoundFX(Vector2 endPos)
    {
        var worldPos = Camera.main.ScreenToWorldPoint(new Vector2(endPos.x, LevelView.RootHeight - endPos.y));
        var fx = Instantiate(_wordFoundFX, worldPos, Quaternion.identity);
        fx.Play();
    }

}