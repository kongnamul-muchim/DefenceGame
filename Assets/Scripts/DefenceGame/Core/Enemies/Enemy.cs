using UnityEngine;
using DefenceGame.Data;

namespace DefenceGame.Core
{
    public class Enemy : MonoBehaviour
    {
        [Header("Enemy Data")]
        public int id;
        public string enemyName;
        public float maxHealth;
        public float currentHealth;
        public float speed;
        public int rewardGold;
        
        [Header("Components")]
        public SpriteRenderer spriteRenderer;
        private PathAgent pathAgent;
        
        [Header("Health Bar")]
        [Tooltip("체력바 프리팹 (World Space Canvas with Slider)")]
        public GameObject healthBarPrefab;
        public Vector3 healthBarOffset = new Vector3(0, 0.6f, 0);
        private HealthBar healthBar;
        private GameObject healthBarInstance;
        
        [Header("Random Movement")]
        public bool useRandomMovement = false;
        public float directionChangeInterval = 2f;
        public float directionRandomness = 0.5f;
        public float wallCheckDistance = 0.5f;
        public LayerMask obstacleLayer;
        
        private bool isInitialized = false;
        private bool hasReachedCastle = false;
        private Vector3 targetPosition;
        private Vector3 currentDirection;
        private float directionChangeTimer;
        
        // Slow effect (MageTower)
        private float originalSpeed;
        private float currentSlowMultiplier = 1f;
        private System.Collections.Generic.Dictionary<MonoBehaviour, float> slowSources = new System.Collections.Generic.Dictionary<MonoBehaviour, float>();
        
        // Last attacker tracking (for exp reward)
        private Unit lastAttacker;
        
        // Events
        public System.Action<Enemy> OnEnemyDefeated;
        public System.Action<Enemy> OnEnemyReachedCastle;
        
