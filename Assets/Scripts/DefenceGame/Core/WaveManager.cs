using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DefenceGame.Data;

namespace DefenceGame.Core
{
    [System.Serializable]
    public class EnemyPrefabMapping
    {
        public int enemyId;
        public GameObject prefab;
    }
    
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }
        
        [Header("Spawn Settings")]
        public Transform enemySpawnPoint;
        public Transform castleTarget;
        
        [Header("Enemy Prefabs")]
        // Map enemy ID to prefab (set in Inspector)
        public List<EnemyPrefabMapping> enemyPrefabs = new List<EnemyPrefabMapping>();
        private Dictionary<int, GameObject> enemyPrefabDict;
        
        [Header("Wave Data")]
        public GameDataSO gameData;
        
        [Header("Current State")]
        [SerializeField] private int currentWave = 0;
        [SerializeField] private bool isWaveActive = false;
        [SerializeField] private int enemiesRemainingInWave = 0;
        [SerializeField] private int totalEnemiesSpawned = 0;
        
        private List<WaveData> waveDataList;
        private Dictionary<int, List<WaveData>> wavesByNumber;
        private Coroutine currentWaveCoroutine;
        
        // Events
        public System.Action<int> OnWaveStarted;
        public System.Action<int> OnWaveCompleted;
        public System.Action OnAllWavesCompleted;
        
        // Properties
        public int CurrentWave => currentWave;
        public bool IsWaveActive => isWaveActive;
        public int EnemiesRemaining => enemiesRemainingInWave;
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
        
        private void Start()
        {
            InitializeWaves();
            
            // Auto-start game after initialization
            if (wavesByNumber != null && wavesByNumber.Count > 0)
            {
                StartGame();
            }
        }
        
        private void InitializeWaves()
        {
            if (gameData == null)
            {
                Debug.LogError("GameDataSO not assigned!");
                return;
            }
            
            waveDataList = gameData.Waves;
            
            if (waveDataList == null || waveDataList.Count == 0)
            {
                Debug.LogError("No wave data found in GameDataSO!");
                return;
            }
            
            // Group waves by wave number
            wavesByNumber = new Dictionary<int, List<WaveData>>();
            foreach (var wave in waveDataList)
            {
                if (!wavesByNumber.ContainsKey(wave.WaveNumber))
                {
                    wavesByNumber[wave.WaveNumber] = new List<WaveData>();
                }
                wavesByNumber[wave.WaveNumber].Add(wave);
            }
            
            Debug.Log($"WaveManager initialized with {waveDataList.Count} wave entries");
        }
        
        public void StartGame()
        {
            currentWave = 0;
            totalEnemiesSpawned = 0;
            StartNextWave();
        }
        
        public void StartNextWave()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                Debug.Log("Cannot start wave: Game is over");
                return;
            }
            
            currentWave++;
            
            if (!wavesByNumber.ContainsKey(currentWave))
            {
                Debug.Log($"Wave {currentWave} not found. All waves completed!");
                OnAllWavesCompleted?.Invoke();
                return;
            }
            
            if (currentWaveCoroutine != null)
            {
                StopCoroutine(currentWaveCoroutine);
            }
            
            currentWaveCoroutine = StartCoroutine(SpawnWave(currentWave));
        }
        
        private IEnumerator SpawnWave(int waveNumber)
        {
            isWaveActive = true;
            OnWaveStarted?.Invoke(waveNumber);
            
            List<WaveData> waveData = wavesByNumber[waveNumber];
            enemiesRemainingInWave = 0;
            
            // Calculate total enemies in this wave
            foreach (var data in waveData)
            {
                enemiesRemainingInWave += data.Count;
            }
            
            Debug.Log($"Wave {waveNumber} started! Enemies: {enemiesRemainingInWave}");
            
            // Spawn enemies for each entry in the wave
            foreach (var data in waveData)
            {
                EnemyData enemyData = GetEnemyData(data.EnemyId);
                if (enemyData == null)
                {
                    Debug.LogError($"Enemy data not found for ID: {data.EnemyId}");
                    continue;
                }
                
                for (int i = 0; i < data.Count; i++)
                {
                    if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                    {
                        yield break;
                    }
                    
                    SpawnEnemy(enemyData, data.HealthMultiplier);
                    totalEnemiesSpawned++;
                    
                    yield return new WaitForSeconds(data.SpawnInterval);
                }
            }
            
            isWaveActive = false;
            OnWaveCompleted?.Invoke(waveNumber);
            
            Debug.Log($"Wave {waveNumber} completed!");
            
            // Auto-start next wave after a delay
            yield return new WaitForSeconds(3f);
            
            if (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
            {
                StartNextWave();
            }
        }
        
        private void SpawnEnemy(EnemyData enemyData, float healthMultiplier)
        {
            // Get prefab for this enemy ID
            GameObject prefabToSpawn = GetEnemyPrefab(enemyData.Id);
            if (prefabToSpawn == null)
            {
                Debug.LogError($"No prefab found for enemy ID: {enemyData.Id} ({enemyData.Name})");
                return;
            }
            
            if (enemySpawnPoint == null)
            {
                Debug.LogError("Enemy spawn point not assigned!");
                return;
            }
            
            Vector3 spawnPosition = GetNonOverlappingSpawnPosition();
            GameObject enemy = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            
            // Initialize enemy with random target position around castle
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                Vector3 randomTargetPos = GetRandomCastleTargetPosition();
                enemyComponent.Initialize(enemyData, healthMultiplier, randomTargetPos);
            }
            
            Debug.Log($"Spawned enemy: {enemyData.Name} (ID: {enemyData.Id}) at {spawnPosition}");
        }
        
        private GameObject GetEnemyPrefab(int enemyId)
        {
            if (enemyPrefabDict == null)
            {
                InitializeEnemyPrefabDict();
            }
            
            if (enemyPrefabDict.TryGetValue(enemyId, out GameObject prefab))
            {
                return prefab;
            }
            
            return null;
        }
        
        private void InitializeEnemyPrefabDict()
        {
            enemyPrefabDict = new Dictionary<int, GameObject>();
            foreach (var mapping in enemyPrefabs)
            {
                if (mapping.prefab != null)
                {
                    enemyPrefabDict[mapping.enemyId] = mapping.prefab;
                }
            }
        }
        
        private Vector3 GetNonOverlappingSpawnPosition()
        {
            Vector3 basePosition = enemySpawnPoint.position;
            float radius = 0.5f;
            
            // Try to find a non-overlapping position
            for (int i = 0; i < 10; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * radius;
                Vector3 spawnPos = basePosition + new Vector3(randomOffset.x, randomOffset.y, 0);
                
                // Check if position is clear
                Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPos, 0.3f);
                if (colliders.Length == 0)
                {
                    return spawnPos;
                }
            }
            
            // If all attempts failed, return base position with small offset
            return basePosition + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0);
        }
        
        private EnemyData GetEnemyData(int enemyId)
        {
            if (gameData == null || gameData.Enemies == null)
                return null;
            
            return gameData.Enemies.Find(e => e.Id == enemyId);
        }
        
        public void EnemyReachedCastle()
        {
            enemiesRemainingInWave--;
            
            if (enemiesRemainingInWave <= 0 && !isWaveActive)
            {
                Debug.Log($"All enemies in wave {currentWave} defeated or reached castle");
            }
        }
        
        public void StopWaves()
        {
            if (currentWaveCoroutine != null)
            {
                StopCoroutine(currentWaveCoroutine);
                currentWaveCoroutine = null;
            }
            isWaveActive = false;
        }
        
        public void ResetWaves()
        {
            StopWaves();
            currentWave = 0;
            totalEnemiesSpawned = 0;
            enemiesRemainingInWave = 0;
        }
        
        private Vector3 GetRandomCastleTargetPosition()
        {
            if (castleTarget == null)
            {
                return Vector3.zero;
            }
            
            // Random offset around castle (within 2 units radius)
            Vector2 randomOffset = Random.insideUnitCircle * 2f;
            return castleTarget.position + new Vector3(randomOffset.x, randomOffset.y, 0);
        }
    }
}
