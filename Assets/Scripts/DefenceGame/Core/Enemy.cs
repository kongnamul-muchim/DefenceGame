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
        
        private bool isInitialized = false;
        private bool hasReachedCastle = false;
        
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
        
        public void Initialize(EnemyData data, float healthMultiplier, Vector3 targetPosition)
        {
            id = data.Id;
            enemyName = data.Name;
            maxHealth = data.Health * healthMultiplier;
            currentHealth = maxHealth;
            speed = data.Speed;
            rewardGold = data.RewardGold;
            
            // Set sprite
            SetEnemySprite(data.Name);
            
            // Set speed
            if (pathAgent != null)
            {
                pathAgent.speed = speed;
            }
            
            // Find path to castle
            FindPathToTarget(targetPosition);
            
            isInitialized = true;
            hasReachedCastle = false;
            
            Debug.Log($"Enemy initialized: {enemyName}, HP: {currentHealth}, Speed: {speed}");
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
        
        private void Update()
        {
            if (!isInitialized) return;
            
            // Check if reached castle bounds (3x3 area around castle center)
            if (!hasReachedCastle && GameManager.Instance != null)
            {
                if (GameManager.Instance.IsInCastleBounds(transform.position))
                {
                    ReachCastle();
                }
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
            if (hasReachedCastle) return;
            
            currentHealth -= damage;
            
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
                GameManager.Instance.EnemyDefeated(rewardGold);
            }
            
            OnEnemyDefeated?.Invoke(this);
            
            Debug.Log($"Enemy {enemyName} defeated! Reward: {rewardGold}");
            
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
    }
}