        private void Awake()
        {
            pathAgent = GetComponent<PathAgent>();
            if (pathAgent == null)
            {
                pathAgent = gameObject.AddComponent<PathAgent>();
            }
            
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
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
        
        public void Initialize(EnemyData data, float healthMultiplier, Vector3 targetPos)
        {
            id = data.Id;
            enemyName = data.Name;
            maxHealth = data.Health * healthMultiplier;
            currentHealth = maxHealth;
            speed = data.Speed;
            originalSpeed = speed; // 원래 속도 저장
            rewardGold = data.RewardGold;
            targetPosition = targetPos;
            
            // Set sprite
            SetEnemySprite(data.Name);
            
            // Set speed
            if (pathAgent != null)
            {
                pathAgent.speed = speed;
            }
            
            // Setup health bar
            SetupHealthBar();
            
            // Initialize movement
            if (useRandomMovement)
            {
                InitializeRandomMovement();
            }
            else
            {
                // Use pathfinding
                FindPathToTarget(targetPosition);
            }
            
            isInitialized = true;
            hasReachedCastle = false;
            
            Debug.Log($"Enemy initialized: {enemyName}, HP: {currentHealth}, Speed: {speed}");
        }
        
        private void SetupHealthBar()
        {
            // 이미 Enemy 프리팹에 자식으로 있는 HealthBar 찾기
            healthBar = GetComponentInChildren<HealthBar>(true);
            
            if (healthBar == null)
            {
                Debug.LogWarning($"[{enemyName}] HealthBar not found in children!");
                return;
            }
            
            // Initialize health bar
            healthBar.Initialize(maxHealth);
            Debug.Log($"[{enemyName}] HealthBar initialized with {maxHealth} HP");
        }
        
        private void InitializeRandomMovement()
        {
            // Start with direction towards castle
            currentDirection = (targetPosition - transform.position).normalized;
            directionChangeTimer = directionChangeInterval;
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            if (useRandomMovement)
            {
                UpdateRandomMovement();
            }
            
            // Check if reached castle bounds (3x3 area around castle center)
            if (!hasReachedCastle && GameManager.Instance != null)
            {
                if (GameManager.Instance.IsInCastleBounds(transform.position))
                {
                    ReachCastle();
                }
            }
        }
        
        private void UpdateRandomMovement()
        {
            // Check for wall/barrier ahead
            CheckAndAvoidObstacles();
            
            // Change direction periodically with randomness
            directionChangeTimer -= Time.deltaTime;
            if (directionChangeTimer <= 0)
            {
                ChangeDirectionWithRandomness();
                directionChangeTimer = directionChangeInterval;
            }
            
            // Move in current direction
            transform.position += currentDirection * speed * Time.deltaTime;
            
            // Flip sprite based on movement direction
            UpdateSpriteDirection(currentDirection);
        }
        
        private void CheckAndAvoidObstacles()
        {
            // Check ahead for obstacles using raycast
            RaycastHit2D hit = Physics2D.Raycast(transform.position, currentDirection, wallCheckDistance, obstacleLayer);
            
            if (hit.collider != null)
            {
                // Hit wall/barrier, change direction immediately
                Debug.Log($"{enemyName} hit {hit.collider.name}, changing direction");
                
                // Try to find a new direction that's not blocked
                Vector3 newDirection = FindClearDirection();
                currentDirection = newDirection;
            }
            
            // Also check using GridSystem
            Vector3 nextPosition = transform.position + currentDirection * wallCheckDistance;
            if (GridSystem.Instance != null)
            {
                GridSystem.Node nextNode = GridSystem.Instance.GetNodeFromWorldPosition(nextPosition);
                if (nextNode == null || !nextNode.isWalkable)
                {
                    // Next position is wall or out of bounds
                    currentDirection = FindClearDirection();
                }
            }
        }
        
        private Vector3 FindClearDirection()
        {
            // Try several random directions to find one that's clear
            for (int i = 0; i < 8; i++)
            {
                float angle = Random.Range(0f, 360f);
                Vector3 testDirection = Quaternion.Euler(0, 0, angle) * Vector3.right;
                
                Vector3 nextPos = transform.position + testDirection * wallCheckDistance;
                
                // Check with raycast
                RaycastHit2D hit = Physics2D.Raycast(transform.position, testDirection, wallCheckDistance, obstacleLayer);
                if (hit.collider != null) continue;
                
                // Check with GridSystem
                if (GridSystem.Instance != null)
                {
                    GridSystem.Node node = GridSystem.Instance.GetNodeFromWorldPosition(nextPos);
                    if (node == null || !node.isWalkable) continue;
                }
                
                // Found clear direction
                return testDirection;
            }
            
            // If no clear direction found, move towards castle
            return (targetPosition - transform.position).normalized;
        }
        
        private void ChangeDirectionWithRandomness()
        {
            // Get direction to castle
            Vector3 toCastle = (targetPosition - transform.position).normalized;
            
            // Add random offset
            float randomAngle = Random.Range(-directionRandomness * 90f, directionRandomness * 90f);
            currentDirection = Quaternion.Euler(0, 0, randomAngle) * toCastle;
        }
        
        private void UpdateSpriteDirection(Vector3 direction)
        {
            if (spriteRenderer == null) return;
            
            // Flip based on horizontal movement
            if (direction.x > 0.01f)
            {
                spriteRenderer.flipX = true;  // Face right
            }
            else if (direction.x < -0.01f)
            {
                spriteRenderer.flipX = false; // Face left
            }
        }
        
        private void SetEnemySprite(string enemyName)
        {
            // Try to load sprite from Pixel Monster Pack
            string spritePath = $"Assets/Asset/64x64 monsters/{enemyName.ToLower()}.png";
            Sprite sprite = LoadSprite(spritePath);
            
            if (sprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }
        
        private Sprite LoadSprite(string path)
        {
            // This is a simplified version. In a real project, you'd use AssetDatabase or Resources
            // For now, we'll rely on the prefab having the sprite already set
            return null;
        }
        
        private void FindPathToTarget(Vector3 targetPosition)
        {
            if (pathAgent != null && Pathfinder.Instance != null)
            {
                pathAgent.SetDestination(targetPosition);
            }
        }
        
        private void ReachCastle()
        {
            if (hasReachedCastle) return;
            
            hasReachedCastle = true;
            
            // Damage castle
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DamageCastle(1);
            }
            
            // Notify wave manager
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.EnemyReachedCastle();
            }
            
            OnEnemyReachedCastle?.Invoke(this);
            
            Debug.Log($"Enemy {enemyName} reached castle!");
            
            // Destroy enemy
            Destroy(gameObject);
        }
        
