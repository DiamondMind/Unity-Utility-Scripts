using UnityEditor;
using UnityEngine;

namespace DiamondMind.Prototypes.Tools
{
    public static class SceneViewToCamera
    {
        [MenuItem("DiamondMind/Tools/Align Main Camera To Scene View %#m")]
        private static void AlignMainCamera()
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogError("No Main Camera found in the scene.");
                return;
            }

            SceneView sceneView = SceneView.lastActiveSceneView;

            if (sceneView == null)
            {
                Debug.LogError("No active Scene View found.");
                return;
            }

            Transform sceneCamTransform = sceneView.camera.transform;
            Transform mainCamTransform = mainCamera.transform;

            Undo.RecordObject(mainCamTransform, "Align Main Camera To Scene View");

            mainCamTransform.position = sceneCamTransform.position;
            mainCamTransform.rotation = sceneCamTransform.rotation;

            EditorUtility.SetDirty(mainCamTransform);

            Debug.Log("Main Camera aligned to Scene View.");
        }
    }
}

