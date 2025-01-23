using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DiamondMind.Prototypes.Tools;
using DiamondMind.Prototypes.Generic.Health;

namespace DiamondMind.Prototypes.Shooter
{

    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {

        #region ---------- Inspector Variables ----------

        public bool debug;
        public bool drawTrajectory;
        public GameObject gfx;
        [Header("---------- Projectile Options ----------")]
        [SerializeField] private float startVelocity = 300;
        [Tooltip("How much force is applied to the object that has been hit by projectile if it has a rigidbody")]
        [SerializeField] private float hitForce = 5f;
        [SerializeField] private int lifetime = 100;
        [SerializeField] private bool useDecals = true;

        [Header("---------- Layer & Tags ----------")]
        [Tooltip("Layer of objects to recieve damage \n This will be overwritten by the value set in the shooter manager")]
        public LayerMask damageLayers;
        [Tooltip("Tags the projectile should ignore during its lifetime \n This will be overwritten by the value set in the shooter manager")]
        public List<string> ignoreTags;

        [Header("---------- Damage Options ----------")]
        [SerializeField] private bool explodeOnImpact;
        public DamageParameters damageParameters;

        [Header("---------- Penetration Options ----------")]
        [SerializeField] private bool allowPenetration = true;
        public bool debugPenetration;
        [SerializeField] private float penetrationCheckDistance = 1f;
        [SerializeField] private bool placeDecalOnExitPoint;
        [SerializeField] private BulletPenetrationProfile penetrationData;

        [Header("---------- Ricochet Options ----------")]
        [Tooltip("Ricochet off hitted object if unable to penetrate it")]
        [SerializeField] private bool allowRicochet;
        [Tooltip("Minimum velocity to allow ricochet")]
        [SerializeField] private int minRicochetVelocity = 30;
        [Tooltip("Minimum angle in degrees at which ricochet is allowed")]
        [SerializeField, Range(1, 90)] private int minRicochetAngle = 10;

        [Header("---------- Events ----------")]
        [SerializeField] private ProjectileEvents projectileEvents;

        #endregion


        #region ---------- Private Variables ----------

        public Rigidbody Rb
        {
            get
            {
                if (_rb == null)
                {
                    _rb = GetComponent<Rigidbody>();
                }
                return _rb;
            }
            private set { _rb = value; }
        }
        Rigidbody _rb;
        Coroutine _lifeCoroutine;
        Collider _collider;
        DecalManager _decalManager;
        Explosive _explosive;

        /// <summary>
        /// Position where projectile started its journey
        /// </summary>
        [HideInInspector] public Vector3 startPos;
        [HideInInspector] public float maxRange;

        float currentVelocity;
        int damageValue;
        float hitDistance;
        Vector3 shootDirection;
        int redirections;
        int hits;
        Vector3 redirectPos;

        #endregion


        #region ---------- Initialization ----------

        void OnEnable()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _explosive = GetComponent<Explosive>();

            if (useDecals)
                _decalManager = GetComponent<DecalManager>();
        }

        public void Initialize(float velocity, float maxRange, Vector3 shootDirection, bool useGfx, bool useGravity, bool startTrajectory)
        {
            this.startVelocity = velocity;
            this.currentVelocity = startVelocity;
            this.maxRange = maxRange;
            this.shootDirection = shootDirection;
            gfx.SetActive(useGfx);
            _rb.useGravity = useGravity;
            projectileEvents.onInitialize?.Invoke();

            if(_explosive)
            {
                _explosive.damageParameters = damageParameters;
                _explosive.damageLayers = damageLayers;
            }

            if (startTrajectory)
            {
                _rb.velocity = shootDirection.normalized * velocity;
                projectileEvents.onStartTrajectory?.Invoke();

                // Start the lifetime coroutine
                if (_lifeCoroutine != null)
                {
                    StopCoroutine(_lifeCoroutine);
                }
                _lifeCoroutine = StartCoroutine(LifeTimer());
            }

            if (debug) Debug.Log("Projectile initialized");

        }

        #endregion


        #region ---------- Lieftime Behaviour ----------

