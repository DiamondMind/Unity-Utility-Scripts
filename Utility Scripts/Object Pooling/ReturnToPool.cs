using System.Collections;
using UnityEngine;

namespace DiamondMind.Prototypes.Tools
{
    public class ReturnToPool : MonoBehaviour
    {
        public bool automaticReturn;
        public float returnDelay;
        public UnityEngine.Events.UnityEvent onReturn;

        private void OnEnable()
        {
            // Start the return process when the object is enabled
            if (automaticReturn)
            {
                StartCoroutine(TriggerReturn(returnDelay));
            }
        }

        public void Return(float delay)
        {
            StartCoroutine(TriggerReturn(delay));
        }

        private IEnumerator TriggerReturn(float delay)
        {
            yield return new WaitForSeconds(delay);

            onReturn.Invoke();
            ObjectPool.Instance.Return(this.gameObject.name, this.gameObject);
        }

    }
}
