using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace DiamondMind.Prototypes.Tools
{
    public class ProjectExporter : EditorWindow
    {
        // Root nodes for the Assets and ProjectSettings trees
        private AssetNode assetsRootNode;
        private AssetNode projectSettingsRootNode;

        // Scroll positions for the tree views
        private Vector2 assetsScrollPos;
        private Vector2 settingsScrollPos;

        private bool includePackages = false;
        private bool includeDependencies = true; 

        /// <summary>
        /// Represents a node in the tree (either a folder or a file).
        /// Each node has a 'selected' flag and a reference to its parent.
        /// </summary>
        private class AssetNode
        {
            public string name;               // Display name
            public string path;               // Relative path within the project
            public bool isFolder;             // True if the node represents a folder
            public bool selected;             // Selection state for export
            public bool foldout;              // Controls folder expansion
            public AssetNode parent;          // Reference to the parent node
            public List<AssetNode> children = new List<AssetNode>(); // Child nodes
        }

        /// <summary>
        /// Adds a menu item in the Unity Editor.
        /// Opens the Project Exporter window and initializes the asset trees.
        /// </summary>
        [MenuItem("Export/Export Custom Package")]
        public static void ShowWindow()
        {
            ProjectExporter window = GetWindow<ProjectExporter>("Export Package");
            window.minSize = new Vector2(500, 500);
            window.BuildTrees(); 
        }

        /// <summary>
        /// Constructs the tree structures for both Assets and ProjectSettings.
        /// </summary>
        private void BuildTrees()
        {
            // Build the Assets tree, the root has no parent
            assetsRootNode = BuildAssetNode("Assets", null);
            // Build the ProjectSettings tree, the root has no parent
            projectSettingsRootNode = BuildProjectSettingsNode();
        }

        /// <summary>
        /// Recursively builds a tree structure starting from a given relative path.
        /// Parent references are set during tree construction.
        /// </summary>
        /// <param name="relativePath">Relative folder path (e.g., "Assets").</param>
        /// <param name="parent">The parent node (null for root nodes).</param>
        /// <returns>An AssetNode representing the folder and its contents.</returns>
        private AssetNode BuildAssetNode(string relativePath, AssetNode parent)
        {
            // Determine the absolute path
            string absolutePath = (relativePath == "Assets")
                ? Application.dataPath
                : Path.Combine(Application.dataPath.Replace("/Assets", ""), relativePath);

            AssetNode node = new AssetNode
            {
                name = Path.GetFileName(relativePath),
                path = relativePath,
                isFolder = Directory.Exists(absolutePath),
                parent = parent
            };

            if (node.isFolder)
            {
                // Process subdirectories
                foreach (string dir in Directory.GetDirectories(absolutePath))
                {
                    string dirRelativePath = GetRelativePath(dir);
                    AssetNode child = BuildAssetNode(dirRelativePath, node);
                    node.children.Add(child);
                }
                // Process files (ignoring .meta files)
                foreach (string file in Directory.GetFiles(absolutePath))
                {
                    if (!file.EndsWith(".meta"))
                    {
                        string fileRelativePath = GetRelativePath(file);
                        AssetNode fileNode = new AssetNode
                        {
                            name = Path.GetFileName(file),
                            path = fileRelativePath,
                            isFolder = false,
                            parent = node
                        };
                        node.children.Add(fileNode);
                    }
                }
            }
            return node;
        }

        /// <summary>
        /// Converts an absolute path to a relative path using forward slashes.
        /// Removes the project root portion of the path.
        /// </summary>
        /// <param name="absolutePath">The absolute path to convert.</param>
        /// <returns>A relative path starting from the project root.</returns>
        private string GetRelativePath(string absolutePath)
        {
            return absolutePath.Replace("\\", "/").Replace(Application.dataPath.Replace("/Assets", "") + "/", "");
        }

        /// <summary>
        /// Builds the tree structure for the ProjectSettings folder.
        /// Handles nested folders by splitting file paths into parts.
        /// The root node is created here with no parent.
        /// </summary>
        /// <returns>An AssetNode representing the ProjectSettings folder and its contents.</returns>
        private AssetNode BuildProjectSettingsNode()
        {
            string projectSettingsAbsolute = Path.Combine(Application.dataPath, "../ProjectSettings").Replace("\\", "/");
            AssetNode node = new AssetNode
            {
                name = "ProjectSettings",
                path = "ProjectSettings",
                isFolder = Directory.Exists(projectSettingsAbsolute),
                parent = null
            };

            if (node.isFolder)
            {
                // Process each file (ignoring .meta files) in ProjectSettings
                foreach (string file in Directory.GetFiles(projectSettingsAbsolute, "*", SearchOption.AllDirectories))
                {
                    if (!file.EndsWith(".meta"))
                    {
                        string relativePath = "ProjectSettings/" + file.Substring(projectSettingsAbsolute.Length).TrimStart('/', '\\');
                        string[] parts = relativePath.Split('/');
                        AddNodeToTree(node, parts, 0, relativePath);
                    }
                }
            }
            return node;
        }

        /// <summary>
        /// Recursively adds nodes to the tree for each part of the file path.
        /// Parent pointers are assigned for newly created nodes.
        /// </summary>
        /// <param name="parent">The current parent node.</param>
        /// <param name="parts">Path segments split by '/' character.</param>
        /// <param name="index">Current index within the parts array.</param>
        /// <param name="fullPath">The full relative path of the file.</param>
        private void AddNodeToTree(AssetNode parent, string[] parts, int index, string fullPath)
        {
            if (index >= parts.Length)
                return;

            AssetNode child = parent.children.Find(n => n.name == parts[index]);
            if (child == null)
            {
                child = new AssetNode
                {
                    name = parts[index],
                    path = string.Join("/", parts, 0, index + 1),
                    isFolder = (index < parts.Length - 1),
                    parent = parent
                };
                parent.children.Add(child);
            }
            if (index < parts.Length - 1)
                AddNodeToTree(child, parts, index + 1, fullPath);
            else
                child.isFolder = false; // Leaf node is a file
        }

        /// <summary>
        /// Draws the "Select All" and "Deselect All" buttons for a given root node.
        /// The buttons apply to all children of the root node.
        /// </summary>
        /// <param name="root">The root node whose children will be affected.</param>
        /// <param name="label">A label indicating the section (e.g., "Assets" or "ProjectSettings").</param>
        private void DrawSelectButtonsFor(AssetNode root, string label)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All " + label))
            {
                foreach (var child in root.children)
                {
                    SetAllNodes(child, true);
                }
            }
            if (GUILayout.Button("Deselect All " + label))
            {
                foreach (var child in root.children)
                {
                    SetAllNodes(child, false);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Recursively sets the selection state for a node and all its descendants.
        /// This method skips the root node (which is non-selectable).
        /// </summary>
        /// <param name="node">The node whose selection state will be set.</param>
        /// <param name="selected">The selection state to apply.</param>
        private void SetAllNodes(AssetNode node, bool selected)
        {
            node.selected = selected;
            if (node.isFolder)
            {
                foreach (var child in node.children)
                {
                    SetAllNodes(child, selected);
                }
            }
        }

        /// <summary>
        /// Recursively draws each node in the tree.
        /// - For root nodes (Assets/ProjectSettings) no toggle is shown.
        /// - For non-root nodes, a toggle is shown; any change triggers upward propagation.
        /// - For folders, a foldout arrow is drawn.
        /// </summary>
        /// <param name="node">The node to draw in the GUI.</param>
        private void DrawNode(AssetNode node)
        {
            EditorGUILayout.BeginHorizontal();

            // For folder nodes, draw a foldout arrow
            if (node.isFolder)
            {
                Rect foldoutRect = EditorGUILayout.GetControlRect(GUILayout.Width(12));
                node.foldout = EditorGUI.Foldout(foldoutRect, node.foldout, "");
            }

            // For non-root nodes, display a toggle. For root nodes, simply display the label
            if (node.parent != null)
            {
                bool newSelected = EditorGUILayout.ToggleLeft(node.name, node.selected);
                if (newSelected != node.selected)
                {
                    node.selected = newSelected;
                    if (node.isFolder)
                    {
                        // Propagate selection downward
                        SetChildrenSelection(node, newSelected);
                    }
                    // Propagate selection upward
                    if (node.parent != null)
                    {
                        UpdateParentSelection(node.parent);
                    }
                }
            }
            else
            {
                // Root nodes ("Assets" and "ProjectSettings") are not selectable
                EditorGUILayout.LabelField(node.name);
            }
            EditorGUILayout.EndHorizontal();

            // If the folder is expanded, recursively draw its children
            if (node.isFolder && node.foldout)
            {
                EditorGUI.indentLevel++;
                foreach (var child in node.children)
                {
                    DrawNode(child);
                }
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Recursively sets the selection state for all children of a folder.
        /// </summary>
        /// <param name="node">The folder node.</param>
        /// <param name="selected">The selection state to apply.</param>
        private void SetChildrenSelection(AssetNode node, bool selected)
        {
            foreach (var child in node.children)
            {
                child.selected = selected;
                if (child.isFolder)
                {
                    SetChildrenSelection(child, selected);
                }
            }
        }

        /// <summary>
        /// Updates a parent's selection state based on its children.
        /// A parent becomes selected if any of its children are selected.
        /// This change propagates upward recursively.
        /// </summary>
        /// <param name="node">The parent node to update.</param>
        private void UpdateParentSelection(AssetNode node)
        {
            bool anySelected = false;
            foreach (var child in node.children)
            {
                if (child.selected)
                {
                    anySelected = true;
                    break;
                }
            }
            node.selected = anySelected;
            if (node.parent != null)
            {
                UpdateParentSelection(node.parent);
            }
        }

        /// <summary>
        /// Draws the GUI for the exporter window.
        /// Contains separate select/deselect buttons for Assets and ProjectSettings,
        /// the asset selection sections, export options, and the export button.
        /// </summary>
        private void OnGUI()
        {
            // Assets section
            if (assetsRootNode != null)
            {
                DrawSelectButtonsFor(assetsRootNode, "Assets");
            }
            EditorGUILayout.LabelField("Select Assets to Export", EditorStyles.boldLabel);
            assetsScrollPos = EditorGUILayout.BeginScrollView(assetsScrollPos, GUILayout.Height(200));
            if (assetsRootNode != null)
            {
                DrawNode(assetsRootNode);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // ProjectSettings section
            if (projectSettingsRootNode != null)
            {
                DrawSelectButtonsFor(projectSettingsRootNode, "ProjectSettings");
            }
            EditorGUILayout.LabelField("Select Project Settings to Export", EditorStyles.boldLabel);
            settingsScrollPos = EditorGUILayout.BeginScrollView(settingsScrollPos, GUILayout.Height(120));
            if (projectSettingsRootNode != null)
            {
                DrawNode(projectSettingsRootNode);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            // Export options
            includePackages = EditorGUILayout.ToggleLeft("Include Packages (manifest.json)", includePackages);
            includeDependencies = EditorGUILayout.ToggleLeft("Include Dependencies", includeDependencies);

            EditorGUILayout.Space();
            // Export button: triggers the export process
            if (GUILayout.Button("Export"))
            {
                ExportProject();
            }
        }

        /// <summary>
        /// Recursively collects the file paths of all selected nodes.
        /// Only file nodes (non-folders) are added.
        /// </summary>
        /// <param name="node">The current node in the tree.</param>
        /// <param name="selectedPaths">List to accumulate selected file paths.</param>
        private void GatherSelectedPaths(AssetNode node, List<string> selectedPaths)
        {
            if (!node.isFolder)
            {
                if (node.selected) selectedPaths.Add(node.path);
            }
            else
            {
                foreach (var child in node.children)
                {
                    GatherSelectedPaths(child, selectedPaths);
                }
            }
        }

        /// <summary>
        /// Exports the selected assets and project settings as a Unity package.
        /// The package is saved to a location chosen by the user.
        /// Export options include any dependencies and the package manifest based on toggles.
        /// </summary>
        private void ExportProject()
        {
            string projectName = PlayerSettings.productName.Replace(" ", "_");
            string defaultFilename = projectName + ".unitypackage";
            string exportPath = EditorUtility.SaveFilePanel("Save Unity Package", "", defaultFilename, "unitypackage");
            if (string.IsNullOrEmpty(exportPath))
            {
                Debug.Log("Export canceled by user");
                return;
            }

            List<string> exportContent = new List<string>();
            GatherSelectedPaths(assetsRootNode, exportContent);
            GatherSelectedPaths(projectSettingsRootNode, exportContent);

            if (includePackages)
            {
                string manifestFile = "Packages/manifest.json";
                string manifestAbsolute = Path.Combine(Application.dataPath, "../Packages/manifest.json");
                if (File.Exists(manifestAbsolute))
                {
                    exportContent.Add(manifestFile);
                }
            }

            if (exportContent.Count == 0)
            {
                Debug.LogWarning("No assets selected for export!");
                return;
            }

            ExportPackageOptions options = ExportPackageOptions.Interactive | ExportPackageOptions.Recurse;
            if (includeDependencies)
                options |= ExportPackageOptions.IncludeDependencies;

            AssetDatabase.ExportPackage(exportContent.ToArray(), exportPath, options);
            Debug.Log($"Exported package saved at: {exportPath}");
            Close();
        }

    }
}
