using UnityEngine;

namespace DefenceGame.Core
{
    /// <summary>
    /// 적의 체력 관리를 담당하는 컴포넌트
    /// 단일 책임: 체력 관리
    /// </summary>
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Health Data")]
        [SerializeField] private float maxHealth;
        [SerializeField] private float currentHealth;
        
        private HealthBar healthBar;
        private Unit lastAttacker;
        
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsAlive => currentHealth > 0;
        public Unit LastAttacker => lastAttacker;
        
        public System.Action OnDeath;
        public System.Action<float> OnHealthChanged;
        
        public void Initialize(float health, HealthBar bar)
        {
            maxHealth = health;
            currentHealth = maxHealth;
            healthBar = bar;
            
            if (healthBar != null)
            {
                healthBar.Initialize(maxHealth);
            }
        }
        
        public void TakeDamage(float damage, Unit attacker = null)
        {
            if (!IsAlive) return;
            
            currentHealth -= damage;
            
            // Track last attacker
            if (attacker != null)
            {
                lastAttacker = attacker;
            }
            
            // Update health bar
            if (healthBar != null)
            {
                healthBar.UpdateHealth(currentHealth);
            }
            
            OnHealthChanged?.Invoke(currentHealth);
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        private void Die()
        {
            OnDeath?.Invoke();
        }
        
        public void Reset()
        {
            currentHealth = maxHealth;
            lastAttacker = null;
            
            if (healthBar != null)
            {
                healthBar.UpdateHealth(currentHealth);
            }
        }
    }
}
