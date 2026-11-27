using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Database")]
    [SerializeField] private SFXDatabase database;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Audio Sources")]
    [SerializeField, Min(1)]
    private int initialAudioSourceCount = 10;

    private readonly List<AudioSource> audioSources = new();

    private int nextSourceIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeAudioSources();

        if (database == null)
            Debug.LogError("SFXDatabase n'est pas assignée.", this);

        if (sfxGroup == null)
            Debug.LogError("SFX AudioMixerGroup n'est pas assigné.", this);
    }

    private void InitializeAudioSources()
    {
        audioSources.Clear();

        for (int i = 0; i < initialAudioSourceCount; i++)
        {
            CreateAudioSource();
        }
    }

    private AudioSource CreateAudioSource()
    {
        GameObject sourceObject = new GameObject(
            $"SFX AudioSource {audioSources.Count}"
        );

        sourceObject.transform.SetParent(transform);

        AudioSource source = sourceObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = false;

        // Envoie le son vers le groupe SFX du Mixer.
        source.outputAudioMixerGroup = sfxGroup;

        audioSources.Add(source);

        return source;
    }

    private AudioSource GetAvailableAudioSource()
    {
        for (int i = 0; i < audioSources.Count; i++)
        {
            int index = (nextSourceIndex + i) % audioSources.Count;

            if (!audioSources[index].isPlaying)
            {
                nextSourceIndex = (index + 1) % audioSources.Count;
                return audioSources[index];
            }
        }

        return CreateAudioSource();
    }

    public void Play(SFXType type)
    {
        PlayInternal(type, null);
    }

    public void PlayAtPosition(SFXType type, Vector3 position)
    {
        PlayInternal(type, position);
    }

    private void PlayInternal(SFXType type, Vector3? position)
    {
        if (database == null)
            return;

        if (!database.Sounds.TryGetValue(
            type,
            out SFXDefinition definition))
        {
            Debug.LogWarning(
                $"Aucun son enregistré pour le type {type}.",
                this
            );

            return;
        }

        if (definition == null || definition.clip == null)
        {
            Debug.LogWarning(
                $"Aucun AudioClip assigné pour le son {type}.",
                this
            );

            return;
        }

        AudioSource source = GetAvailableAudioSource();

        source.Stop();

        source.transform.position = position ?? transform.position;

        source.volume = definition.volume;

        source.pitch = definition.randomizePitch
            ? Random.Range(
                Mathf.Min(definition.minPitch, definition.maxPitch),
                Mathf.Max(definition.minPitch, definition.maxPitch)
            )
            : 1f;

        source.spatialBlend = definition.spatialBlend;
        source.minDistance = definition.minDistance;
        source.maxDistance = Mathf.Max(
            definition.minDistance,
            definition.maxDistance
        );

        source.PlayOneShot(definition.clip);
    }

    public void StopAll()
    {
        foreach (AudioSource source in audioSources)
        {
            if (source != null)
                source.Stop();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}