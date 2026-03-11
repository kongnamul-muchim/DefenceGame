using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DefenceGame.Data;

namespace DefenceGame.Core
{
    /// <summary>
    /// 특수능력 등급 기반 테스트용 매니저
    /// 레벨 7 달성 시 해당 타워의 모든 등급을 랜덤으로 소환
    /// </summary>
    public class GradeAbilityTestManager : MonoBehaviour
    {
        public static GradeAbilityTestManager Instance { get; private set; }
        
        [Header("Test Settings")]
        [Tooltip("테스트 모드 활성화")]
        public bool enableTestMode = true;
        
        [Tooltip("레벨 7 달성 시 자동 소환")]
        public bool autoSpawnOnLevel7 = true;
        
        [Tooltip("랜덤 소환 간격 (초)")]
        public float spawnInterval = 0.5f;
        
        [Tooltip("소환 위치 오프셋 (격자 단위)")]
        public Vector3 spawnOffset = new Vector3(2f, 0f, 0f);
        
        [Header("등급별 색상 디버깅")]
        public bool showGradeColors = true;
        
        // 레벨 7에 도달한 타워 추적
        private HashSet<string> level7ReachedTowers = new HashSet<string>();
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            if (enableTestMode && TowerLevelManager.Instance != null)
            {
                TowerLevelManager.Instance.OnTowerLevelUp += OnTowerLevelUp;
                Debug.Log("[GradeAbilityTest] 테스트 모드 활성화됨 - 레벨 7 달성 시 자동 소환");
            }
        }
        
        private void OnDestroy()
        {
            if (TowerLevelManager.Instance != null)
            {
                TowerLevelManager.Instance.OnTowerLevelUp -= OnTowerLevelUp;
            }
        }
        
        /// <summary>
        /// 타워 레벨업 이벤트 핸들러
        /// </summary>
        private void OnTowerLevelUp(string towerType, int newLevel)
        {
            if (!enableTestMode || !autoSpawnOnLevel7) return;
            
            if (newLevel == 7 && !level7ReachedTowers.Contains(towerType))
            {
                level7ReachedTowers.Add(towerType);
                Debug.Log($"[GradeAbilityTest] {towerType} 레벨 7 달성! 모든 등급 소환 시작");
                StartCoroutine(SpawnAllGrades(towerType));
            }
        }
        
        /// <summary>
        /// 모든 등급의 타워를 순차적으로 소환
        /// </summary>
        private IEnumerator SpawnAllGrades(string towerType)
        {
            // 모든 등급을 리스트에 넣고 셔플
            List<GradeType> allGrades = new List<GradeType>
            {
                GradeType.Common,
                GradeType.Uncommon,
                GradeType.Rare,
                GradeType.Epic,
                GradeType.Legendary
            };
            
            // Fisher-Yates 셔플
            for (int i = allGrades.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                GradeType temp = allGrades[i];
                allGrades[i] = allGrades[j];
                allGrades[j] = temp;
            }
            
            // 각 등급별 소환
            foreach (GradeType grade in allGrades)
            {
                SpawnTowerOfGrade(towerType, grade);
                yield return new WaitForSeconds(spawnInterval);
            }
            
            Debug.Log($"[GradeAbilityTest] {towerType} 모든 등급 소환 완료!");
        }
        
        /// <summary>
        /// 특정 등급의 타워 소환
        /// </summary>
        private void SpawnTowerOfGrade(string towerType, GradeType grade)
        {
            if (UnitPlacementManager.Instance == null)
            {
                Debug.LogError("[GradeAbilityTest] UnitPlacementManager.Instance is null!");
                return;
            }
            
            // 타워 ID 가져오기 (TowerDataSO에서)
            int towerId = GetTowerIdByType(towerType);
            if (towerId == -1)
            {
                Debug.LogError($"[GradeAbilityTest] Unknown tower type: {towerType}");
                return;
            }
            
            // TowerData 생성
            TowerData towerData = CreateTestTowerData(towerId, towerType, grade);
            
            // 유닛 소환
            UnitPlacementManager.Instance.PlaceUnit(towerData);
            
            Debug.Log($"[GradeAbilityTest] 소환됨: {towerType} ({grade}) - 배율: {GradeMultiplier.GetMultiplier(grade):F2}");
        }
        
