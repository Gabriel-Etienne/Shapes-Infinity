
using UnityEditor;
using UnityEngine;

using System.Collections.Generic;
using System.Linq;

public static class UpgradeTreeGenerator
{
    // --------------------------------------------------
    // CONFIGURATION
    // --------------------------------------------------

    private const float START_X = 100f;
    private const float START_Y = 100f;

    private const float HORIZONTAL_SPACING = 200f;
    private const float VERTICAL_SPACING = 200f;

    private const int ROOT_SPACING = 5;
    private const int MAX_SHIFT_ATTEMPTS = 1000;

    // Droite, bas, gauche, haut.
    private static readonly Vector2Int[] DIRECTIONS =
    {
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.up
    };

    // --------------------------------------------------
    // GENERATION
    // --------------------------------------------------

    public static int Generate(UpgradeTreeData tree, string folderPath)
    {
        if (tree == null)
        {
            Debug.LogError("No UpgradeTree selected!");
            return 0;
        }

        if (string.IsNullOrEmpty(folderPath) ||
            !AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("Invalid upgrade folder!");
            return 0;
        }

        string[] guids = AssetDatabase.FindAssets(
            "t:UpgradeData",
            new[] { folderPath }
        );

        List<UpgradeData> addedUpgrades = new();

        // --------------------------------------------------
        // GENERATION DES ID UNIQUES
        // --------------------------------------------------

        HashSet<string> usedIDs = new();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            UpgradeData upgrade =
                AssetDatabase.LoadAssetAtPath<UpgradeData>(path);

            if (upgrade == null)
                continue;

            string currentID = upgrade.UpgradeID;

            // ID vide ou déjà utilisé : on en génère un nouveau.
            if (string.IsNullOrWhiteSpace(currentID) ||
                usedIDs.Contains(currentID))
            {
                string newID;

                do
                {
                    newID = System.Guid.NewGuid().ToString("N");
                }
                while (usedIDs.Contains(newID));

                upgrade.SetUpgradeID(newID);

                EditorUtility.SetDirty(upgrade);

                Debug.Log(
                    $"New ID generated for {upgrade.UpgradeName}: {newID}"
                );
            }

            usedIDs.Add(upgrade.UpgradeID);
        }

        // --------------------------------------------------
        // AJOUT DES AMELIORATIONS
        // --------------------------------------------------

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            UpgradeData upgrade =
                AssetDatabase.LoadAssetAtPath<UpgradeData>(path);

            if (upgrade == null || tree.ContainsUpgrade(upgrade))
                continue;

