using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiamondMind.Prototypes.Tools
{
    public class RemoveParent : MonoBehaviour
    {
        public bool removeOnStart;
        void Start()
        {
            if (removeOnStart)
            {
                Remove();
            }
        }

        public void Remove()
        {
            transform.SetParent(null);
        }
    }
}