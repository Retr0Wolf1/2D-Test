// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.Serialization;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const string MusicKey = "MusicOn";
    private const string SFXKey = "SFXOn";

    [FormerlySerializedAs("MusicSource")]
    [SerializeField] private AudioSource _musicSource;

    [FormerlySerializedAs("SFXSource")]
    [SerializeField] private AudioSource _sfxSource;

    public AudioSource MusicSource => _musicSource;
    public AudioSource SFXSource => _sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    public void LoadSettings()
    {
        bool musicOn = PlayerPrefs.GetInt(MusicKey, 1) == 1;
        bool sfxOn = PlayerPrefs.GetInt(SFXKey, 1) == 1;

        if (_musicSource != null)
        {
            _musicSource.mute = !musicOn;
        }

        if (_sfxSource != null)
        {
            _sfxSource.mute = !sfxOn;
        }
    }

    public void SetMusicOn(bool on)
    {
        if (_musicSource != null)
        {
            _musicSource.mute = !on;
        }

        PlayerPrefs.SetInt(MusicKey, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSFXOn(bool on)
    {
        if (_sfxSource != null)
        {
            _sfxSource.mute = !on;
        }

        PlayerPrefs.SetInt(SFXKey, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsMusicOn()
    {
        return PlayerPrefs.GetInt(MusicKey, 1) == 1;
    }

    public bool IsSFXOn()
    {
        return PlayerPrefs.GetInt(SFXKey, 1) == 1;
    }
}