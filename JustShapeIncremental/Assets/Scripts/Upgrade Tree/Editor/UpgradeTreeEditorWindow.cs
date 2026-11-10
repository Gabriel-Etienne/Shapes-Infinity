
using UnityEditor;
using UnityEngine;

public class UpgradeTreeEditorWindow : EditorWindow
{
    [SerializeField] private UpgradeTreeData tree;
    [SerializeField] private DefaultAsset upgradeFolder;

    private UpgradeNodeData selectedNode;

    private Vector2 panOffset;
    private Vector2 detailsScroll;

    private float zoom = 1f;

    private const float NODE_SIZE = 150f;
    private const float MIN_ZOOM = 0.25f;
    private const float MAX_ZOOM = 1.5f;
    private const float PANEL_WIDTH = 280f;

    private bool isDirty;

    [MenuItem("Tools/Upgrade System/Tree Editor")]
    public static void OpenWindow()
    {
        UpgradeTreeEditorWindow window =
            GetWindow<UpgradeTreeEditorWindow>();

        window.titleContent = new GUIContent("Upgrade Tree");
        window.minSize = new Vector2(700, 400);
        window.Show();
    }

    private void OnGUI()
    {
        DrawToolbar();

        float canvasWidth = Mathf.Max(
            100f,
            position.width - PANEL_WIDTH
        );

        Rect canvasRect = new Rect(
            0,
            40,
            canvasWidth,
            position.height - 40
        );

        Rect detailsRect = new Rect(
            canvasWidth,
            40,
            PANEL_WIDTH,
            position.height - 40
        );

        DrawCanvas(canvasRect);
        DrawDetailsPanel(detailsRect);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUILayout.Label("Tree:", GUILayout.Width(35));

        tree = (UpgradeTreeData)EditorGUILayout.ObjectField(
            tree,
            typeof(UpgradeTreeData),
            false,
            GUILayout.Width(180)
        );

        GUILayout.Space(10);

        GUILayout.Label("Folder:", GUILayout.Width(45));

        upgradeFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            upgradeFolder,
            typeof(DefaultAsset),
            false,
            GUILayout.Width(160)
        );

        GUI.enabled =
            tree != null &&
            upgradeFolder != null &&
            AssetDatabase.IsValidFolder(
                AssetDatabase.GetAssetPath(upgradeFolder)
            );

        if (GUILayout.Button(
            "Generate",
            EditorStyles.toolbarButton,
            GUILayout.Width(80)))
        {
            GenerateTree();
        }

        GUI.enabled = tree != null;

        if (GUILayout.Button(
            "Save",
            EditorStyles.toolbarButton,
            GUILayout.Width(60)))
        {
            SaveTree();
        }

        GUI.enabled = true;

        GUILayout.FlexibleSpace();

        GUILayout.Label(
            $"Zoom: {zoom:P0}",
            EditorStyles.miniLabel
        );

