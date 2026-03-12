using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DefenceGame.Data;

namespace DefenceGame.Core
{
    public class GachaManager : MonoBehaviour
    {
        public static GachaManager Instance { get; private set; }
        
        [Header("Settings")]
        public int baseGachaCost = 50;
        public int gachaCostIncrease = 10;
        public int freeGachaCount = 4;
        public GameDataSO gameData;
        
        private int gachaCount = 0;
        
        [Header("Input")]
        public KeyCode gachaKey = KeyCode.Space;
        public bool enableKeyboardInput = true;
        
        // Events
        public event Action<TowerData> OnGachaSuccess;
        public event Action<string> OnGachaFailed;
        public event Action<int> OnGoldChanged;
        
        private Dictionary<GradeType, List<TowerData>> towersByGrade;
        private Dictionary<GradeType, float> gradeProbabilities;
        private System.Random random;
        private float lastGachaTime;
        public float gachaCooldown = 0.5f;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                random = new System.Random();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            InitializeGachaData();
        }
        
        private void Update()
        {
            if (enableKeyboardInput && Input.GetKeyDown(gachaKey))
            {
                TryGacha();
            }
        }
        
        private void TryGacha()
        {
            // Cooldown check
            if (Time.time - lastGachaTime < gachaCooldown)
            {
                return;
            }
            
            TowerData result = PerformGacha();
            if (result != null)
            {
                lastGachaTime = Time.time;
                // Auto-place the unit
                UnitPlacementManager.Instance?.PlaceUnit(result);
            }
        }
        
        private void InitializeGachaData()
        {
            if (gameData == null)
            {
                Debug.LogError("GameDataSO not assigned!");
                return;
            }
            
            // Group towers by grade
            towersByGrade = new Dictionary<GradeType, List<TowerData>>();
            foreach (var tower in gameData.Towers)
            {
                if (!towersByGrade.ContainsKey(tower.Grade))
                {
                    towersByGrade[tower.Grade] = new List<TowerData>();
                }
                towersByGrade[tower.Grade].Add(tower);
            }
            
            // Build probability table from GachaData
            gradeProbabilities = new Dictionary<GradeType, float>();
            foreach (var gacha in gameData.GachaProbabilities)
            {
                gradeProbabilities[gacha.Grade] = gacha.Probability;
            }
            
            // Normalize probabilities
            float totalProbability = gradeProbabilities.Values.Sum();
            if (totalProbability > 0)
            {
                foreach (var grade in gradeProbabilities.Keys.ToList())
                {
                    gradeProbabilities[grade] /= totalProbability;
                }
            }
            
            Debug.Log($"GachaManager initialized with {gameData.Towers.Count} towers");
        }
        
        /// <summary>
        /// Get current gacha cost based on summon count
        /// </summary>
        public int GetCurrentGachaCost()
        {
            if (gachaCount < freeGachaCount)
            {
                return baseGachaCost;
            }
            else
            {
                int extraPulls = gachaCount - freeGachaCount + 1;
                return baseGachaCost + (extraPulls * gachaCostIncrease);
            }
        }
        
        /// <summary>
        /// Perform gacha and return a random tower
        /// </summary>
        public TowerData PerformGacha()
        {
            int currentCost = GetCurrentGachaCost();
            
            // Check gold
            if (GameManager.Instance == null || GameManager.Instance.CurrentGold < currentCost)
            {
                int currentGold = GameManager.Instance?.CurrentGold ?? 0;
                string message = $"골드가 부족합니다! (필요: {currentCost}, 보유: {currentGold})";
                OnGachaFailed?.Invoke(message);
                return null;
            }
            
            // Deduct gold
            if (!GameManager.Instance.SpendGold(currentCost))
            {
                string message = "골드 차감에 실패했습니다.";
                OnGachaFailed?.Invoke(message);
                return null;
            }
            
            // Roll for grade
            GradeType rolledGrade = RollGrade();
            
            // Get random tower from that grade
            TowerData result = GetRandomTower(rolledGrade);
            
            if (result != null)
            {
                gachaCount++;
                OnGachaSuccess?.Invoke(result);
            }
            else
            {
                string message = "가챠 실패: 유닛을 찾을 수 없습니다.";
                OnGachaFailed?.Invoke(message);
            }
            
            return result;
        }
        
        private GradeType RollGrade()
        {
            float roll = UnityEngine.Random.Range(0f, 1f);
            float cumulative = 0f;
            
            // Sort grades by probability (highest first for better UX)
            var sortedGrades = gradeProbabilities.OrderByDescending(x => x.Value);
            
            foreach (var kvp in sortedGrades)
            {
                cumulative += kvp.Value;
                if (roll <= cumulative)
                {
                    return kvp.Key;
                }
            }
            
            // Fallback to lowest grade
            return GradeType.Common;
        }
        
        private TowerData GetRandomTower(GradeType grade)
        {
            if (towersByGrade == null || !towersByGrade.ContainsKey(grade))
            {
                return null;
            }
            
            List<TowerData> towers = towersByGrade[grade];
            if (towers.Count == 0)
            {
                return null;
            }
            
            int randomIndex = UnityEngine.Random.Range(0, towers.Count);
            return towers[randomIndex];
        }
        
        /// <summary>
        /// Get probability for a specific grade
        /// </summary>
        public float GetGradeProbability(GradeType grade)
        {
            if (gradeProbabilities.ContainsKey(grade))
            {
                return gradeProbabilities[grade];
            }
            return 0f;
        }
        
        /// <summary>
        /// Get all towers of a specific grade
        /// </summary>
        public List<TowerData> GetTowersByGrade(GradeType grade)
        {
            if (towersByGrade.ContainsKey(grade))
            {
                return towersByGrade[grade];
            }
            return new List<TowerData>();
        }
    }
}