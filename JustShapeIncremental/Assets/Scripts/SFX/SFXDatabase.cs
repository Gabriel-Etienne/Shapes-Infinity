using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

[CreateAssetMenu(
    fileName = "SFXDatabase",
    menuName = "Game/SFX/SFX Database"
)]
public class SFXDatabase : ScriptableObject
{
    [SerializeField]
    private SerializedDictionary<SFXType, SFXDefinition> sounds = new();

    public IReadOnlyDictionary<SFXType, SFXDefinition> Sounds => sounds;
}