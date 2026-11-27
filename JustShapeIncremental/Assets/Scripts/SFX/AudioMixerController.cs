using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerController : MonoBehaviour
{
    public static AudioMixerController Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    private const string MASTER_VOLUME = "MasterVolume";
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (audioMixer == null)
        {
            Debug.LogError("AudioMixer n'est pas assigné.", this);
        }
    }

    public void SetMasterVolume(float volume)
    {
        SetVolume(MASTER_VOLUME, volume);
    }

    public void SetMusicVolume(float volume)
    {
        SetVolume(MUSIC_VOLUME, volume);
    }

    public void SetSFXVolume(float volume)
    {
        SetVolume(SFX_VOLUME, volume);
    }

    private void SetVolume(string parameter, float volume)
    {
        if (audioMixer == null)
            return;

        // Convertit une valeur 0-1 en décibels.
        float volumeDB = Mathf.Log10(
            Mathf.Clamp(volume, 0.0001f, 1f)
        ) * 20f;

        audioMixer.SetFloat(parameter, volumeDB);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}