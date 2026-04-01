using System;
using UnityEngine;

namespace DefenceGame.Core.Systems.Progression
{
    /// <summary>
    /// 점수 및 진행 관리 시스템 - 단일 책임: 점수 계산과 생존 시간 관리
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }
        
        [SerializeField] private float survivalTime;
        [SerializeField] private int enemiesDefeated;
        [SerializeField] private int totalScore;
        
        public event Action<int> OnScoreChanged;
        public event Action<float> OnSurvivalTimeChanged;
        public event Action<int> OnEnemiesDefeatedChanged;
        
        public float SurvivalTime => survivalTime;
        public int EnemiesDefeated => enemiesDefeated;
        public int TotalScore => totalScore;
        
        private bool isTracking = false;
        
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
        
        private void Update()
        {
            if (isTracking)
            {
                survivalTime += Time.deltaTime;
                OnSurvivalTimeChanged?.Invoke(survivalTime);
                CalculateScore();
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
            survivalTime = 0f;
            enemiesDefeated = 0;
            totalScore = 0;
            isTracking = true;
            
            OnScoreChanged?.Invoke(totalScore);
            OnSurvivalTimeChanged?.Invoke(survivalTime);
            OnEnemiesDefeatedChanged?.Invoke(enemiesDefeated);
        }
        
        public void StopTracking()
        {
            isTracking = false;
        }
        
        public void RecordEnemyDefeated()
        {
            enemiesDefeated++;
            OnEnemiesDefeatedChanged?.Invoke(enemiesDefeated);
            CalculateScore();
        }
        
        private void CalculateScore()
        {
            // Score formula: survival time * 10 + enemies defeated * 100
            totalScore = Mathf.FloorToInt(survivalTime * 10) + (enemiesDefeated * 100);
            OnScoreChanged?.Invoke(totalScore);
        }
    }
}
