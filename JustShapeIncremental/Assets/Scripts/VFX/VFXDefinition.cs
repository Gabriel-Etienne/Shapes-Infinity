using System;
using UnityEngine;

[Serializable]
public class VFXDefinition
{
    [Header("Prefab")]
    public GameObject prefab;

    [Header("Pooling")]
    [Min(0)]
    public int initialPoolSize = 5;
}