        private void OnCollisionEnter(Collision collision)
        {
            if((!allowPenetration || !allowRicochet) && hits > 0)
            {
                // Prevent inadvertent hits
                return;
            }

            if (!ignoreTags.Contains(collision.gameObject.tag))
            {
                hits++;

                if (debug)
                {
                    Debug.Log("Projectile hit: " + collision.gameObject.name);
                }
                if (drawTrajectory)
                {
                    if (redirections == 0)
                        Debug.DrawLine(startPos, transform.position, Color.green, 10f);
                    else
                        Debug.DrawLine(redirectPos, transform.position, Color.green, 10f);
                }

                hitDistance = Vector3.Distance(startPos, transform.position);
                damageValue = CalculateDamage(hitDistance);

                projectileEvents.onHit?.Invoke();
                ProjectileHit(collision.gameObject);

                if (useDecals && _decalManager)
                {
                    Vector3 hitPoint = collision.GetContact(0).point;
                    Vector3 hitNormal = collision.GetContact(0).normal;
                    _decalManager.PlaceDecal(collision.gameObject, hitPoint, hitNormal);
                }

                if (lifetime > 0 && allowPenetration)
                {
                    // Stop movement
                    _rb.velocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                    HandlePenetration(collision);
                }
                else
                {
                    if (CanDeactivate())
                    {
                        Deactivate();
                    }
                }
            }
        }

        private void ProjectileHit(GameObject hittedObject)
        {
            // Apply damage if hittedObject object has a healthManager
            if (hittedObject.transform.TryGetComponent(out HealthManager healthManager) && (damageLayers.value & (1 << hittedObject.layer)) != 0 && !explodeOnImpact)
            {
                healthManager.TakeDamage(damageValue, damageParameters.damageType);
            }
            // Add force if hitted object is a rigidbody
            if (hittedObject.TryGetComponent(out Rigidbody otherRb) && !hittedObject.isStatic)
            {
                otherRb.AddForce(transform.forward * hitForce * hitDistance, ForceMode.Impulse);
            }

            // Trigger explosion if projectile is of type explosive
            if (explodeOnImpact && _explosive != null && !_explosive.exploding)
            {
                _explosive.damageParameters = damageParameters;
                _explosive.damageLayers = damageLayers;
                _explosive.TriggerExplosion();
            }
        }

        private void HandlePenetration(Collision collision)
        {
            if (penetrationData == null || penetrationData.penetrationSpecs.Length == 0)
            {
                Debug.LogWarning("No BulletPenetrationData found.");
                return;
            }

            foreach (BulletPenetrationSpec data in penetrationData.penetrationSpecs)
            {
                if (collision.gameObject.CompareTag(data.tag))
                {
                    HandlePenetrationData(collision, data);
                    return;
                }
            }

            if (debugPenetration) Debug.Log("No BulletPenetrationData found for collision with " + collision.gameObject.name);

            if (CanDeactivate())
            {
                Deactivate();
            }
            else
            {
                Debug.LogWarning("Cant deactivate now");
            }
        }

        private void HandlePenetrationData(Collision collision, BulletPenetrationSpec data)
        {
            ContactPoint contactInfo = collision.GetContact(0);
            Vector3 impactPoint = contactInfo.point;
            redirections++;
            redirectPos = impactPoint;
            Ray ray = new Ray(impactPoint + transform.forward, -transform.forward);

            if (collision.collider.Raycast(ray, out RaycastHit hitInfo, penetrationCheckDistance))
            {
                Vector3 exitPoint = hitInfo.point;
                float objectThickness = Vector3.Distance(impactPoint, exitPoint);

                if (objectThickness <= data.maxThickness)
                {
                    PenetrateObject(collision.gameObject, data, contactInfo, exitPoint, objectThickness);
                }
                else
                {
                    HandleNonPenetration(collision.gameObject, data, contactInfo);
                }
            }
            else
            {
                if (debugPenetration) Debug.Log("Unable to penetrate " + collision.gameObject.name + " Check raycast");
                if (CanDeactivate())
                {
                    Deactivate();
                }
                else
                {
                    Debug.LogWarning("Cant deactivate now");
                }
            }
        }

        private void PenetrateObject(GameObject hittedObject, BulletPenetrationSpec data, ContactPoint contactInfo, Vector3 exitPoint, float objectThickness)
        {
            // Move the projectile to the exit point with an offset based on its size
            transform.position = exitPoint + transform.forward * _collider.bounds.size.z;

            // Adjust the projectile lifetime and velocity based on penetration data
            lifetime = Mathf.RoundToInt(lifetime * (1 - (data.reduceLifeByThickness ? (data.strength * objectThickness) : data.strength)));
            currentVelocity = currentVelocity * (1 - (data.reduceLifeByThickness ? (data.strength * objectThickness) : data.strength));

            if (data.accuracyLoss > 0f)
                shootDirection += new Vector3(
                        Random.Range(-data.accuracyLoss, data.accuracyLoss),
                        Random.Range(-data.accuracyLoss, data.accuracyLoss),
                        Random.Range(-data.accuracyLoss, data.accuracyLoss)
                );

            _rb.velocity = shootDirection.normalized * currentVelocity;
            if (_decalManager && useDecals && placeDecalOnExitPoint)
            {
                _decalManager.PlaceDecal(hittedObject, exitPoint, -contactInfo.normal);
            }

            if (lifetime > 0)
            {
                projectileEvents.onPenetrate?.Invoke();

                if (debugPenetration)
                    Debug.Log("Penetrated " + hittedObject.name + ", Object thickness: " + objectThickness + ", Lifetime: " + lifetime + ", Velocity: " + currentVelocity);
                if (drawTrajectory)
                    Debug.DrawLine(redirectPos, redirectPos + shootDirection * 5f, Color.green, 10f);
            }
            else
            {
                if (CanDeactivate())
                {
                    Deactivate();
                }
                else
                {
                    Debug.LogWarning("Cant deactivate now");
                }
            }
        }

