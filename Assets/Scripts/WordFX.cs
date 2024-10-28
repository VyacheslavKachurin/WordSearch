using System;
using UnityEngine;

public class WordFX : MonoBehaviour
{
    public static event Action OnAnimDone;
    void Start()
    {
        var main = GetComponent<ParticleSystem>().main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    void OnParticleSystemStopped()
    {
        Destroy(gameObject);
        OnAnimDone?.Invoke();
    }
}