        /// <summary>
        /// 타워 타입에 해당하는 ID 반환
        /// </summary>
        private int GetTowerIdByType(string towerType)
        {
            // UnitPlacementManager의 GetTowerPrefab 로직에 맞춰 ID 설정
            // ((towerId - 1) % 4) + 1로 변환됨
            // 1,5,9,13,17 → 1 (Archer)
            // 2,6,10,14,18 → 2 (Wizard)
            // 3,7,11,15,19 → 3 (WizardTower)
            // 4,8,12,16,20 → 4 (Laser)
            switch (towerType)
            {
                case "Archer":
                    return 1;  // 1,5,9,13,17 → 1
                case "Wizard":
                    return 2;  // 2,6,10,14,18 → 2
                case "WizardTower":
                    return 3;  // 3,7,11,15,19 → 3
                case "Laser":
                    return 4;  // 4,8,12,16,20 → 4
                default:
                    return 1;
            }
        }
        
        /// <summary>
        /// 테스트용 TowerData 생성
        /// </summary>
        private TowerData CreateTestTowerData(int id, string towerType, GradeType grade)
        {
            // 기본 데이터 설정
            float baseAttackPower = 10f;
            float baseAttackSpeed = 1f;
            float baseRange = 5f;
            
            // 타워별 기본값 설정
            switch (towerType)
            {
                case "Archer":
                    baseAttackPower = 12f;
                    baseAttackSpeed = 1.2f;
                    baseRange = 6f;
                    break;
                case "Wizard":
                    baseAttackPower = 15f;
                    baseAttackSpeed = 0.8f;
                    baseRange = 5f;
                    break;
                case "WizardTower":
                    baseAttackPower = 10f;
                    baseAttackSpeed = 1f;
                    baseRange = 7f;
                    break;
                case "Laser":
                    baseAttackPower = 18f;
                    baseAttackSpeed = 0.6f;
                    baseRange = 8f;
                    break;
            }
            
            TowerData data = new TowerData();
            data.Id = id;
            data.Name = $"{towerType}_{grade}";
            data.AttackPower = baseAttackPower;
            data.AttackSpeed = baseAttackSpeed;
            data.Range = baseRange;
            data.Grade = grade;
            
            return data;
        }
        
        /// <summary>
        /// 수동으로 특정 타워의 모든 등급 소환 (디버그용)
        /// </summary>
        [ContextMenu("Test Spawn Archer")]
        public void TestSpawnArcher()
        {
            if (!level7ReachedTowers.Contains("Archer"))
            {
                level7ReachedTowers.Add("Archer");
                StartCoroutine(SpawnAllGrades("Archer"));
            }
        }
        
        [ContextMenu("Test Spawn Wizard")]
        public void TestSpawnWizard()
        {
            if (!level7ReachedTowers.Contains("Wizard"))
            {
                level7ReachedTowers.Add("Wizard");
                StartCoroutine(SpawnAllGrades("Wizard"));
            }
        }
        
        [ContextMenu("Test Spawn WizardTower")]
        public void TestSpawnWizardTower()
        {
            if (!level7ReachedTowers.Contains("WizardTower"))
            {
                level7ReachedTowers.Add("WizardTower");
                StartCoroutine(SpawnAllGrades("WizardTower"));
            }
        }
        
        [ContextMenu("Test Spawn Laser")]
        public void TestSpawnLaser()
        {
            if (!level7ReachedTowers.Contains("Laser"))
            {
                level7ReachedTowers.Add("Laser");
                StartCoroutine(SpawnAllGrades("Laser"));
            }
        }
        
        [ContextMenu("Reset Level7 Tracking")]
        public void ResetLevel7Tracking()
        {
            level7ReachedTowers.Clear();
            Debug.Log("[GradeAbilityTest] 레벨 7 추적 리셋 완료");
        }
    }
}
