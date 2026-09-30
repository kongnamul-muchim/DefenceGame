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

        [Header("Visual Effects")]
        [SerializeField] private float hitFlashDuration = 0.1f;
        [SerializeField] private Color hitFlashColor = new Color(1f, 1f, 1f, 0.5f); // 투명한 흰색

        private HealthBar healthBar;
        private Unit lastAttacker;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private bool isFlashing = false;

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

            // SpriteRenderer 캐싱
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
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

            // 피격 시각 효과
            FlashOnHit();

            OnHealthChanged?.Invoke(currentHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void FlashOnHit()
        {
            if (spriteRenderer == null || isFlashing) return;

            isFlashing = true;
            spriteRenderer.color = hitFlashColor;

            // 코루틴으로 원래 색상 복원
            StartCoroutine(RestoreOriginalColor());
        }

        private System.Collections.IEnumerator RestoreOriginalColor()
        {
            yield return new WaitForSeconds(hitFlashDuration);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            isFlashing = false;
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
