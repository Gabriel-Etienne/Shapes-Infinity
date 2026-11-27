using System.Collections.Generic;
using UnityEngine;

public class VFXPool
{
    private readonly Queue<VFXInstance> availableInstances = new();

    private readonly GameObject prefab;
    private readonly Transform parent;

    public VFXPool(
        GameObject prefab,
        Transform parent,
        int initialSize)
    {
        this.prefab = prefab;
        this.parent = parent;

        Prewarm(initialSize);
    }

    private void Prewarm(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            VFXInstance instance = CreateInstance();

            if (instance != null)
                availableInstances.Enqueue(instance);
        }
    }

    private VFXInstance CreateInstance()
    {
        GameObject instanceObject = Object.Instantiate(
            prefab,
            parent
        );

        VFXInstance instance =
            instanceObject.GetComponent<VFXInstance>();

        if (instance == null)
        {
            Debug.LogError(
                $"La prefab '{prefab.name}' ne possède pas VFXInstance."
            );

            Object.Destroy(instanceObject);
            return null;
        }

        instanceObject.name = prefab.name;
        instanceObject.SetActive(false);

        return instance;
    }

    public void Play(
        Vector3 position,
        Quaternion rotation,
        Vector3 scale)
    {
        VFXInstance instance;

        if (availableInstances.Count > 0)
        {
            instance = availableInstances.Dequeue();
        }
        else
        {
            instance = CreateInstance();
        }

        if (instance == null)
            return;

        instance.Play(
            position,
            rotation,
            scale,
            ReturnToPool
        );
    }

    private void ReturnToPool(VFXInstance instance)
    {
        instance.gameObject.SetActive(false);
        availableInstances.Enqueue(instance);
    }
}