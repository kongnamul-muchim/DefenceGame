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
        public List<EnemyPrefabMapping> enemyPrefabs = new List<EnemyPrefabMapping>();
        private Dictionary<int, GameObject> enemyPrefabDict;
        
        [Header("Wave Data")]
        public GameDataSO gameData;
        
        [Header("Current State")]
        [SerializeField] private int currentWave = 0;
        [SerializeField] private bool isWaveActive = false;
        [SerializeField] private int enemiesRemainingInWave = 0;
        [SerializeField] private int totalEnemiesSpawned = 0;
        [SerializeField] private Enemy currentBoss = null; // 현재 보스 추적
        [SerializeField] private bool isBossWave = false; // 보스 웨이브 여부
        
        [Header("Infinite Wave Settings")]
        [SerializeField] private int maxWaveNumber = 6;
        [SerializeField] private float healthIncreasePerWave = 0.1f;
        [SerializeField] private float eliteSpawnChance = 0.1f;
        [SerializeField] private float eliteHealthMultiplier = 2.0f;
        [SerializeField] private int eliteGoldMultiplier = 3;
        [SerializeField] private float eliteSizeMultiplier = 1.2f;
        
        private List<WaveData> waveDataList;
        private Dictionary<int, List<WaveData>> wavesByNumber;
        private Coroutine currentWaveCoroutine;
        
        public System.Action<int> OnWaveStarted;
        public System.Action<int> OnWaveCompleted;
        public System.Action OnAllWavesCompleted;
        
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
            
            if (wavesByNumber != null && wavesByNumber.Count > 0)
            {
                StartGame();
            }
        }
        
        private void InitializeWaves()
        {
            if (gameData == null)
            {
                return;
            }
            
            waveDataList = gameData.Waves;
            
            if (waveDataList == null || waveDataList.Count == 0)
            {
                return;
            }
            
            wavesByNumber = new Dictionary<int, List<WaveData>>();
            foreach (var wave in waveDataList)
            {
                if (!wavesByNumber.ContainsKey(wave.WaveNumber))
                {
                    wavesByNumber[wave.WaveNumber] = new List<WaveData>();
                }
                wavesByNumber[wave.WaveNumber].Add(wave);
            }
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
                return;
            }
            
            currentWave++;
            
            int waveDataIndex = ((currentWave - 1) % maxWaveNumber) + 1;
            
            if (currentWaveCoroutine != null)
            {
                StopCoroutine(currentWaveCoroutine);
            }
            
            currentWaveCoroutine = StartCoroutine(SpawnWave(currentWave, waveDataIndex));
        }
        
        private IEnumerator SpawnWave(int waveNumber, int waveDataIndex)
        {
            isWaveActive = true;
            isBossWave = (waveDataIndex == 6); // 6웨이브는 보스 웨이브
            currentBoss = null;
            OnWaveStarted?.Invoke(waveNumber);
            
            List<WaveData> waveData = wavesByNumber[waveDataIndex];
            enemiesRemainingInWave = 0;
            
            foreach (var data in waveData)
            {
                enemiesRemainingInWave += data.Count;
            }
            
            foreach (var data in waveData)
            {
                EnemyData enemyData = GetEnemyData(data.EnemyId);
                if (enemyData == null)
                {
                    continue;
                }
                
                for (int i = 0; i < data.Count; i++)
                {
                    if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                    {
                        yield break;
                    }
                    
                    float totalHealthMultiplier = data.HealthMultiplier * (1f + (waveNumber - 1) * healthIncreasePerWave);
                    
                    bool isElite = waveNumber > maxWaveNumber && Random.value < eliteSpawnChance;
                    
                    Enemy spawnedEnemy = SpawnEnemy(enemyData, totalHealthMultiplier, isElite);
                    totalEnemiesSpawned++;
                    
                    // 보스 추적
                    if (isBossWave && spawnedEnemy != null)
                    {
                        currentBoss = spawnedEnemy;
                    }
                    
                    yield return new WaitForSeconds(data.SpawnInterval);
                }
            }
            
            // 보스 웨이브인 경우 보스가 죽을 때까지 대기
            if (isBossWave && currentBoss != null)
            {
                while (currentBoss != null && currentBoss.CurrentHealth > 0)
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }
            
            isWaveActive = false;
            isBossWave = false;
            currentBoss = null;
            OnWaveCompleted?.Invoke(waveNumber);
            
            GiveWaveClearGold(waveNumber);
            
            yield return new WaitForSeconds(3f);
            
            if (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
            {
                StartNextWave();
            }
        }
        
        private Enemy SpawnEnemy(EnemyData enemyData, float healthMultiplier, bool isElite = false)
        {
            GameObject prefabToSpawn = GetEnemyPrefab(enemyData.Id);
            if (prefabToSpawn == null)
            {
                return null;
            }
            
            if (enemySpawnPoint == null)
            {
                return null;
            }
            
            Vector3 spawnPosition = GetNonOverlappingSpawnPosition();
            GameObject enemy = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                Vector3 randomTargetPos = GetRandomCastleTargetPosition();
                
                if (isElite)
                {
                    healthMultiplier *= eliteHealthMultiplier;
                    
                    enemy.transform.localScale *= eliteSizeMultiplier;
                    
                    SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.color = new Color(0.6f, 0.2f, 0.8f, 1f);
                    }
                    
                    enemyData = CreateEliteEnemyData(enemyData);
                }
                
                enemyComponent.Initialize(enemyData, healthMultiplier, randomTargetPos);
                
                if (isElite)
                {
                    enemy.name = $"Elite_{enemyData.Name}";
                }
                
                return enemyComponent;
            }
            
            return null;
        }
        
        private EnemyData CreateEliteEnemyData(EnemyData baseData)
        {
            EnemyData eliteData = new EnemyData
            {
                Id = baseData.Id,
                Name = $"Elite_{baseData.Name}",
                Health = baseData.Health,
                Speed = baseData.Speed,
                RewardGold = baseData.RewardGold * eliteGoldMultiplier
            };
            return eliteData;
        }
        
        private void GiveWaveClearGold(int waveNumber)
        {
            int goldReward = 0;
            
            if (waveNumber <= 6)
            {
                goldReward = 50 + (waveNumber - 1) * 10;
            }
            else
            {
                goldReward = 100 + (waveNumber - 6) * 5;
            }
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddGold(goldReward);
            }
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
                if (!enemyPrefabDict.ContainsKey(mapping.enemyId))
                {
                    enemyPrefabDict[mapping.enemyId] = mapping.prefab;
                }
            }
        }
        
        private EnemyData GetEnemyData(int enemyId)
        {
            if (gameData == null) return null;
            
            foreach (var enemy in gameData.Enemies)
            {
                if (enemy.Id == enemyId)
                {
                    return enemy;
                }
            }
            
            return null;
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
        
        public void EnemyReachedCastle()
        {
            enemiesRemainingInWave--;
        }
        
        public void EnemyDefeated()
        {
            enemiesRemainingInWave--;
        }
    }
}
