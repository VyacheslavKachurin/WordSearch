using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance { get; private set; }
    private AudioSource _audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;

            _audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(this);
        }
    }

    public void PlayLetter(int pitch)
    {
        Debug.Log($"Pitch Sound: {pitch}");
    }
}