using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource MusicSource;
    public AudioSource SFXSource;

    private const string MusicKey = "MusicOn";
    private const string SFXKey = "SFXOn";

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

        if (MusicSource != null) MusicSource.mute = !musicOn;
        if (SFXSource != null) SFXSource.mute = !sfxOn;
    }

    public void SetMusicOn(bool on)
    {
        if (MusicSource != null) MusicSource.mute = !on;
        PlayerPrefs.SetInt(MusicKey, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSFXOn(bool on)
    {
        if (SFXSource != null) SFXSource.mute = !on;
        PlayerPrefs.SetInt(SFXKey, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsMusicOn() => PlayerPrefs.GetInt(MusicKey, 1) == 1;
    public bool IsSFXOn() => PlayerPrefs.GetInt(SFXKey, 1) == 1;
}