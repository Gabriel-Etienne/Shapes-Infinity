using System;
using System.Collections;
using UnityEngine;

public class VFXInstance : MonoBehaviour
{
    private ParticleSystem[] particleSystems;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void Play(
        Vector3 position,
        Quaternion rotation,
        Vector3 scale,
        Action<VFXInstance> onFinished)
    {
        if (particleSystems == null || particleSystems.Length == 0)
        {
            Debug.LogError(
                $"VFX '{name}' ne contient aucun ParticleSystem.",
                this
            );

            onFinished?.Invoke(this);
            return;
        }

        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = scale;

        gameObject.SetActive(true);

        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            ps.Play(true);
        }

        StartCoroutine(WaitForParticles(onFinished));
    }

    private IEnumerator WaitForParticles(
        Action<VFXInstance> onFinished)
    {
        while (AreParticlesAlive())
        {
            yield return null;
        }

        onFinished?.Invoke(this);
    }

    private bool AreParticlesAlive()
    {
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null && ps.IsAlive(true))
                return true;
        }

        return false;
    }
}