using UnityEngine;
using UnityEditor;

// this is a script that can be used to export an project with project settings
namespace DiamondMind.Prototypes.FPSPlayer
{
    public static class ExportPackage
    {
        [MenuItem("Export/Export Entire Project/Export with tags and layers, Input settings")]
        public static void Export()
        {
            string[] projectContent = new string[] {"Assets", "ProjectSettings/TagManager.asset", "ProjectSettings/InputManager.asset", "ProjectSettings/ProjectSettings.asset"};
            AssetDatabase.ExportPackage(projectContent, "Project.unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
            Debug.Log("Assets exported with dependencies and project settings");
        }
    }
}