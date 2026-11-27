using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Database")]
    [SerializeField] private VFXDatabase database;

    private Dictionary<VFXType, VFXPool> pools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializePools();
    }

    private void InitializePools()
    {
        pools.Clear();

        if (database == null)
        {
            Debug.LogError(
                "VFXDatabase n'est pas assignée.",
                this
            );

            return;
        }

        foreach (var entry in database.Effects)
        {
            VFXType type = entry.Key;
            VFXDefinition definition = entry.Value;

            if (definition == null || definition.prefab == null)
            {
                Debug.LogWarning(
                    $"Aucune prefab assignée pour le VFX {type}.",
                    this
                );

                continue;
            }

            VFXPool pool = new VFXPool(
                definition.prefab,
                transform,
                definition.initialPoolSize
            );

            pools.Add(type, pool);
        }
    }

    public void Play(VFXType type, Vector3 position)
    {
        Play(
            type,
            position,
            Quaternion.identity,
            Vector3.one
        );
    }

    public void Play(
        VFXType type,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale)
    {
        if (!pools.TryGetValue(type, out VFXPool pool))
        {
            Debug.LogWarning(
                $"Aucun VFX enregistré pour le type {type}.",
                this
            );

            return;
        }

        pool.Play(position, rotation, scale);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}