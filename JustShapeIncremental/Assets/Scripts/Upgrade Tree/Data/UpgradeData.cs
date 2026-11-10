
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewUpgrade",
    menuName = "Upgrade System/Upgrade"
)]
public class UpgradeData : ScriptableObject
{
    [Serializable]
    public struct UpgradeLevelData
    {
        [Header("Level Information")]

        [Min(0)]
        public int cost;

        [TextArea(2, 4)]
        public string description;

        [Header("Effect")]

        public float effectValue;

    }

    [Header("Identification")]

    [SerializeField]
    private string upgradeID;

    [Header("Informations")]

    [SerializeField]
    private string upgradeName;

    [SerializeField]
    private Sprite icon;

    [Header("Levels")]

    [SerializeField]
    private List<UpgradeLevelData> levels = new();

    [Header("Prerequisites")]

    [SerializeField]
    private UpgradePrerequisiteMode prerequisiteRequirement =
        UpgradePrerequisiteMode.Unlocked;

    [SerializeField]
    private List<UpgradeData> prerequisites = new();

    public string UpgradeID => upgradeID;

    public string UpgradeName => upgradeName;

    public Sprite Icon => icon;

    public IReadOnlyList<UpgradeLevelData> Levels => levels;

    public int MaxLevel => levels.Count;

    public int Cost => GetCostForLevel(0);

    public UpgradePrerequisiteMode PrerequisiteRequirement =>
        prerequisiteRequirement;

    public IReadOnlyList<UpgradeData> Prerequisites => prerequisites;

    public UpgradeLevelData GetLevelData(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
            return default;

        return levels[levelIndex];
    }

    public int GetCostForLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
            return 0;

        return levels[levelIndex].cost;
    }

    public bool HasLevel(int levelIndex)
    {
        return levelIndex >= 0 && levelIndex < levels.Count;
    }

    public void SetUpgradeID(string newID)
    {
        upgradeID = newID;
    }

    private void OnValidate()
    {
        // Ensure the level list exists.
        if (levels == null)
            levels = new List<UpgradeLevelData>();

        // Prevent negative costs.
        for (int i = 0; i < levels.Count; i++)
        {
            UpgradeLevelData level = levels[i];

            level.cost = Mathf.Max(0, level.cost);

            levels[i] = level;
        }

        // Generate an ID only if one doesn't already exist.
        if (string.IsNullOrWhiteSpace(upgradeID))
            GenerateID();
    }

    [ContextMenu("Regenerate ID")]
    private void GenerateID()
    {
        upgradeID = Guid.NewGuid().ToString("N");
    }
}
