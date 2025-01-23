using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiamondMind.Prototypes.Tools
{
    public class ObjectContainer : MonoBehaviour
    {
        public static ObjectContainer Instance;

        // Lazy Initialization
        public static Transform root
        {
            get
            {
                if (Instance == null)
                {
                    Instance = new GameObject("Object Container", typeof(ObjectContainer)).GetComponent<ObjectContainer>();
                }
                return Instance.transform;
            }
        }

    }

}
