using UnityEngine;
using System.Collections.Generic;
using DefenceGame.Data;

namespace DefenceGame.Core
{
    public class UnitMergeManager : MonoBehaviour
    {
        public static UnitMergeManager Instance { get; private set; }
        
        [Header("Merge Settings")]
        public int mergeCount = 2; // 합성에 필요한 유닛 수 (2개)
        
        [Header("Effects")]
        public ParticleSystem mergeEffectPrefab; // Inspector에서 연결
        public AudioClip mergeSound; // 합성 사운드
        
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
        
        // 합성 가능 여부 확인
        public bool CanMerge(Unit draggedUnit, Unit targetUnit)
        {
            if (draggedUnit == null || targetUnit == null) return false;
            
            // 1. 같은 등급인가?
            if (draggedUnit.grade != targetUnit.grade)
                return false;
            
            // 2. 같은 이름인가?
            if (draggedUnit.unitName != targetUnit.unitName)
                return false;
            
            // 3. Legendary는 합성 불가
            if (draggedUnit.grade == GradeType.Legendary)
                return false;
            
            return true;
        }
        
        // 합성 실행
        public void MergeUnits(Unit draggedUnit, Unit targetUnit)
        {
            if (!CanMerge(draggedUnit, targetUnit))
            {
                Debug.LogWarning("Cannot merge these units!");
                return;
            }
            
            // 1. 다음 등급 계산
            GradeType nextGrade = GetNextGrade(draggedUnit.grade);
            
            // 2. 위치 저장 (타겟 유닛 위치 = 드래그하지 않은 유닛 위치)
            Vector3 mergePosition = targetUnit.transform.position;
            
            // 3. 원래 유닛 이름 저장 (같은 이름의 상위 등급 유닛을 찾기 위해)
            string unitName = draggedUnit.unitName;
            GradeType originalGrade = draggedUnit.grade;
            
            // 4. 이펙트 재생 (유닛 제거 전)
            PlayMergeEffect(mergePosition);
            
            // 5. 기존 유닛들 제거
            // PlacementManager에서 제거
            if (UnitPlacementManager.Instance != null)
            {
                UnitPlacementManager.Instance.RemoveUnit(draggedUnit);
                UnitPlacementManager.Instance.RemoveUnit(targetUnit);
            }
            
            Destroy(draggedUnit.gameObject);
            Destroy(targetUnit.gameObject);
            
            // 6. 새 유닛 생성 (같은 이름의 상위 등급 유닛)
            TowerData newTower = GetUpgradedTower(unitName, nextGrade);
            if (newTower != null)
            {
                UnitPlacementManager.Instance?.PlaceUnitAtPosition(newTower, mergePosition);
                Debug.Log($"Merge successful! {originalGrade} {unitName} x{mergeCount} → {nextGrade} {newTower.Name}");
            }
            else
            {
                // 같은 이름의 상위 등급 유닛이 없으면 해당 등급의 랜덤 유닛 생성
                newTower = GetRandomTowerByGrade(nextGrade);
                if (newTower != null)
                {
                    UnitPlacementManager.Instance?.PlaceUnitAtPosition(newTower, mergePosition);
                    Debug.Log($"Merge successful! {originalGrade} {unitName} x{mergeCount} → {nextGrade} {newTower.Name} (random)");
                }
                else
                {
                    Debug.LogError($"Failed to get tower for grade: {nextGrade}");
                }
            }
        }
        
        // 다음 등급 계산
        private GradeType GetNextGrade(GradeType current)
        {
            switch (current)
            {
                case GradeType.Common:
                    return GradeType.Uncommon;
                case GradeType.Uncommon:
                    return GradeType.Rare;
                case GradeType.Rare:
                    return GradeType.Epic;
                case GradeType.Epic:
                    return GradeType.Legendary;
                default:
                    return GradeType.Legendary; // 최고 등급 유지
            }
        }
        
        // 특정 등급의 랜덤 타워 선택 (가챠 확률 적용)
        private TowerData GetRandomTowerByGrade(GradeType targetGrade)
        {
            if (GameDataSO.Instance == null)
            {
                Debug.LogError("GameDataSO.Instance is null!");
                return null;
            }
            
            // 해당 등급의 모든 타워 가져오기
            List<TowerData> towersOfGrade = new List<TowerData>();
            foreach (var tower in GameDataSO.Instance.Towers)
            {
                if (tower.Grade == targetGrade)
                {
                    towersOfGrade.Add(tower);
                }
            }
            
            if (towersOfGrade.Count == 0)
            {
                Debug.LogWarning($"No towers found for grade: {targetGrade}");
                return null;
            }
            
            // 랜덤 선택
            int randomIndex = Random.Range(0, towersOfGrade.Count);
            return towersOfGrade[randomIndex];
        }
        
        // 같은 이름의 상위 등급 타워 찾기
        private TowerData GetUpgradedTower(string unitName, GradeType targetGrade)
        {
            if (GameDataSO.Instance == null)
            {
                Debug.LogError("GameDataSO.Instance is null!");
                return null;
            }
            
            // 같은 이름의 상위 등급 타워 찾기
            foreach (var tower in GameDataSO.Instance.Towers)
            {
                if (tower.Name == unitName && tower.Grade == targetGrade)
                {
                    return tower;
                }
            }
            
            Debug.LogWarning($"No upgraded tower found for {unitName} at grade {targetGrade}");
            return null;
        }
        
        // 합성 이펙트 재생
        private void PlayMergeEffect(Vector3 position)
        {
            if (mergeEffectPrefab != null)
            {
                ParticleSystem effect = Instantiate(mergeEffectPrefab, position, Quaternion.identity);
                Destroy(effect.gameObject, effect.main.duration);
            }
            
            if (mergeSound != null)
            {
                AudioSource.PlayClipAtPoint(mergeSound, position);
            }
        }
    }
}
