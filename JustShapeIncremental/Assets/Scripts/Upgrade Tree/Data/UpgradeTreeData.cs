using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewUpgradeTree",
    menuName = "Upgrade System/Upgrade Tree"
)]
public class UpgradeTreeData : ScriptableObject
{
    [SerializeField]
    private List<UpgradeNodeData> nodes = new();

    public IReadOnlyList<UpgradeNodeData> Nodes => nodes;

    public void SetNodes(List<UpgradeNodeData> newNodes)
    {
        nodes = newNodes;
    }

    public bool ContainsUpgrade(UpgradeData upgrade)
    {
        return nodes.Exists(node => node.Upgrade == upgrade);
    }

    public void AddNode(UpgradeData upgrade, Vector2 position)
    {
        if (upgrade == null || ContainsUpgrade(upgrade))
            return;

        nodes.Add(new UpgradeNodeData(upgrade, position));
    }

    public void RemoveNode(UpgradeData upgrade)
    {
        nodes.RemoveAll(node => node.Upgrade == upgrade);
    }

    public void ClearNodes()
    {
        nodes.Clear();
    }
}