using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DiamondMind.Prototypes.Generic.Health
{
    public class HealthManager : MonoBehaviour
    {
        [Header("---------- Health ----------")]
        [SerializeField] private bool isImmortal;
        [SerializeField] private float currentHealth;
        [SerializeField] private int startHealth = 100;
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private List<string> ignoreDamageTypes;
        [Header("---------- Recovery ----------")]
        [SerializeField] private bool canRecoverHealth;
        [SerializeField] private int healthRecovery = 5;
        [SerializeField] private float healthRecoveryDelay = 3f;
        [Header("---------- Shield ----------")]
        [SerializeField] private bool useShield;
        [SerializeField] private int shieldStrength = 50;
        [SerializeField] private int maxShieldStrength = 100;
        [SerializeField] private List<string> shieldDamageTypes;
        [Header("---------- Events ----------")]
        [SerializeField] private HealthEvents healthEvents;

        public OnRecieveDamage _OnRecieveDamage
        {
            get { return healthEvents.onRecieveDamage; }
        }
        public bool isDead 
        { 
            get; 
            private set; 
        }
        public int MaxHealth
        {
            get { return maxHealth; }
        }
        public int CurrentHealth
        {
            get { return Mathf.RoundToInt(currentHealth); }
        }

        float currentRecoveryDelay;
        bool recoveringHealth;

        private void Start()
        {
            currentHealth = startHealth > 0 ? startHealth : maxHealth; currentRecoveryDelay = healthRecoveryDelay;
            shieldStrength = maxShieldStrength;
        }

        private void FixedUpdate()
        {
            if (canRecoverHealth && !isDead && currentHealth < maxHealth)
            {
                HandleHealthRecovery();
            }
        }

        private void HandleHealthRecovery()
        {
            if (recoveringHealth)
            {
                currentHealth += healthRecovery * Time.fixedDeltaTime;
                if (currentHealth >= maxHealth)
                {
                    currentHealth = maxHealth;
                    recoveringHealth = false;
                    currentRecoveryDelay = healthRecoveryDelay;
                    healthEvents.onFinishHealthRecovery?.Invoke();
                }
            }
            else
            {
                currentRecoveryDelay -= Time.deltaTime;
                if (currentRecoveryDelay <= 0)
                {
                    recoveringHealth = true;
                    healthEvents.onStartHealthRecovery?.Invoke();
                }
            }
        }

        public void TakeDamage(int damageAmount, string damageType)
        {
            if (isImmortal || isDead || ignoreDamageTypes.Contains(damageType)) 
                return;

            if (useShield && shieldDamageTypes.Contains(damageType))
            {
                // Reduce the shield value by the damage amount
                if (shieldStrength >= damageAmount)
                {
                    shieldStrength -= damageAmount; // Shield absorbs the full damage
                    damageAmount = 0;
                    healthEvents.onShieldAbsorbDamage?.Invoke();
                }
                else
                {
                    damageAmount -= shieldStrength; // Shield absorbs part of the damage
                    healthEvents.onShieldAbsorbDamage?.Invoke();
                    shieldStrength = 0;      
                    healthEvents.onShieldBreak?.Invoke();
                }
            }

            // Damage left after passing shield is applied to health
            if (damageAmount > 0)
            {
                currentHealth -= damageAmount;
                healthEvents.onRecieveDamage?.Invoke(damageAmount, damageType);
            }

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
            else
            {
                recoveringHealth = false;
                currentRecoveryDelay = healthRecoveryDelay;
            }
        }

        private void Die()
        {
            currentHealth = 0;
            isDead = true;
            recoveringHealth = false;
            healthEvents.onDead?.Invoke();
        }

        public void Heal(int healAmount)
        {
            if (isDead) 
                return;

            currentHealth += healAmount;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
        }

        public void ReplaceShield()
        {
            if (isDead)
                return;

            useShield = true;
            shieldStrength = maxShieldStrength;
        }

        public void Revive(float healthPercentage = 1f)
        {
            if (!isDead) 
                return;

            isDead = false;
            currentHealth = Mathf.RoundToInt(maxHealth * healthPercentage);
        }

        public void ChangeMaxHealth(int newValue)
        {
            maxHealth = newValue;
        }

        public void ChangeMaxShieldStrength(int newValue)
        {
            maxShieldStrength = newValue;
        }


        [System.Serializable]
        public struct HealthEvents
        {
            public OnRecieveDamage onRecieveDamage;
            public UnityEvent onShieldAbsorbDamage;
            public UnityEvent onShieldBreak;
            public UnityEvent onStartHealthRecovery;
            public UnityEvent onFinishHealthRecovery;
            public UnityEvent onDead;
        }

        [System.Serializable]
        public class OnRecieveDamage : UnityEvent<int, string> { }

    }
}
