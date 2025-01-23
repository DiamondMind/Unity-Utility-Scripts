using System;
using System.Linq;
using UnityEngine;
using DiamondMind.Prototypes.Tools;

namespace DiamondMind.Prototypes.Shooter
{
    public class DecalManager : MonoBehaviour
    {
        [Header("---------- Decals & Hit Effects Options ----------")]
        [Tooltip("The layer mask to specify which layers can have decals")]
        public LayerMask decalLayer;
        public DecalProfile decalData;

        public void PlaceDecal(GameObject target, Vector3 position, Vector3 normal)
        {
            if (!IsDecalLayer(target.layer))
                return;

            // Find the decal data or use default if not found
            DecalSpec decalSpec = Array.Find(decalData.decalSpecs, data => target.CompareTag(data.tag));
            if (decalSpec.Equals(default(DecalSpec)))
            {
                if (decalData.defaultDecalSpec.Equals(default(DecalSpec)))
                    return;

                decalSpec = decalData.defaultDecalSpec;
            }

            Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);

            // Instantiate hit particle effects
            if (!string.IsNullOrWhiteSpace(decalSpec.hitEffectsName))
            {
                GameObject effect = ObjectPool.Instance.Get(decalSpec.hitEffectsName, position, rotation);
                effect.transform.SetParent(target.transform, true);
            }
            else
            {
                Debug.LogWarning("No hit effect to instantiate");
            }

            // Instantiate decal
            if (decalSpec.decals != null && decalSpec.decals.Any())
            {
                string[] decals = decalSpec.decals;
                if (decals.Length > 1)
                {
                    int index = UnityEngine.Random.Range(0, decals.Length);
                    GameObject decalObj = ObjectPool.Instance.Get(decalSpec.decals[index], position, rotation);
                    decalObj.transform.SetParent(target.transform, true);
                }
                else
                {
                    GameObject decalObj = ObjectPool.Instance.Get(decals[0], position, rotation);
                    decalObj.transform.SetParent(target.transform, true);
                }
            }
            else
            {
                Debug.LogWarning("No decal to instantiate");
            }
        }

        private bool IsDecalLayer(int layer)
        {
            // Check if a given layer is included in the decal layer mask
            return (decalLayer & (1 << layer)) != 0;
        }

    }
}
