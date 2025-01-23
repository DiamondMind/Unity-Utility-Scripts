using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiamondMind.Prototypes.Tools
{
    public class LookAtCamera : MonoBehaviour
    {
        [Tooltip("Align position to always stay on top of parent")]
        public bool alignUp = false;
        [Tooltip("Height of alignment on top of parent")]
        public float alignmentHeight = 1;
        [Tooltip("Use smooth rotation to look at the camera")]
        public bool useSmoothRotation = true;
        public bool onlyRotateY;

        Transform parent;
        Camera mainCamera;

        private void Start()
        {
            parent = transform.parent;
            mainCamera = Camera.main;
        }

        private void FixedUpdate()
        {
            AlignWithParent();

            if (!mainCamera)
                return;

            Look();
        }

        private void AlignWithParent()
        {
            if (alignUp && parent)
                transform.position = parent.position + Vector3.up * alignmentHeight;
        }

        private void Look()
        {
            Vector3 lookDirection = mainCamera.transform.position - transform.position;

            // If onlyRotateY is true, zero out the x and z components of the lookDirection
            if (onlyRotateY)
            {
                lookDirection.x = 0;
                lookDirection.z = 0;
            }

            var targetRotation = Quaternion.LookRotation(lookDirection);

            if (useSmoothRotation)
            {
                SmoothRotate(targetRotation);
            }
            else
            {
                InstantRotate(targetRotation);
            }
        }

        private void SmoothRotate(Quaternion targetRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 4f);
            if (onlyRotateY)
            {
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            }
        }

        private void InstantRotate(Quaternion targetRotation)
        {
            if (onlyRotateY)
            {
                transform.eulerAngles = new Vector3(0, targetRotation.eulerAngles.y, 0);
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
    }
}
