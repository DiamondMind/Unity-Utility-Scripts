using UnityEngine;

namespace DiamondMind.Prototypes.Shooter
{
    [CreateAssetMenu(fileName = "BulletPenetrationData", menuName = "DiamondMind/Shooter/BulletPenetrationData")]
    public class BulletPenetrationProfile : ScriptableObject
    {
        public BulletPenetrationSpec[] penetrationSpecs;
    }

    [System.Serializable]
    public struct BulletPenetrationSpec
    {
        public string tag;
        [Tooltip("Reduce bullet  life by the thickness of object or use strength directly")]
        public bool reduceLifeByThickness;
        [Tooltip("How much resistance to penetration the object has")]
        [Range(0f, 1f)] public float strength;
        [Tooltip("The maximum thickness to allow penetration")]
        [Range(0.1f, 2f)] public float maxThickness;
        [Range(0f, 1f)] public float accuracyLoss;
        [Tooltip("Should the projectile bounce off the object if its unable to penetrate")]
        public bool ricochet;
        [Tooltip("The friction coefficient based on the surface")]
        [Range(0f, 1f)] public float surfaceFriction;
    }
}
