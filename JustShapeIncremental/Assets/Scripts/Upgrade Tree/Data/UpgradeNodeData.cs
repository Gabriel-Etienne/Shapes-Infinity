using System;
using UnityEngine;

[Serializable]
public class UpgradeNodeData
{
    [SerializeField]
    private UpgradeData upgrade;

    [SerializeField]
    private Vector2 position;

    public UpgradeData Upgrade => upgrade;

    public Vector2 Position
    {
        get => position;
        set => position = value;
    }

    public UpgradeNodeData(
        UpgradeData upgrade,
        Vector2 position)
    {
        this.upgrade = upgrade;
        this.position = position;
    }
}