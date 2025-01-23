using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DiamondMind.Prototypes.Generic.Health
{
    public class HealthDisplayUI : MonoBehaviour
    {
        [Header("---------- Display Settings ----------")]
        [SerializeField] private HealthManager healthManager;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider damageSlider;
        [SerializeField] private float damageDecreaseSpeed = 5f;
        [SerializeField] private bool showDamageType = true;
        [SerializeField] private bool lookAtCamera = true;
        [SerializeField] private Text damageText;
        [SerializeField] private float damageTextDisplayTime = 2f;

        Transform mainCameraTransform;
        float accumulatedDamage;
        bool isDamageDelayActive;

        private void Start()
        {
            mainCameraTransform = Camera.main.transform;
            healthManager ??= GetComponentInParent<HealthManager>();

            if (healthManager == null)
            {
                Debug.LogWarning("HealthManager component not found on parent. Destroying HealthDisplayUI.");
                Destroy(this.gameObject);
                return;
            }

            healthManager._OnRecieveDamage.AddListener(UpdateDamageDisplay);
            InitializeUI();
        }

        private void InitializeUI()
        {
            healthSlider.maxValue = healthManager.MaxHealth;
            healthSlider.value = healthSlider.maxValue;

            damageSlider.maxValue = healthManager.MaxHealth;
            damageSlider.value = healthSlider.maxValue;

            damageText.text = string.Empty;
        }

        private void Update()
        {
            UpdateHealthSlider();
            RotateToFaceCamera();
        }

        private void UpdateHealthSlider()
        {
            healthSlider.value = healthManager.CurrentHealth;
        }

        private void RotateToFaceCamera()
        {
            if (lookAtCamera && mainCameraTransform != null)
            {
                transform.LookAt(mainCameraTransform.position, Vector3.up);
            }
        }

        private void UpdateDamageDisplay(int damageAmount, string damageType)
        {
            accumulatedDamage += damageAmount;

            damageText.text = ($"{accumulatedDamage:00}" + (showDamageType && !string.IsNullOrEmpty(damageType) ? $" : by {damageType}" : ""));

            if (!isDamageDelayActive && healthManager != null && healthManager.gameObject.activeInHierarchy)
            {
                StartCoroutine(DamageDelayRoutine());
            }
        }

        private IEnumerator DamageDelayRoutine()
        {
            isDamageDelayActive = true;

            while (damageSlider.value > healthSlider.value)
            {
                damageSlider.value -= damageDecreaseSpeed;

                yield return null;
            }

            isDamageDelayActive = false;

            // Show the damage text for a short duration
            yield return new WaitForSeconds(damageTextDisplayTime);

            accumulatedDamage = 0;
            damageText.text = string.Empty;
        }

    }
}