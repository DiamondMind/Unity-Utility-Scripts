using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DiamondMind.Prototypes.Tools;
using DiamondMind.Prototypes.Generic.Health;

namespace DiamondMind.Prototypes.Shooter
{
    public enum ExplodeMethod
    {
        Normal, StartTimer, ProximityEnter, ProximityEnterTimer, Remote, RemoteTimer
    }

    public class Explosive : MonoBehaviour
    {
        public bool debug;
        [Header("---------- Explosive Options ----------")]
        [SerializeField] private bool showGizmos;
        public ExplodeMethod explodeMethod;
        public DamageParameters damageParameters;
        [Tooltip("Tag of obejcts to ignore if explode method is set to ProximityEnter or ProximityEnterTime")]
        public List<string> proximityIgnoreTags;
        public LayerMask damageLayers;
        [Tooltip("The exact prefab name of the explosion effect to use")]
        [SerializeField] private string explodeEffectName;
        [SerializeField] private float minExplosionForce = 2000;
        [SerializeField] private float maxExplosionForce = 5000;
        [SerializeField] private LayerMask applyForceLayer;
        [SerializeField] private float maxExplosionRadius = 10;
        [SerializeField] private float upwardsModifier = 1;
        [SerializeField] private ForceMode forceMode;
        [SerializeField, Range(1f, 30f)] private float timeToExplode = 10f;

        [Header("---------- Events ----------")]
        [SerializeField] private ExplodeEvents explodeEvents;

        Coroutine _explodeCoroutine;
        public bool exploding { get; private set; }

        private void Start()
        {
            if(explodeMethod == ExplodeMethod.StartTimer)
            {
                TriggerExplosion();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (proximityIgnoreTags.Contains(other.tag))
                return;

            if (explodeMethod == ExplodeMethod.ProximityEnter || explodeMethod == ExplodeMethod.ProximityEnterTimer)
            {
                TriggerExplosion();
                explodeEvents.onProximityEnter?.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (proximityIgnoreTags.Contains(other.tag))
                return;

            if (explodeMethod == ExplodeMethod.ProximityEnter || explodeMethod == ExplodeMethod.ProximityEnterTimer)
            {
                explodeEvents.onProximityExit?.Invoke();
            }
        }

        public void TriggerExplosion()
        {
            if (_explodeCoroutine != null && !exploding)
            {
                StopCoroutine(_explodeCoroutine);
                _explodeCoroutine = null;
            }

            if(explodeMethod == ExplodeMethod.Normal)
            {
                _explodeCoroutine = StartCoroutine(Explode(0));
            }
            else if (explodeMethod == ExplodeMethod.ProximityEnter || explodeMethod == ExplodeMethod.Remote)
            {
                _explodeCoroutine = StartCoroutine(Explode(0));
            }
            else if(explodeMethod == ExplodeMethod.StartTimer || explodeMethod == ExplodeMethod.ProximityEnterTimer || explodeMethod == ExplodeMethod.RemoteTimer)
            {
                _explodeCoroutine = StartCoroutine(Explode(timeToExplode));
            }

        }

        public void StopExplosion()
        {
            if (_explodeCoroutine != null)
            {
                StopCoroutine(_explodeCoroutine);
                _explodeCoroutine = null;

                exploding = false;


                if(debug) Debug.Log("Explosion stopped");
            }
        }

        private IEnumerator Explode(float delay)
        {
            exploding = true;

            if (delay > 0f)
            {
                explodeEvents.onStartTimer?.Invoke();

                // Count down the delay in seconds
                float elapsedTime = 0f;
                while (elapsedTime < delay)
                {
                    yield return new WaitForSeconds(1f);
                    elapsedTime += 1f;
                    explodeEvents.onTick?.Invoke();  // Invoke tick event every second
                }

                ExplosionImpact();
                ObjectPool.Instance.Get(explodeEffectName, transform.position, Quaternion.identity);
                explodeEvents.onEndTimer?.Invoke();
                explodeEvents.onExplode?.Invoke();
            }
            else
            {
                ExplosionImpact();
                ObjectPool.Instance.Get(explodeEffectName, transform.position, Quaternion.identity);
                explodeEvents.onExplode?.Invoke();
            }

            exploding = false;
            Deactivate();
        }

        private void ExplosionImpact()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, maxExplosionRadius, applyForceLayer);

            foreach (Collider col in colliders)
            {
                // Add explosion force
                if (col.gameObject.TryGetComponent(out Rigidbody otherRb) && !col.gameObject.isStatic)
                {
                    float distance = Vector3.Distance(otherRb.position, transform.position);
                    float force = Mathf.Lerp(maxExplosionForce, minExplosionForce, distance / maxExplosionRadius);
                    otherRb.AddExplosionForce(force, transform.position, maxExplosionRadius, upwardsModifier, forceMode);
                }
                // Apply explosion damage
                if (col.gameObject.TryGetComponent(out HealthManager healthManager) && (damageLayers.value & (1 << col.gameObject.layer)) != 0)
                {
                    float distance = Vector3.Distance(col.transform.position, transform.position);
                    int damageAmount = Mathf.RoundToInt(Mathf.Lerp(damageParameters.maxDamage, damageParameters.minDamage, distance / maxExplosionRadius));
                    healthManager.TakeDamage(damageAmount, damageParameters.damageType);
                }

                if (debug) Debug.Log("Exploded");
            }
        }

        public void Deactivate()
        {
            explodeEvents.onDeactivate?.Invoke();

            if (this.TryGetComponent(out Projectile component))
            {
                component.Deactivate();
            }
        }

        public void RemoveParent()
        {
            transform.parent = null;
        }

        public void RemoveParentOfOther(Transform other)
        {
            other.parent = null;
        }

        void OnDrawGizmosSelected()
        {
            if (!showGizmos)
                return;

            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawSphere(transform.position, maxExplosionRadius);
        }

        [System.Serializable]
        public struct ExplodeEvents
        {
            public UnityEvent onProximityEnter;
            public UnityEvent onProximityExit;
            public UnityEvent onStartTimer;
            public UnityEvent onTick;
            public UnityEvent onEndTimer;
            public UnityEvent onExplode;
            public UnityEvent onDeactivate;
        }

    }
}