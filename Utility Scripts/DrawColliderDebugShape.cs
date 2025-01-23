using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiamondMind.Prototypes.Tools
{
    [RequireComponent(typeof(Collider))]
    public class DrawColliderDebugShape : MonoBehaviour
    {
        public bool drawAsWire;
        public float transparency = 0.5f;
        public Color gizmoColor = Color.red;

        private void OnDrawGizmos()
        {
            Collider collider = GetComponent<Collider>();

            if (collider != null)
            {
                if (drawAsWire)
                {
                    Gizmos.color = gizmoColor;
                }
                else
                {
                    Color fullColor = gizmoColor;
                    fullColor.a = transparency;
                    Gizmos.color = fullColor;
                }

                if (collider is SphereCollider)
                {
                    DrawSphereGizmo((SphereCollider)collider);
                }
                else if (collider is BoxCollider)
                {
                    DrawBoxGizmo((BoxCollider)collider);
                }
                else if (collider is CapsuleCollider)
                {
                    DrawCapsuleGizmo((CapsuleCollider)collider);
                }
                else if (collider is MeshCollider)
                {
                    DrawMeshGizmo((MeshCollider)collider);
                }
                else if (collider is WheelCollider)
                {
                    DrawWheelGizmo((WheelCollider)collider);
                }
            }
        }

        private void DrawSphereGizmo(SphereCollider sphereCollider)
        {
            Vector3 center = transform.TransformPoint(sphereCollider.center);
            float radius = sphereCollider.radius * Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));

            if (drawAsWire)
            {
                Gizmos.DrawWireSphere(center, radius);
            }
            else
            {
                Gizmos.DrawSphere(center, radius);
            }
        }
        private void DrawBoxGizmo(BoxCollider boxCollider)
        {
            Vector3 center = transform.TransformPoint(boxCollider.center);
            Vector3 size = Vector3.Scale(boxCollider.size, transform.lossyScale);
            Quaternion rotation = transform.rotation;

            if (drawAsWire)
            {
                Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, size);
                Gizmos.matrix = Matrix4x4.identity; // Reset matrix
            }
            else
            {
                Gizmos.matrix = Matrix4x4.TRS(center, rotation, size);
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
                Gizmos.matrix = Matrix4x4.identity; // Reset matrix
            }
        }

        private void DrawCapsuleGizmo(CapsuleCollider capsuleCollider)
        {
            Vector3 center = transform.TransformPoint(capsuleCollider.center);
            float radius = capsuleCollider.radius * Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
            float height = capsuleCollider.height * Mathf.Abs(transform.lossyScale.y);
            float halfHeight = height / 2;
            Quaternion rotation = transform.rotation;

            if (drawAsWire)
            {
                // Top hemisphere
                Gizmos.matrix = Matrix4x4.TRS(center + Vector3.up * halfHeight, rotation, Vector3.one);
                Gizmos.DrawWireSphere(Vector3.zero, radius);

                // Bottom hemisphere
                Gizmos.matrix = Matrix4x4.TRS(center - Vector3.up * halfHeight, rotation, Vector3.one);
                Gizmos.DrawWireSphere(Vector3.zero, radius);

                // Side cylinder
                Gizmos.matrix = Matrix4x4.TRS(center, rotation, new Vector3(radius * 2, height, radius * 2));
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                Gizmos.matrix = Matrix4x4.identity; // Reset matrix
            }
            else
            {
                // Top hemisphere
                DrawTransparentSphere(center + Vector3.up * halfHeight, radius);

                // Bottom hemisphere
                DrawTransparentSphere(center - Vector3.up * halfHeight, radius);

                // Side cylinder
                DrawTransparentCube(center, new Vector3(radius * 2, height, radius * 2));
            }
        }

        private void DrawMeshGizmo(MeshCollider meshCollider)
        {
            if (drawAsWire)
            {
                // Draw the mesh as a wireframe using the actual mesh data.
                Mesh mesh = meshCollider.sharedMesh;
                if (mesh != null)
                {
                    Gizmos.DrawMesh(mesh, transform.position, transform.rotation * meshCollider.transform.rotation, transform.lossyScale);
                }
            }
            else
            {
                // Draw the mesh as a full shape with transparency.
                Color fullColor = gizmoColor;
                fullColor.a = transparency;
                Gizmos.color = fullColor;
                Gizmos.DrawMesh(meshCollider.sharedMesh, transform.position, transform.rotation * meshCollider.transform.rotation, transform.lossyScale);
            }
        }


        private void DrawWheelGizmo(WheelCollider wheelCollider)
        {
            Vector3 center = transform.TransformPoint(wheelCollider.center);
            float radius = wheelCollider.radius * Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
            Quaternion rotation = transform.rotation;

            if (drawAsWire)
            {
                Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
                Gizmos.DrawWireSphere(Vector3.zero, radius);
                Gizmos.matrix = Matrix4x4.identity; // Reset matrix
            }
            else
            {
                Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
                Gizmos.DrawSphere(Vector3.zero, radius);
                Gizmos.matrix = Matrix4x4.identity; // Reset matrix
            }
        }

        private void DrawTransparentSphere(Vector3 position, float radius)
        {
            Color transparentColor = gizmoColor;
            transparentColor.a = transparency;
            Gizmos.color = transparentColor;
            Gizmos.DrawSphere(position, radius);
        }

        private void DrawTransparentCube(Vector3 position, Vector3 size)
        {
            Color transparentColor = gizmoColor;
            transparentColor.a = transparency;
            Gizmos.color = transparentColor;
            Gizmos.DrawCube(position, size);
        }
    }
}
