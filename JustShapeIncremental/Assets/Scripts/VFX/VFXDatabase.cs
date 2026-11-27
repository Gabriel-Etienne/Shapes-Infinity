using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

[CreateAssetMenu(
    fileName = "VFXDatabase",
    menuName = "Game/VFX/VFX Database"
)]
public class VFXDatabase : ScriptableObject
{
    [SerializeField]
    private SerializedDictionary<VFXType, VFXDefinition> effects = new();

    public IReadOnlyDictionary<VFXType, VFXDefinition> Effects => effects;
}