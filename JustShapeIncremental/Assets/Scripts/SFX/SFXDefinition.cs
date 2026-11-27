using System;
using UnityEngine;

[Serializable]
public class SFXDefinition
{
    [Header("Audio")]
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Pitch")]
    public bool randomizePitch = false;

    [Range(0.1f, 3f)]
    public float minPitch = 0.9f;

    [Range(0.1f, 3f)]
    public float maxPitch = 1.1f;

    [Header("Spatial Audio")]
    [Range(0f, 1f)]
    public float spatialBlend = 0f;

    [Min(0.1f)]
    public float minDistance = 1f;

    [Min(0.1f)]
    public float maxDistance = 20f;
}