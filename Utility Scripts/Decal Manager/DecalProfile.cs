using UnityEngine;

namespace DiamondMind.Prototypes.Shooter
{
    [CreateAssetMenu(fileName = "DecalData", menuName = "DiamondMind/Shooter/DecalData")]
    public class DecalProfile : ScriptableObject
    {
        public DecalSpec defaultDecalSpec;
        public DecalSpec[] decalSpecs;
    }

    [System.Serializable]
    public struct DecalSpec
    {
        public string tag;
        public string hitEffectsName;
        public string[] decals;
    }
}