        private void HandleNonPenetration(GameObject hittedObject, BulletPenetrationSpec data, ContactPoint contactInfo)
        {
            if (debugPenetration) Debug.Log(hittedObject.name + " is too thick to penetrate");

            if (data.ricochet && allowRicochet && RicochetAngle(contactInfo.normal) >= minRicochetAngle)
            {
                if (debug) Debug.Log("Projectile is ricocheting off " + hittedObject.name);
                HandleRicochet(contactInfo, data.surfaceFriction);
            }
            else
            {
                if (CanDeactivate())
                {
                    Deactivate();
                }
                else
                {
                    Debug.LogWarning("Cant deactivate now");
                }
            }
        }

        private float RicochetAngle(Vector3 normal)
        {
            float angle = Vector3.Angle(-shootDirection.normalized, normal);

            return angle;
        }

        private void HandleRicochet(ContactPoint contactInfo, float frictionCoefficient)
        {
            projectileEvents.onRicochet?.Invoke();
            redirections++;
            redirectPos = contactInfo.point;
            Vector3 reflectVector = Vector3.Reflect(shootDirection.normalized, contactInfo.normal);
            lifetime = Mathf.RoundToInt(lifetime * (1 - frictionCoefficient));
            currentVelocity = currentVelocity * (1 - frictionCoefficient);

            if (currentVelocity < minRicochetVelocity && lifetime <= 0)
            {
                if (CanDeactivate())
                {
                    Deactivate();
                }
                else
                {
                    Debug.LogWarning("Cant deactivate now");
                }   // Deactivate projectile if speed and lifetime are too low to continue
                return;
            }

            // Set new velocity
            _rb.velocity = reflectVector.normalized * currentVelocity;
            if (drawTrajectory) Debug.DrawLine(contactInfo.point, contactInfo.point + reflectVector * 1f, Color.green, 10f);
        }

        private int CalculateDamage(float distance)
        {
            if (!damageParameters.damageByDistance)
            {
                return damageParameters.defaultDamage;
            }

            if (distance <= damageParameters.minDistance)
            {
                return damageParameters.maxDamage;
            }
            else if (distance >= damageParameters.maxDistance)
            {
                return damageParameters.minDamage;
            }
            else
            {
                // Linear interpolation between max and min damage
                float t = (distance - damageParameters.maxDistance) / (damageParameters.minDistance - damageParameters.maxDistance);
                return Mathf.RoundToInt(Mathf.Lerp(damageParameters.maxDamage, damageParameters.minDamage, t));
            }
        }

        private IEnumerator LifeTimer()
        {
            // Calculate the time to reach the maxRange and deactivate projectile object
            float timeToDeactivate = maxRange / startVelocity;
            yield return new WaitForSeconds(timeToDeactivate);

            if (CanDeactivate())
            {
                Deactivate();
            }
            else
            {
                Debug.LogWarning("Cant deactivate now");
            }
        }

        #endregion


        #region ---------- Deactivation ----------

        private bool CanDeactivate()
        {
            if (!explodeOnImpact && _explosive == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public void Deactivate()
        {
            projectileEvents.onDeactivate?.Invoke();

            if (this.TryGetComponent(out ReturnToPool component))
            {
                component.Return(0);
            }
            else
            {
                Destroy(this.gameObject);
            }

        }

        #endregion


        #region ---------- Others ----------

        [System.Serializable]
        public struct ProjectileEvents
        {
            public UnityEvent onInitialize;
            public UnityEvent onStartTrajectory;
            public UnityEvent onHit;
            public UnityEvent onPenetrate;
            public UnityEvent onRicochet;
            public UnityEvent onDeactivate;
        }

        #endregion

    }
}