        private void OnPathComplete()
        {
            // If path is complete but haven't reached castle bounds yet
            // Keep moving towards castle
            if (!hasReachedCastle && GameManager.Instance != null && GameManager.Instance.castleTransform != null)
            {
                FindPathToTarget(GameManager.Instance.castleTransform.position);
            }
        }
        
        public void TakeDamage(float damage)
        {
            TakeDamage(damage, null);
        }
        
        public void TakeDamage(float damage, Unit attacker)
        {
            if (hasReachedCastle) return;
            
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
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        private void Die()
        {
            // Give reward
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EnemyDefeated(rewardGold, rewardGold);
            }
            
            // Give exp to last attacker
            if (lastAttacker != null && TowerLevelManager.Instance != null)
            {
                TowerLevelManager.Instance.AddExpFromKill(lastAttacker, this);
            }
            
            OnEnemyDefeated?.Invoke(this);
            
            // Destroy enemy
            Destroy(gameObject);
        }
        
        private void OnDestroy()
        {
            if (WaveManager.Instance != null && !hasReachedCastle)
            {
                WaveManager.Instance.EnemyReachedCastle();
            }
        }
        
        // Slow effect methods (MageTower)
        public void ApplySlowEffect(float slowPercent, MonoBehaviour source)
        {
            if (source == null) 
            {
                // Debug.LogWarning("[Enemy] ApplySlowEffect called with null source");
                return;
            }
            
            // 이미 이 소스에서 느려짐이 적용되어 있으면 리턴
            if (slowSources.ContainsKey(source)) 
            {
                // Debug.Log($"[{enemyName}] Already slowed by {source.GetType().Name}");
                return;
            }
            
            slowSources.Add(source, slowPercent);
            UpdateSlowMultiplier();
            
            // 시각적 효과 (파란색으로 변경)
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.3f, 0.3f, 1f, 1f); // 더 진한 파란색 틴트
                // Debug.Log($"[{enemyName}] Slowed by {slowPercent * 100}% - Color changed to BLUE");
            }
            else
            {
                // Debug.LogWarning($"[{enemyName}] SpriteRenderer is null - cannot change color");
            }
        }
        
        public void RemoveSlowEffect(MonoBehaviour source)
        {
            if (source == null) return;
            
            slowSources.Remove(source);
            UpdateSlowMultiplier();
            
            // 모든 느려짐이 제거되면 원래 색상으로 복원
            if (slowSources.Count == 0 && spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
                // Debug.Log($"[{enemyName}] Slow removed - Color restored to WHITE");
            }
        }
        
        private void UpdateSlowMultiplier()
        {
            // 가장 강한 느려짐 효과 적용 (중첩하지 않음)
            float maxSlow = 0f;
            foreach (var kvp in slowSources)
            {
                maxSlow = Mathf.Max(maxSlow, kvp.Value);
            }
            
            // 100% 둔화 테스트를 위해 최소값 제한 제거 (0.1f -> 0f)
            currentSlowMultiplier = Mathf.Max(0f, 1f - maxSlow);
            
            // 속도 업데이트
            float oldSpeed = speed;
            speed = originalSpeed * currentSlowMultiplier;
            if (pathAgent != null)
            {
                pathAgent.speed = speed;
            }
            
            // 디버그 로그 (활성화됨)
            if (maxSlow > 0)
            {
                Debug.Log($"[{enemyName}] Speed: {oldSpeed} -> {speed} (slow: {maxSlow * 100}%, multiplier: {currentSlowMultiplier})");
            }
        }
    }
}