        EditorGUILayout.EndHorizontal();
    }

    private void GenerateTree()
    {
        if (tree == null || upgradeFolder == null)
            return;

        string folderPath =
            AssetDatabase.GetAssetPath(upgradeFolder);

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("Invalid upgrade folder!");
            return;
        }

        Undo.RecordObject(tree, "Generate Upgrade Tree");

        UpgradeTreeGenerator.Generate(
            tree,
            folderPath
        );

        EditorUtility.SetDirty(tree);
        AssetDatabase.SaveAssets();

        isDirty = false;
        selectedNode = null;

        Repaint();
    }

    private void DrawCanvas(Rect canvasRect)
    {
        GUI.BeginGroup(canvasRect);

        Rect localRect = new Rect(
            0,
            0,
            canvasRect.width,
            canvasRect.height
        );

        EditorGUI.DrawRect(
            localRect,
            new Color(0.13f, 0.13f, 0.13f)
        );

        HandleCanvasInput();

        DrawGrid(localRect);
        DrawConnections();
        DrawNodes();

        int nodeCount = tree != null
            ? tree.Nodes.Count
            : 0;

        GUI.Label(
            new Rect(10, 10, 250, 20),
            $"Nodes detected: {nodeCount}",
            EditorStyles.whiteLabel
        );

        GUI.EndGroup();
    }

    private void DrawGrid(Rect canvasRect)
    {
        Handles.BeginGUI();

        Color previousColor = Handles.color;

        Handles.color = new Color(1f, 1f, 1f, 0.12f);

        float spacing = 20f * zoom;

        float startX = Mathf.Repeat(panOffset.x, spacing);
        float startY = Mathf.Repeat(panOffset.y, spacing);

        for (float x = startX; x < canvasRect.width; x += spacing)
        {
            Handles.DrawLine(
                new Vector3(x, 0),
                new Vector3(x, canvasRect.height)
            );
        }

        for (float y = startY; y < canvasRect.height; y += spacing)
        {
            Handles.DrawLine(
                new Vector3(0, y),
                new Vector3(canvasRect.width, y)
            );
        }

        Handles.color = previousColor;

        Handles.EndGUI();
    }

    private void DrawConnections()
    {
        if (tree == null)
            return;

        Handles.BeginGUI();

        Color previousColor = Handles.color;

        Handles.color = new Color(0.3f, 0.75f, 1f);

        foreach (UpgradeNodeData node in tree.Nodes)
        {
            if (node == null || node.Upgrade == null)
                continue;

            foreach (UpgradeData prerequisite in node.Upgrade.Prerequisites)
            {
                if (prerequisite == null)
                    continue;

                UpgradeNodeData parentNode = FindNode(prerequisite);

                if (parentNode == null)
                    continue;

                Rect parentRect = GetNodeRect(parentNode);
                Rect childRect = GetNodeRect(node);

                Vector2 parentCenter = parentRect.center;
                Vector2 childCenter = childRect.center;

                Vector2 difference = childCenter - parentCenter;

                Vector2 start;
                Vector2 end;

                if (Mathf.Abs(difference.x) >= Mathf.Abs(difference.y))
                {
                    if (difference.x >= 0)
                    {
                        start = new Vector2(
                            parentRect.xMax,
                            parentCenter.y
                        );

                        end = new Vector2(
                            childRect.xMin,
                            childCenter.y
                        );
                    }
                    else
                    {
                        start = new Vector2(
                            parentRect.xMin,
                            parentCenter.y
                        );

                        end = new Vector2(
                            childRect.xMax,
                            childCenter.y
                        );
                    }
                }
                else
                {
                    if (difference.y >= 0)
                    {
                        start = new Vector2(
                            parentCenter.x,
                            parentRect.yMax
                        );

                        end = new Vector2(
                            childCenter.x,
                            childRect.yMin
                        );
                    }
                    else
                    {
                        start = new Vector2(
                            parentCenter.x,
                            parentRect.yMin
                        );

                        end = new Vector2(
                            childCenter.x,
                            childRect.yMax
                        );
                    }
                }

                Handles.DrawAAPolyLine(
                    Mathf.Max(1f, 5f * zoom),
                    start,
                    end
                );
            }
        }

        Handles.color = previousColor;

        Handles.EndGUI();
    }

    private Rect GetNodeRect(UpgradeNodeData node)
    {
        return new Rect(
            panOffset.x + node.Position.x * zoom,
            panOffset.y + node.Position.y * zoom,
            NODE_SIZE * zoom,
            NODE_SIZE * zoom
        );
    }

    private void DrawNodes()
    {
        if (tree == null)
            return;

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            normal =
            {
                textColor = Color.white
            }
        };

        GUIStyle costStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal =
            {
                textColor = new Color(1f, 0.85f, 0.35f)
            }
        };

        GUIStyle levelStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = Color.white
            }
        };

        for (int i = 0; i < tree.Nodes.Count; i++)
        {
            UpgradeNodeData node = tree.Nodes[i];

            if (node == null || node.Upgrade == null)
                continue;

            Rect nodeRect = GetNodeRect(node);

            bool isRoot =
                node.Upgrade.Prerequisites.Count == 0;

            Color nodeColor = isRoot
                ? new Color(0.85f, 0.15f, 0.15f)
                : new Color(0.20f, 0.25f, 0.32f);

            EditorGUI.DrawRect(nodeRect, nodeColor);

            GUI.Box(
                nodeRect,
                GUIContent.none,
                EditorStyles.helpBox
            );

            Rect titleRect = new Rect(
                nodeRect.x + 5f * zoom,
                nodeRect.y + 4f * zoom,
                nodeRect.width - 10f * zoom,
                25f * zoom
            );

            GUI.Label(
                titleRect,
                node.Upgrade.UpgradeName,
                titleStyle
            );

            Rect costRect = new Rect(
                nodeRect.x + 5f * zoom,
                nodeRect.y + 30f * zoom,
                nodeRect.width - 10f * zoom,
                20f * zoom
            );

            string costText = node.Upgrade.MaxLevel > 0
                ? $"Cost : {node.Upgrade.GetCostForLevel(0)}"
                : "Cost : --";

            GUI.Label(
                costRect,
                costText,
                costStyle
            );

            Rect levelRect = new Rect(
                nodeRect.x + 5f * zoom,
                nodeRect.y + 50f * zoom,
                nodeRect.width - 10f * zoom,
                18f * zoom
            );

            GUI.Label(
                levelRect,
                $"0/{node.Upgrade.MaxLevel}",
                levelStyle
            );

            Sprite icon = node.Upgrade.Icon;

            const float ICON_SIZE = 48f;

            Rect iconRect = new Rect(
                nodeRect.center.x - (ICON_SIZE * 0.5f * zoom),
                nodeRect.y + 75f * zoom,
                ICON_SIZE * zoom,
                ICON_SIZE * zoom
            );

            if (icon != null)
            {
                Texture2D texture = icon.texture;
                Rect textureRect = icon.textureRect;

                Rect uvRect = new Rect(
                    textureRect.x / texture.width,
                    textureRect.y / texture.height,
                    textureRect.width / texture.width,
                    textureRect.height / texture.height
                );

                GUI.DrawTextureWithTexCoords(
                    iconRect,
                    texture,
                    uvRect,
                    true
                );
            }
            else
            {
                GUI.Box(
                    iconRect,
                    "No Icon",
                    EditorStyles.centeredGreyMiniLabel
                );
            }

            if (selectedNode == node)
            {
                Handles.BeginGUI();

                Color previousColor = Handles.color;

                Handles.color = Color.yellow;

                Handles.DrawSolidRectangleWithOutline(
                    nodeRect,
                    Color.clear,
                    Color.yellow
                );

                Handles.color = previousColor;

                Handles.EndGUI();
            }
        }
    }

    private void DrawDetailsPanel(Rect panelRect)
    {
        GUI.BeginGroup(panelRect);

        Rect localRect = new Rect(
            0,
            0,
            panelRect.width,
            panelRect.height
        );

        EditorGUI.DrawRect(
            localRect,
            new Color(0.18f, 0.18f, 0.18f)
        );

        GUILayout.BeginArea(
            new Rect(10, 10, panelRect.width - 20, panelRect.height - 20)
        );

        GUILayout.Label(
            "Upgrade Details",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space(10);

        if (selectedNode == null || selectedNode.Upgrade == null)
        {
            EditorGUILayout.HelpBox(
                "Select an upgrade on the canvas to see its details.",
                MessageType.Info
            );

            GUILayout.EndArea();
            GUI.EndGroup();
            return;
        }

        UpgradeData upgrade = selectedNode.Upgrade;

        detailsScroll = EditorGUILayout.BeginScrollView(detailsScroll);

        SerializedObject serializedUpgrade =
            new SerializedObject(upgrade);

        serializedUpgrade.Update();

        EditorGUILayout.LabelField(
            "Identification",
            EditorStyles.boldLabel
        );

        SerializedProperty idProperty =
            serializedUpgrade.FindProperty("upgradeID");

        EditorGUI.BeginDisabledGroup(true);

        if (idProperty != null)
        {
            EditorGUILayout.PropertyField(
                idProperty,
                new GUIContent("Upgrade ID")
            );
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Informations",
            EditorStyles.boldLabel
        );

        SerializedProperty nameProperty =
            serializedUpgrade.FindProperty("upgradeName");

        SerializedProperty descriptionProperty =
            serializedUpgrade.FindProperty("description");

        SerializedProperty iconProperty =
            serializedUpgrade.FindProperty("icon");

        if (nameProperty != null)
        {
            EditorGUILayout.PropertyField(
                nameProperty,
                new GUIContent("Name")
            );
        }

        if (descriptionProperty != null)
        {
            EditorGUILayout.PropertyField(
                descriptionProperty,
                new GUIContent("Description")
            );
        }

        if (iconProperty != null)
        {
            EditorGUILayout.PropertyField(
                iconProperty,
                new GUIContent("Icon")
            );
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Levels",
            EditorStyles.boldLabel
        );

        SerializedProperty levelsProperty =
            serializedUpgrade.FindProperty("levels");

        if (levelsProperty != null)
        {
            EditorGUILayout.PropertyField(
                levelsProperty,
                new GUIContent("Upgrade Levels"),
                true
            );

            EditorGUILayout.HelpBox(
                $"Maximum Level: {levelsProperty.arraySize}",
                MessageType.None
            );

            if (levelsProperty.arraySize > 0)
            {
                EditorGUILayout.HelpBox(
                    $"First purchase cost: {upgrade.GetCostForLevel(0)}",
                    MessageType.None
                );
            }
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Prerequisites",
            EditorStyles.boldLabel
        );

        SerializedProperty prerequisiteRequirementProperty =
            serializedUpgrade.FindProperty("prerequisiteRequirement");

        if (prerequisiteRequirementProperty != null)
        {
            EditorGUILayout.PropertyField(
                prerequisiteRequirementProperty,
                new GUIContent("Required Parent State")
            );
        }

        SerializedProperty prerequisitesProperty =
            serializedUpgrade.FindProperty("prerequisites");

        EditorGUI.BeginDisabledGroup(true);

        if (prerequisitesProperty != null)
        {
            EditorGUILayout.PropertyField(
                prerequisitesProperty,
                new GUIContent("Prerequisites"),
                true
            );
        }

        EditorGUI.EndDisabledGroup();

        if (serializedUpgrade.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(upgrade);
            EditorUtility.SetDirty(tree);

            isDirty = true;

            Repaint();
        }

        EditorGUILayout.Space(15);

        if (GUILayout.Button("Save Changes"))
        {
            EditorUtility.SetDirty(upgrade);
            SaveTree();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.EndArea();

        GUI.EndGroup();
    }

    private UpgradeNodeData FindNode(UpgradeData upgrade)
    {
        if (tree == null)
            return null;

        foreach (UpgradeNodeData node in tree.Nodes)
        {
            if (node != null && node.Upgrade == upgrade)
                return node;
        }

        return null;
    }

    private void HandleCanvasInput()
    {
        Event currentEvent = Event.current;

        if (currentEvent.type == EventType.ScrollWheel)
        {
            Vector2 mousePosition = currentEvent.mousePosition;

            float previousZoom = zoom;

            zoom = Mathf.Clamp(
                zoom - currentEvent.delta.y * 0.02f,
                MIN_ZOOM,
                MAX_ZOOM
            );

            Vector2 graphPosition =
                (mousePosition - panOffset) / previousZoom;

            panOffset =
                mousePosition - graphPosition * zoom;

            currentEvent.Use();

            Repaint();
        }

        if (
            currentEvent.type == EventType.MouseDrag &&
            currentEvent.button == 2)
        {
            panOffset += currentEvent.delta;

            currentEvent.Use();

            Repaint();
        }

        if (
            currentEvent.type == EventType.MouseDown &&
            currentEvent.button == 0)
        {
            selectedNode = null;

            if (tree != null)
            {
                for (int i = tree.Nodes.Count - 1; i >= 0; i--)
                {
                    UpgradeNodeData node = tree.Nodes[i];

                    if (node == null || node.Upgrade == null)
                        continue;

                    Rect nodeRect = GetNodeRect(node);

                    if (nodeRect.Contains(currentEvent.mousePosition))
                    {
                        selectedNode = node;
                        break;
                    }
                }
            }

            currentEvent.Use();

            Repaint();
        }
    }

    private void SaveTree()
    {
        if (tree == null)
            return;

        EditorUtility.SetDirty(tree);

        if (selectedNode != null && selectedNode.Upgrade != null)
        {
            EditorUtility.SetDirty(selectedNode.Upgrade);
        }

        AssetDatabase.SaveAssets();

        isDirty = false;

        Debug.Log("Upgrade Tree successfully saved!");
    }

    private void OnDisable()
    {
        if (isDirty && tree != null)
        {
            SaveTree();
        }
    }
}
