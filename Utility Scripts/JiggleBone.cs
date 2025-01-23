// ============================================================
// Name:        Improved Jiggle Bone v.2.0
// Author:      Rabiu Oluwaseun (DiamondMind)
// Date:        9-12-2023
// License:     Free to use and modify. Credit to Fishypants for the original script.
//
// Description: This script is an improved version of the Jiggle Bone script by Fishypants.
//              It includes various enhancements for better usability, readability, and performance.
//
// Improvements Made:
// - Added tooltips for Inspector parameters.
// - Organized parameters into headers for better Inspector organization.
// - Applied gravity to bone dynamics for more realistic behavior.
// - Implemented sideways jiggle
// - Enhanced code readability and naming conventions.
// - Improved the Squash and Stretch section for clarity.
// - Updated debug visualization for easier debugging.
//
// How to Use:
// 1. Attach this script to a bone, preferably bones at the end.
// 2. Set the 'boneAxis' to be the bone's front-facing axis.
// 3. Configure other parameters as needed.
// 4. Enjoy improved jiggle dynamics for your bones!
//
// Original Script by Fishypants
// ============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiamondMind.Prototypes.Tools
{
    public class JiggleBone : MonoBehaviour
    {
        [Header("General Settings")]
        [Tooltip("Enable debug visualization.")]
        public bool debugMode = true;

        [Header("Bone Settings")]
        [Tooltip("The local bone axis")]
        public Vector3 boneAxis = Vector3.forward;
        [Tooltip("The target distance from the bone's pivot")]
        public float targetDistance = 2.0f;

        [Header("Dynamics Settings")]
        [Tooltip("Stiffness of the bone")]
        public float boneStiffness = 0.1f;
        [Tooltip("Adjust this value for the desired side jiggle effect")]
        public float sideJiggle = 0.1f;
        [Tooltip("Mass of the bone")]
        public float boneMass = 0.9f;
        [Tooltip("Damping of the bone")]
        public float boneDamping = 0.75f;
        [Tooltip("Gravity applied to the bone")]
        public float boneGravity = 0.75f;

        [Header("Squash and Stretch")]
        [Tooltip("Enable squash and stretch effect")]
        public bool squashAndStretch = true;
        [Tooltip("Stretch factor for the sides")]
        public float sideStretch = 0.15f;
        [Tooltip("Stretch factor for the front")]
        public float frontStretch = 0.2f;
        [Tooltip("Stretch factor for sideways jiggle")]

        // Dynamics variables
        private Vector3 force = Vector3.zero;
        private Vector3 acc = Vector3.zero;
        private Vector3 vel = Vector3.zero;

        // Squash and Stretch variables
        private Vector3 targetPosition = Vector3.zero;
        private Vector3 dynamicPosition = Vector3.zero;

        private void Awake()
        {
            // Set targetPosition and dynamicPosition at startup
            targetPosition = transform.position + transform.TransformDirection(boneAxis * targetDistance);
            dynamicPosition = targetPosition;
        }

        private void LateUpdate()
        {
            // Reset the bone rotation so we can recalculate the upVector and forwardVector
            transform.rotation = new Quaternion();

            // Update forwardVector and upVector
            Vector3 forwardVector = transform.TransformDirection(boneAxis * targetDistance);
            Vector3 upVector = transform.up;

            // Calculate target position
            targetPosition = transform.position + transform.TransformDirection(boneAxis * targetDistance);

            // Calculate force, acceleration, and velocity per X, Y and Z
            force = (targetPosition - dynamicPosition) * boneStiffness;

            acc = force / boneMass;
            vel += acc * (1 - boneDamping);

            // Apply gravity
            force.y -= boneGravity;
            acc = force / boneMass;

            // Update dynamic position
            dynamicPosition += vel + force;

            // Set bone rotation to look at dynamicPosition
            transform.LookAt(dynamicPosition, upVector);

            // Squash and Stretch
            if (squashAndStretch)
            {
                Vector3 dynamicVec = dynamicPosition - targetPosition;
                float stretchMag = dynamicVec.magnitude;

                Vector3 scale = Vector3.one;
                scale.x = (boneAxis.x == 0) ? 1 + (-stretchMag * sideStretch) : 1 + (stretchMag * frontStretch);
                scale.y = (boneAxis.y == 0) ? 1 + (-stretchMag * sideStretch) : 1 + (stretchMag * frontStretch);
                scale.z = (boneAxis.z == 0) ? 1 + (-stretchMag * sideStretch) : 1 + (stretchMag * frontStretch);

                transform.localScale = scale;
            }

            // Apply sideways jiggle
            Vector3 sideJiggleVec = new Vector3(
                Random.Range(-sideJiggle, sideJiggle),
                Random.Range(-sideJiggle, sideJiggle),
                Random.Range(-sideJiggle, sideJiggle)
            );

            dynamicPosition += transform.TransformDirection(sideJiggleVec);

            // Debug Visualization
            if (debugMode)
            {
                Debug.DrawRay(transform.position, forwardVector, Color.blue);
                Debug.DrawRay(transform.position, upVector, Color.green);
                Debug.DrawRay(targetPosition, Vector3.up * 0.2f, Color.yellow);
                Debug.DrawRay(dynamicPosition, Vector3.up * 0.2f, Color.red);
            }
        }
    }

}

