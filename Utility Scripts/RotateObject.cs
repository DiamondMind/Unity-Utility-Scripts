using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiamondMind.Prototypes.Tools
{
    public class RotateObject : MonoBehaviour
    {
        public bool beginOnStart;
        [Tooltip("Rotation speed in degrees per second")]
        public float rotationSpeed = 360f;
        public Vector3 rotationAxis = Vector3.forward;

        bool isRotating;

        private void Awake()
        {
            isRotating = beginOnStart;
        }

        private void Update()
        {
            if (isRotating)
            {
                Rotate();
            }
        }

        private void Rotate()
        {
            float rotationAmount = rotationSpeed * Time.deltaTime;

            transform.Rotate(rotationAxis, rotationAmount);
        }

        public void StartRotating()
        {
            isRotating = true;
        }

        public void StopRotating()
        {
            isRotating = false;
        }

    }
}
