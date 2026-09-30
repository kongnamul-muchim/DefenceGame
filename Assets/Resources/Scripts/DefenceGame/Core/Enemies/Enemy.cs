using UnityEngine;
using DefenceGame.Data;
using DefenceGame.Core.Systems.Defense;
using DefenceGame.Core.Managers;

namespace DefenceGame.Core
{
    /// <summary>
    /// 적의 핵심 클래스 - 각 컴포넌트를 조율
    /// 단일 책임: 데이터 관리 및 컴포넌트 조율
    /// </summary>
    public class Enemy : MonoBehaviour, IDamageable
    {
        [Header("Enemy Data")]
        public int id;
        public string enemyName;
        public int rewardGold;

        // Backward compatibility properties
        public float currentHealth => healthComponent?.CurrentHealth ?? 0;
        public float maxHealth => healthComponent?.MaxHealth ?? 0;
        public float speed => movementComponent?.CurrentSpeed ?? 0;

        [Header("Components")]
        public SpriteRenderer spriteRenderer;

        // Component references
        private EnemyHealth healthComponent;
        private EnemyMovement movementComponent;
        private EnemyStatusEffect statusEffectComponent;
        private PathAgent pathAgent;

        private bool isInitialized = false;
        private bool hasReachedCastle = false;
        private Vector3 targetPosition;

        // Events
        public System.Action<Enemy> OnEnemyDefeated;
        public System.Action<Enemy> OnEnemyReachedCastle;

        private void Awake()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            healthComponent = GetComponent<EnemyHealth>() ?? gameObject.AddComponent<EnemyHealth>();
            movementComponent = GetComponent<EnemyMovement>() ?? gameObject.AddComponent<EnemyMovement>();
            statusEffectComponent = GetComponent<EnemyStatusEffect>() ?? gameObject.AddComponent<EnemyStatusEffect>();
            pathAgent = GetComponent<PathAgent>() ?? gameObject.AddComponent<PathAgent>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            // Subscribe to component events
            healthComponent.OnDeath += HandleDeath;
            statusEffectComponent.OnSpeedChanged += HandleSpeedChanged;
        }

        private void OnEnable()
        {
            if (pathAgent != null)
            {
                pathAgent.OnPathComplete += OnPathComplete;
            }
        }

        private void OnDisable()
        {
            if (pathAgent != null)
            {
                pathAgent.OnPathComplete -= OnPathComplete;
            }
        }

        private void OnDestroy()
        {
            if (healthComponent != null)
            {
                healthComponent.OnDeath -= HandleDeath;
            }
            if (statusEffectComponent != null)
            {
                statusEffectComponent.OnSpeedChanged -= HandleSpeedChanged;
            }

            if (WaveManager.Instance != null && !hasReachedCastle)
            {
                WaveManager.Instance.EnemyReachedCastle();
            }
        }

        public void Initialize(EnemyData data, float healthMultiplier, Vector3 targetPos)
        {
            id = data.Id;
            enemyName = data.Name;
            rewardGold = data.RewardGold;
            targetPosition = targetPos;

            // Initialize health
            HealthBar healthBar = GetComponentInChildren<HealthBar>(true);
            healthComponent.Initialize(data.Health * healthMultiplier, healthBar);

            // Initialize movement
            movementComponent.Initialize(data.Speed, targetPos);

            SetEnemySprite(data.Name);

            isInitialized = true;
            hasReachedCastle = false;
        }

        private void Update()
        {
            if (!isInitialized) return;

            movementComponent.UpdateMovement();
            CheckCastleReached();
        }

        private void CheckCastleReached()
        {
            if (!hasReachedCastle && CastleManager.Instance != null)
            {
                if (CastleManager.Instance.IsInCastleBounds(transform.position))
                {
                    ReachCastle();
                }
            }
        }

        private void ReachCastle()
        {
            if (hasReachedCastle) return;

            hasReachedCastle = true;

            CastleManager.Instance?.DamageCastle(1);
            WaveManager.Instance?.EnemyReachedCastle();

            OnEnemyReachedCastle?.Invoke(this);
            Destroy(gameObject);
        }

        private void OnPathComplete()
        {
            if (!hasReachedCastle && CastleManager.Instance != null)
            {
                pathAgent.SetDestination(CastleManager.Instance.GetCastlePosition());
            }
        }

        private void HandleDeath()
        {
            // Give reward
            GameManager.Instance?.EnemyDefeated(rewardGold, rewardGold);

            // Give exp to last attacker
            if (healthComponent.LastAttacker != null && TowerLevelManager.Instance != null)
            {
                TowerLevelManager.Instance.AddExpFromKill(healthComponent.LastAttacker, this);
            }

            OnEnemyDefeated?.Invoke(this);
            Destroy(gameObject);
        }

        private void HandleSpeedChanged(float multiplier)
        {
            if (movementComponent != null)
            {
                float baseSpeed = movementComponent.CurrentSpeed / multiplier;
                movementComponent.SetSpeed(baseSpeed * multiplier);
            }
        }

        public void TakeDamage(float damage)
        {
            TakeDamage(damage, null);
        }

        public void TakeDamage(float damage, Unit attacker)
        {
            if (hasReachedCastle) return;
            healthComponent.TakeDamage(damage, attacker);
        }

        public void ApplySlowEffect(float slowPercent, MonoBehaviour source)
        {
            statusEffectComponent.ApplySlowEffect(slowPercent, source);
        }

        public void RemoveSlowEffect(MonoBehaviour source)
        {
            statusEffectComponent.RemoveSlowEffect(source);
        }

        private void SetEnemySprite(string enemyName)
        {
            // Sprite loading logic here if needed
        }

        // Public accessors for other systems
        public float CurrentHealth => healthComponent.CurrentHealth;
        public bool IsAlive => healthComponent.IsAlive;
    }
}
