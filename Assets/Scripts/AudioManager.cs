using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{

    [SerializeField] private AudioClip _mainTheme;
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioSource _themeSource;
    [SerializeField] private AudioSource _soundsSource;

    [SerializeField] private AudioClip _btnClick;
    [SerializeField] private AudioClip _lampSound;
    [SerializeField] private AudioClip _lightingSound;
    [SerializeField] private AudioClip _magnetSound;
    [SerializeField] private AudioClip _wrongWordSound;
    [SerializeField] private AudioClip _wordFoundSound;
    [SerializeField] private AudioClip _coinsSound;
    [SerializeField] private AudioClip _windOpenSound;
    [SerializeField] private AudioClip _windCloseSound;
    [SerializeField] private AudioClip _stageCompleted;


    private Dictionary<Sound, AudioClip> _audioClips;
    [SerializeField] private List<AudioClip> _letters;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;

            // _audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(this);

            _audioClips = new Dictionary<Sound, AudioClip>(){
        {Sound.Click, _btnClick},
        {Sound.Lamp, _lampSound},
        {Sound.Light, _lightingSound},
        {Sound.Magnet, _magnetSound},
        {Sound.WrongWord, _wrongWordSound},
        {Sound.WordFound, _wordFoundSound},
        {Sound.Coins, _coinsSound},
        {Sound.WindOpen, _windOpenSound},
        {Sound.WindClose, _windCloseSound},
        {Sound.StageCompleted, _stageCompleted}
    };
            PlayTheme();
        }

        Session.OnMusicChange += HandleMusicChange;


    }


    private void HandleMusicChange(bool value)
    {
        if (value) PlayTheme();
        else
            _themeSource.mute = !value;

    }

    void OnDestroy()
    {
        Session.OnMusicChange -= HandleMusicChange;
    }

    public void PlayTheme()
    {

        if (!Session.IsMusicOn) return;
        _themeSource.clip = _mainTheme;
        _themeSource.loop = true;
        _themeSource.mute = false;
        if (!_themeSource.isPlaying) _themeSource.Play();
    }

    public void PlayLetter(int pitch)
    {
        if (!Session.IsSoundOn) return;

        if (_letters.Count <= pitch)
        {
            Debug.Log($"Sound out of bounds: {pitch}");
            pitch = _letters.Count - 1;
        }

        var clip = _letters[pitch];
        _soundsSource.PlayOneShot(clip, 10);
    }

    public void PlaySound(Sound sound)
    {
        if (!Session.IsSoundOn) return;
        _soundsSource.PlayOneShot(_audioClips[sound], 10);
    }
}

public enum Sound
{
    Click,
    Lamp,
    Light,
    Magnet,
    WrongWord,
    WordFound,
    Coins,
    WindOpen,
    WindClose,
    StageCompleted
}