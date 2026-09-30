using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DefenceGame.Data;
using DefenceGame.Core.Systems.Spawning;
using DefenceGame.Core.Systems.Economy;

namespace DefenceGame.Core.Managers
{
    /// <summary>
    /// 웨이브 관리자 - 단일 책임: 웨이브 타이밍 및 흐름 관리
    /// 실제 스폰은 EnemySpawner에 위임
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("Wave Data")]
        public GameDataSO gameData;

        [Header("Wave Settings")]
        [SerializeField] private int maxWaveNumber = 6;
        [SerializeField] private float healthIncreasePerWave = 0.1f;
        [SerializeField] private float eliteSpawnChance = 0.1f;

        [Header("Current State")]
        [SerializeField] private int currentWave = 0;
        [SerializeField] private bool isWaveActive = false;
        [SerializeField] private int enemiesRemainingInWave = 0;
        [SerializeField] private Enemy currentBoss = null;
        [SerializeField] private bool isBossWave = false;

        private List<WaveData> waveDataList;
        private Dictionary<int, List<WaveData>> wavesByNumber;
        private Coroutine currentWaveCoroutine;

        public System.Action<int> OnWaveStarted;
        public System.Action<int> OnWaveCompleted;
        public System.Action OnAllWavesCompleted;

        public int CurrentWave => currentWave;
        public bool IsWaveActive => isWaveActive;
        public int EnemiesRemaining => enemiesRemainingInWave;
        public int TotalEnemiesSpawned => EnemySpawner.Instance?.TotalEnemiesSpawned ?? 0;

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
            // Auto-assign gameData if not set in Inspector
            if (gameData == null)
            {
                gameData = GameDataSO.Instance;
                if (gameData == null)
                {
                    Debug.LogError("[WaveManager] GameDataSO not found! Please assign in Inspector or ensure GameDataSO exists.");
                    return;
                }
            }

            InitializeWaves();

            if (wavesByNumber != null && wavesByNumber.Count > 0)
            {
                StartGame();
            }
        }

        private void InitializeWaves()
        {
            if (gameData == null) return;

            waveDataList = gameData.Waves;
            if (waveDataList == null || waveDataList.Count == 0) return;

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
            EnemySpawner.Instance?.Initialize();
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

            currentWaveCoroutine = StartCoroutine(RunWave(currentWave, waveDataIndex));
        }

        private IEnumerator RunWave(int waveNumber, int waveDataIndex)
        {
            isWaveActive = true;
            isBossWave = (waveDataIndex == 6);
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
                yield return SpawnEnemyGroup(data, waveNumber);
            }

            // Wait for boss death if boss wave
            if (isBossWave && currentBoss != null)
            {
                yield return WaitForBossDeath();
            }

            CompleteWave(waveNumber);
        }

        private IEnumerator SpawnEnemyGroup(WaveData data, int waveNumber)
        {
            EnemyData enemyData = GetEnemyData(data.EnemyId);
            if (enemyData == null) yield break;

            for (int i = 0; i < data.Count; i++)
            {
                if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                {
                    yield break;
                }

                float totalHealthMultiplier = data.HealthMultiplier * (1f + (waveNumber - 1) * healthIncreasePerWave);
                bool isElite = waveNumber > maxWaveNumber && Random.value < eliteSpawnChance;

                Enemy spawnedEnemy = EnemySpawner.Instance?.SpawnEnemy(enemyData, totalHealthMultiplier, isElite);

                if (isBossWave && spawnedEnemy != null)
                {
                    currentBoss = spawnedEnemy;
                }

                yield return new WaitForSeconds(data.SpawnInterval);
            }
        }

        private IEnumerator WaitForBossDeath()
        {
            while (currentBoss != null && currentBoss.CurrentHealth > 0)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void CompleteWave(int waveNumber)
        {
            isWaveActive = false;
            isBossWave = false;
            currentBoss = null;
            OnWaveCompleted?.Invoke(waveNumber);

            GiveWaveClearGold(waveNumber);

            StartCoroutine(StartNextWaveDelayed());
        }

        private IEnumerator StartNextWaveDelayed()
        {
            yield return new WaitForSeconds(3f);

            if (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
            {
                StartNextWave();
            }
        }

        private void GiveWaveClearGold(int waveNumber)
        {
            int goldReward = waveNumber <= 6
                ? 50 + (waveNumber - 1) * 10
                : 100 + (waveNumber - 6) * 5;

            GoldManager.Instance?.AddGold(goldReward);
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
