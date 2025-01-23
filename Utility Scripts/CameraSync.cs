using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiamondMind.Prototypes.Characters.FPS
{
    public class CameraSync : MonoBehaviour
    {
        public Camera mainCamera;
        public Camera weaponCamera;

        void LateUpdate()
        {
            if (weaponCamera.fieldOfView != mainCamera.fieldOfView)
            {
                weaponCamera.fieldOfView = mainCamera.fieldOfView;
            }

        }
    }

}
