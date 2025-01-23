using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace DiamondMind.Prototypes.Tools
{
    public class DestroyGameObject : MonoBehaviour
    {
        public bool triggerOnStart = true;
        public float destroyDelay;
        public UnityEngine.Events.UnityEvent onDestroy;

        private IEnumerator Start()
        {
            if (!triggerOnStart)
                yield break;

            yield return new WaitForSeconds(destroyDelay);

            Destroy();
        }

        public void Destroy()
        {
            onDestroy.Invoke();
            Destroy(this.gameObject);
        }
    }
}
