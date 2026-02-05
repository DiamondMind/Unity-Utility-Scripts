using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiamondMind.Prototypes.Tools
{
    [ExecuteAlways]
    public class ObjectNote : MonoBehaviour
    {
        public string title;

        [TextArea(3, 10)]
        public string message;
    }

}
