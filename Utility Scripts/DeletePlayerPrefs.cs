using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiamondMind.Prototypes.Tools
{
    public class DeletePlayerPrefs : MonoBehaviour
    {
        [ContextMenu("Clear All PlayerPrefs")]
        private void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("All PlayerPrefs cleared.");
        }
    }

}
