using UnityEditor;
using UnityEngine;

namespace DiamondMind.Prototypes.Tools
{
    public class MissingScriptFinder : MonoBehaviour
    {
        [MenuItem("DiamondMind/Tools/Find Missing Scripts in Scene")]
        static void FindMissingScripts()
        {
            var gameObjects = GameObject.FindObjectsByType<GameObject>((FindObjectsSortMode)FindObjectsInactive.Include);
            int count = 0;

            foreach (var go in gameObjects)
            {
                var components = go.GetComponents<Component>();
                foreach (var c in components)
                {
                    if (c == null)
                    {
                        Debug.LogWarning(
                            $"Missing script found on GameObject: {go.name}",
                            go
                        );
                        count++;
                    }
                }
            }

            Debug.Log($"Scan complete. Missing scripts found: {count}");
        }
    }

}