            tree.AddNode(upgrade, Vector2.zero);
            addedUpgrades.Add(upgrade);
        }

        // --------------------------------------------------
        // ORGANISATION DE L'ARBRE
        // --------------------------------------------------

        if (!ArrangeNodes(tree))
        {
            // On retire les nouveaux nœuds si la génération échoue.
            foreach (UpgradeData upgrade in addedUpgrades)
                tree.RemoveNode(upgrade);

            // On sauvegarde tout de même les éventuels nouveaux IDs.
            AssetDatabase.SaveAssets();

            Debug.LogError(
                "Upgrade generation failed. Check the console."
            );

            return 0;
        }

        // --------------------------------------------------
        // SAUVEGARDE
        // --------------------------------------------------

        EditorUtility.SetDirty(tree);
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"Upgrade generation completed. " +
            $"{addedUpgrades.Count} new upgrades added."
        );

        return addedUpgrades.Count;
    }

    // --------------------------------------------------
    // LAYOUT CONTEXT
    // --------------------------------------------------

    private class LayoutContext
    {
        public Dictionary<UpgradeData, UpgradeNodeData> Nodes = new();

        public Dictionary<UpgradeData, List<UpgradeData>> Children = new();

        public Dictionary<UpgradeData, UpgradeData> Parents = new();

        public Dictionary<UpgradeData, Vector2Int> Positions = new();

        public Dictionary<UpgradeData, Vector2Int> GrowthDirections = new();

        public HashSet<Vector2Int> OccupiedPositions = new();

        // Cases traversées par les connexions.
        public HashSet<Vector2Int> OccupiedPathCells = new();

        // Chemin associé à chaque connexion parent-enfant.
        public Dictionary<UpgradeData, List<Vector2Int>> EdgePaths = new();
    }

    // --------------------------------------------------
    // INITIALISATION
    // --------------------------------------------------

    private static bool ArrangeNodes(UpgradeTreeData tree)
    {
        LayoutContext context = new LayoutContext();

        // Récupération des nœuds.
        foreach (UpgradeNodeData node in tree.Nodes)
        {
            if (node == null || node.Upgrade == null)
                continue;

            UpgradeData upgrade = node.Upgrade;

            if (context.Nodes.ContainsKey(upgrade))
            {
                Debug.LogError(
                    $"Duplicate upgrade: {upgrade.UpgradeName}"
                );

                return false;
            }

            context.Nodes.Add(upgrade, node);
            context.Children.Add(upgrade, new List<UpgradeData>());
        }

        // Construction de la hiérarchie.
        foreach (UpgradeData upgrade in context.Nodes.Keys)
        {
            List<UpgradeData> prerequisites =
                upgrade.Prerequisites
                    .Where(p => p != null)
                    .ToList();

            if (prerequisites.Count > 1)
            {
                Debug.LogError(
                    $"{upgrade.UpgradeName} has multiple prerequisites."
                );

                return false;
            }

            if (prerequisites.Count == 0)
                continue;

            UpgradeData parent = prerequisites[0];

            if (!context.Nodes.ContainsKey(parent))
            {
                Debug.LogError(
                    $"Missing prerequisite for {upgrade.UpgradeName}: " +
                    $"{parent.UpgradeName}"
                );

                return false;
            }

            context.Children[parent].Add(upgrade);
            context.Parents.Add(upgrade, parent);
        }

        // Ordre déterministe des enfants.
        foreach (List<UpgradeData> childList in context.Children.Values)
        {
            childList.Sort((a, b) =>
                string.Compare(
                    a.UpgradeName,
                    b.UpgradeName,
                    System.StringComparison.Ordinal
                )
            );
        }

        // Recherche des racines.
        List<UpgradeData> roots = context.Nodes.Keys
            .Where(upgrade => !context.Parents.ContainsKey(upgrade))
            .OrderBy(upgrade => upgrade.UpgradeName)
            .ToList();

        if (context.Nodes.Count > 0 && roots.Count == 0)
        {
            Debug.LogError("No root upgrade found.");
            return false;
        }

        if (!ValidateHierarchy(context, roots))
            return false;

        // --------------------------------------------------
        // VALIDATION DU NOMBRE D'ENFANTS
        // --------------------------------------------------

        foreach (UpgradeData upgrade in context.Nodes.Keys)
        {
            int childCount = context.Children[upgrade].Count;

            // La racine n'a pas de limite.
            // Toutes les autres améliorations sont limitées à 3 enfants.
            if (context.Parents.ContainsKey(upgrade) && childCount > 3)
            {
                Debug.LogError(
                    $"Invalid upgrade hierarchy: {upgrade.UpgradeName} " +
                    $"has {childCount} children. Maximum allowed: 3."
                );

                return false;
            }
        }

        // Placement des racines.
        for (int i = 0; i < roots.Count; i++)
        {
            UpgradeData root = roots[i];

            Vector2Int rootPosition = new Vector2Int(
                i * ROOT_SPACING,
                0
            );

            context.Positions[root] = rootPosition;
            context.GrowthDirections[root] = Vector2Int.zero;

            context.OccupiedPositions.Add(rootPosition);
        }

        // Génération des branches.
        foreach (UpgradeData root in roots)
        {
            if (!PlaceSubtree(root, context))
                return false;
        }

        if (context.Positions.Count != context.Nodes.Count)
        {
            Debug.LogError("Some upgrades could not be placed.");
            return false;
        }

        ApplyFinalPositions(context);

        return true;
    }

    // --------------------------------------------------
    // VALIDATION DES DEPENDANCES
    // --------------------------------------------------

    private static bool ValidateHierarchy(
        LayoutContext context,
        List<UpgradeData> roots)
    {
        Dictionary<UpgradeData, int> states = new();

        foreach (UpgradeData upgrade in context.Nodes.Keys)
            states[upgrade] = 0;

        int visitedCount = 0;

        foreach (UpgradeData root in roots)
        {
            if (!VisitNode(
                root,
                context,
                states,
                ref visitedCount))
            {
                return false;
            }
        }

        if (visitedCount != context.Nodes.Count)
        {
            Debug.LogError(
                "Some upgrades are unreachable. Circular dependency suspected."
            );

            return false;
        }

        return true;
    }

    private static bool VisitNode(
        UpgradeData upgrade,
        LayoutContext context,
        Dictionary<UpgradeData, int> states,
        ref int visitedCount)
    {
        if (states[upgrade] == 1)
        {
            Debug.LogError(
                $"Circular prerequisite: {upgrade.UpgradeName}"
            );

            return false;
        }

        if (states[upgrade] == 2)
            return true;

        states[upgrade] = 1;

        foreach (UpgradeData child in context.Children[upgrade])
        {
            if (!VisitNode(
                child,
                context,
                states,
                ref visitedCount))
            {
                return false;
            }
        }

        states[upgrade] = 2;
        visitedCount++;

        return true;
    }

    // --------------------------------------------------
    // PLACEMENT DES BRANCHES
    // --------------------------------------------------

    private static bool PlaceSubtree(
        UpgradeData parent,
        LayoutContext context)
    {
        List<UpgradeData> children = context.Children[parent];

        if (children.Count == 0)
            return true;

        bool success;

        if (context.GrowthDirections[parent] == Vector2Int.zero)
        {
            success = PlaceRootChildren(
                parent,
                children,
                context
            );
        }
        else
        {
            success = PlaceChildrenWithParentShift(
                parent,
                children,
                context
            );
        }

        if (!success)
            return false;

        foreach (UpgradeData child in children)
        {
            if (!PlaceSubtree(child, context))
                return false;
        }

        return true;
    }

    // --------------------------------------------------
    // PLACEMENT DES ENFANTS D'UNE RACINE
    // --------------------------------------------------

    private static bool PlaceRootChildren(
        UpgradeData root,
        List<UpgradeData> children,
        LayoutContext context)
    {
        Vector2Int rootPosition = context.Positions[root];

        List<Placement> planned = new();

        for (int i = 0; i < children.Count; i++)
        {
            Vector2Int direction = DIRECTIONS[i];

            Vector2Int candidate = rootPosition + direction;

            if (!CanPlaceNode(
                rootPosition,
                candidate,
                context,
                planned))
            {
                Debug.LogError(
                    $"Could not place {children[i].UpgradeName}."
                );

                return false;
            }

            planned.Add(new Placement(
                children[i],
                candidate,
                direction,
                BuildPath(rootPosition, candidate)
            ));
        }

        CommitPlacements(root, planned, context);

        return true;
    }

    // --------------------------------------------------
    // PLACEMENT DES AUTRES ENFANTS
    // --------------------------------------------------

    private static bool PlaceChildrenWithParentShift(
        UpgradeData parent,
        List<UpgradeData> children,
        LayoutContext context)
    {
        Vector2Int forward = context.GrowthDirections[parent];

        for (int attempt = 0; attempt < MAX_SHIFT_ATTEMPTS; attempt++)
        {
            Vector2Int parentPosition = context.Positions[parent];

            Vector2Int[] directions =
                GetAvailableDirections(forward);

            List<Placement> planned = new();

            bool allPlaced = true;

            foreach (UpgradeData child in children)
            {
                bool found = false;

                foreach (Vector2Int direction in directions)
                {
                    Vector2Int candidate =
                        parentPosition + direction;

                    if (!CanPlaceNode(
                        parentPosition,
                        candidate,
                        context,
                        planned))
                    {
                        continue;
                    }

                    planned.Add(new Placement(
                        child,
                        candidate,
                        direction,
                        BuildPath(parentPosition, candidate)
                    ));

                    found = true;
                    break;
                }

                if (!found)
                {
                    allPlaced = false;
                    break;
                }
            }

            if (allPlaced)
            {
                CommitPlacements(parent, planned, context);
                return true;
            }

            // On déplace le parent uniquement après avoir
            // échoué à placer tout son groupe d'enfants.
            if (!ShiftParentForward(parent, context))
            {
                Debug.LogError(
                    $"Cannot move {parent.UpgradeName} to make room."
                );

                return false;
            }
        }

        Debug.LogError(
            $"Maximum shift attempts reached for {parent.UpgradeName}."
        );

        return false;
    }

    // --------------------------------------------------
    // PLACEMENT DATA
    // --------------------------------------------------

    private class Placement
    {
        public UpgradeData Upgrade;
        public Vector2Int Position;
        public Vector2Int Direction;
        public List<Vector2Int> Path;

        public Placement(
            UpgradeData upgrade,
            Vector2Int position,
            Vector2Int direction,
            List<Vector2Int> path)
        {
            Upgrade = upgrade;
            Position = position;
            Direction = direction;
            Path = path;
        }
    }

    // --------------------------------------------------
    // VERIFICATION D'UNE POSITION ET DE SON CHEMIN
    // --------------------------------------------------

    private static bool CanPlaceNode(
        Vector2Int start,
        Vector2Int end,
        LayoutContext context,
        List<Placement> planned)
    {
        // Une case ne peut pas contenir deux améliorations.
        if (context.OccupiedPositions.Contains(end))
            return false;

        // Une amélioration ne peut pas se trouver sur un chemin.
        if (context.OccupiedPathCells.Contains(end))
            return false;

        // Vérification des positions prévues pour les autres enfants.
        if (planned.Any(p => p.Position == end))
            return false;

        // Vérification du chemin.
        List<Vector2Int> path = BuildPath(start, end);

        foreach (Vector2Int cell in path)
        {
            if (context.OccupiedPositions.Contains(cell))
                return false;

            if (context.OccupiedPathCells.Contains(cell))
                return false;

            if (planned.Any(p =>
                p.Position == cell ||
                p.Path.Contains(cell)))
            {
                return false;
            }
        }

        return true;
    }

    // --------------------------------------------------
    // CONSTRUCTION D'UN CHEMIN RECTILIGNE
    // --------------------------------------------------

    private static List<Vector2Int> BuildPath(
        Vector2Int start,
        Vector2Int end)
    {
        List<Vector2Int> path = new();

        Vector2Int difference = end - start;

        // Les connexions diagonales ne sont pas autorisées.
        if (difference.x != 0 && difference.y != 0)
            return path;

        Vector2Int direction = new Vector2Int(
            difference.x == 0 ? 0 : (int)Mathf.Sign(difference.x),
            difference.y == 0 ? 0 : (int)Mathf.Sign(difference.y)
        );

        Vector2Int current = start + direction;

        // Les extrémités sont exclues :
        // elles correspondent aux deux améliorations.
        while (current != end)
        {
            path.Add(current);
            current += direction;
        }

        return path;
    }

    // --------------------------------------------------
    // ENREGISTREMENT DES PLACEMENTS
    // --------------------------------------------------

    private static void CommitPlacements(
        UpgradeData parent,
        List<Placement> planned,
        LayoutContext context)
    {
        Vector2Int parentPosition = context.Positions[parent];

        foreach (Placement placement in planned)
        {
            RegisterPlacement(
                placement.Upgrade,
                placement.Position,
                placement.Direction,
                context
            );

            RegisterEdge(
                placement.Upgrade,
                placement.Path,
                context
            );
        }
    }

    private static void RegisterPlacement(
        UpgradeData upgrade,
        Vector2Int position,
        Vector2Int direction,
        LayoutContext context)
    {
        context.Positions[upgrade] = position;
        context.GrowthDirections[upgrade] = direction;

        context.OccupiedPositions.Add(position);
    }

    private static void RegisterEdge(
        UpgradeData child,
        List<Vector2Int> path,
        LayoutContext context)
    {
        context.EdgePaths[child] = new List<Vector2Int>(path);

        foreach (Vector2Int cell in path)
            context.OccupiedPathCells.Add(cell);
    }

    // --------------------------------------------------
    // DIRECTIONS AUTORISEES
    // --------------------------------------------------

    private static Vector2Int[] GetAvailableDirections(
        Vector2Int forward)
    {
        Vector2Int clockwise = new Vector2Int(
            -forward.y,
            forward.x
        );

        Vector2Int counterClockwise = new Vector2Int(
            forward.y,
            -forward.x
        );

        return new[]
        {
            forward,
            clockwise,
            counterClockwise
        };
    }

    // --------------------------------------------------
    // DEPLACEMENT DU PARENT
    // --------------------------------------------------

    private static bool ShiftParentForward(
        UpgradeData parent,
        LayoutContext context)
    {
        // Une racine ne peut pas être déplacée de cette manière.
        if (!context.Parents.TryGetValue(parent, out UpgradeData grandParent))
            return false;

        Vector2Int direction = context.GrowthDirections[parent];

        if (direction == Vector2Int.zero)
            return false;

        Vector2Int oldPosition = context.Positions[parent];

        // On retire temporairement l'ancienne connexion.
        if (context.EdgePaths.TryGetValue(parent, out List<Vector2Int> oldPath))
        {
            foreach (Vector2Int cell in oldPath)
                context.OccupiedPathCells.Remove(cell);

            context.EdgePaths.Remove(parent);
        }

        context.OccupiedPositions.Remove(oldPosition);

        Vector2Int newPosition = oldPosition + direction;

        // On vérifie que la nouvelle connexion vers le grand-parent
        // ne traverse aucune amélioration ni aucun autre chemin.
        bool canMove =
            !context.OccupiedPositions.Contains(newPosition) &&
            !context.OccupiedPathCells.Contains(newPosition);

        List<Vector2Int> newPath = BuildPath(
            context.Positions[grandParent],
            newPosition
        );

        if (canMove)
        {
            foreach (Vector2Int cell in newPath)
            {
                if (context.OccupiedPositions.Contains(cell) ||
                    context.OccupiedPathCells.Contains(cell))
                {
                    canMove = false;
                    break;
                }
            }
        }

        if (!canMove)
        {
            // Restauration de la position et de la connexion.
            context.Positions[parent] = oldPosition;
            context.OccupiedPositions.Add(oldPosition);

            RegisterEdge(parent, oldPath ?? new List<Vector2Int>(), context);

            return false;
        }

        // Déplacement définitif.
        context.Positions[parent] = newPosition;
        context.OccupiedPositions.Add(newPosition);

        RegisterEdge(parent, newPath, context);

        return true;
    }

    // --------------------------------------------------
    // POSITIONS FINALES
    // --------------------------------------------------

    private static void ApplyFinalPositions(LayoutContext context)
    {
        if (context.Positions.Count == 0)
            return;

        int minX = context.Positions.Values.Min(p => p.x);
        int minY = context.Positions.Values.Min(p => p.y);

        foreach (var entry in context.Positions)
        {
            Vector2Int gridPosition = entry.Value;

            Vector2 finalPosition = new Vector2(
                START_X + (gridPosition.x - minX) * HORIZONTAL_SPACING,
                START_Y + (gridPosition.y - minY) * VERTICAL_SPACING
            );

            context.Nodes[entry.Key].Position = finalPosition;
        }
    }
}
