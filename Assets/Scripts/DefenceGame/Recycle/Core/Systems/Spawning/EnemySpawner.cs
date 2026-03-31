using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recycle.Core.Systems.Spawning
{
    /// <summary>
    /// 적 스폰 시스템 - 단일 책임: 적의 생성 및 초기화
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance { get; private set; }
        
        [Header("Spawn Settings")]
        public Transform enemySpawnPoint;
        public Transform castleTarget;
        
        [Header("Enemy Prefabs")]
        public List<EnemyPrefabMapping> enemyPrefabs = new List<EnemyPrefabMapping>();
        
        [Header("Elite Settings")]
        [SerializeField] private float eliteHealthMultiplier = 2.0f;
        [SerializeField] private float eliteSizeMultiplier = 1.2f;
        
        private Dictionary<int, GameObject> enemyPrefabDict;
        private int totalEnemiesSpawned = 0;
        
        public int TotalEnemiesSpawned => totalEnemiesSpawned;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        public void Initialize()
        {
            totalEnemiesSpawned = 0;
            InitializeEnemyPrefabDict();
        }
        
        public void SpawnEnemy(GameObject prefab, float healthMultiplier, bool isElite = false)
        {
            if (prefab == null || enemySpawnPoint == null)
            {
                return;
            }
            
            Vector3 spawnPosition = GetNonOverlappingSpawnPosition();
            GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
            
            if (isElite)
            {
                healthMultiplier *= eliteHealthMultiplier;
                enemy.transform.localScale *= eliteSizeMultiplier;
                
                SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color(0.6f, 0.2f, 0.8f, 1f);
                }
            }
            
            totalEnemiesSpawned++;
        }
        
        private GameObject GetEnemyPrefab(int enemyId)
        {
            if (enemyPrefabDict == null)
            {
                InitializeEnemyPrefabDict();
            }
            
            return enemyPrefabDict.TryGetValue(enemyId, out GameObject prefab) ? prefab : null;
        }
        
        private void InitializeEnemyPrefabDict()
        {
            enemyPrefabDict = new Dictionary<int, GameObject>();
            foreach (var mapping in enemyPrefabs)
            {
                if (!enemyPrefabDict.ContainsKey(mapping.enemyId))
                {
                    enemyPrefabDict[mapping.enemyId] = mapping.prefab;
                }
            }
        }
        
        private Vector3 GetNonOverlappingSpawnPosition()
        {
            Vector3 basePosition = enemySpawnPoint != null ? enemySpawnPoint.position : Vector3.zero;
            float randomOffset = Random.Range(-1f, 1f);
            return basePosition + new Vector3(randomOffset, 0, 0);
        }
        
        private Vector3 GetRandomCastleTargetPosition()
        {
            if (castleTarget == null) return Vector3.zero;
            
            Vector3 castlePos = castleTarget.position;
            float randomX = Random.Range(-1.5f, 1.5f);
            float randomY = Random.Range(-1.5f, 1.5f);
            return castlePos + new Vector3(randomX, randomY, 0);
        }
    }
    
    [System.Serializable]
    public class EnemyPrefabMapping
    {
        public int enemyId;
        public GameObject prefab;
    }
}
