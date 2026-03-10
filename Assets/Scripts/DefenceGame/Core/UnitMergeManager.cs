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
            
            Debug.Log($"[Merge Check] Dragged: {draggedUnit.unitName} ({draggedUnit.grade}) vs Target: {targetUnit.unitName} ({targetUnit.grade})");
            
            // 1. 같은 등급인가?
            if (draggedUnit.grade != targetUnit.grade)
            {
                Debug.Log($"[Merge Check] FAILED - Different grades: {draggedUnit.grade} vs {targetUnit.grade}");
                return false;
            }
            
            // 2. 같은 이름인가?
            if (draggedUnit.unitName != targetUnit.unitName)
            {
                Debug.Log($"[Merge Check] FAILED - Different names: {draggedUnit.unitName} vs {targetUnit.unitName}");
                return false;
            }
            
            // 3. Legendary는 합성 불가
            if (draggedUnit.grade == GradeType.Legendary)
            {
                Debug.Log($"[Merge Check] FAILED - Legendary cannot be merged");
                return false;
            }
            
            Debug.Log($"[Merge Check] SUCCESS - Can merge {draggedUnit.grade} {draggedUnit.unitName}");
            return true;
        }
        
        // 합성 실행
        public void MergeUnits(Unit draggedUnit, Unit targetUnit)
        {
            if (!CanMerge(draggedUnit, targetUnit))
            {
                Debug.LogWarning("[Merge] Cannot merge these units!");
                return;
            }
            
            // 1. 다음 등급 계산
            GradeType nextGrade = GetNextGrade(draggedUnit.grade);
            Debug.Log($"[Merge] Next grade calculated: {draggedUnit.grade} -> {nextGrade}");
            
            // 2. 위치 저장 (타겟 유닛 위치 = 드래그하지 않은 유닛 위치)
            Vector3 mergePosition = targetUnit.transform.position;
            
            // 3. 원래 유닛 이름 저장 (같은 이름의 상위 등급 유닛을 찾기 위해)
            string unitName = draggedUnit.unitName;
            GradeType originalGrade = draggedUnit.grade;
            
            // 4. 새 유닛 먼저 찾기 (유닛 제거 전에)
            TowerData newTower = GetUpgradedTower(unitName, nextGrade);
            if (newTower == null)
            {
                Debug.LogWarning($"[Merge] No upgraded tower found for {unitName} at grade {nextGrade}, trying random tower...");
                newTower = GetRandomTowerByGrade(nextGrade);
            }
            
            // 새 유닛 데이터가 없으면 합성 취소
            if (newTower == null)
            {
                Debug.LogError($"[Merge] FAILED - No tower found for grade {nextGrade}. Merge cancelled.");
                return;
            }
            
            Debug.Log($"[Merge] New tower selected: {newTower.Name} ({newTower.Grade})");
            
            // 5. 이펙트 재생
            PlayMergeEffect(mergePosition);
            
            // 6. 기존 유닛들 제거 (새 유닛이 확실히 있을 때만)
            if (UnitPlacementManager.Instance != null)
            {
                UnitPlacementManager.Instance.RemoveUnit(draggedUnit);
                UnitPlacementManager.Instance.RemoveUnit(targetUnit);
            }
            
            Destroy(draggedUnit.gameObject);
            Destroy(targetUnit.gameObject);
            
            // 7. 새 유닛 생성
            UnitPlacementManager.Instance?.PlaceUnitAtPosition(newTower, mergePosition);
            Debug.Log($"[Merge] SUCCESS! {originalGrade} {unitName} x{mergeCount} → {nextGrade} {newTower.Name}");
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
                Debug.LogError("[Merge] GameDataSO.Instance is null!");
                return null;
            }
            
            Debug.Log($"[Merge] Looking for random tower of grade: {targetGrade}");
            int towerCount = GameDataSO.Instance.Towers?.Count ?? 0;
            Debug.Log($"[Merge] Total towers in GameData: {towerCount}");
            
            // 해당 등급의 모든 타워 가져오기
            List<TowerData> towersOfGrade = new List<TowerData>();
            if (GameDataSO.Instance.Towers != null)
            {
                foreach (var tower in GameDataSO.Instance.Towers)
                {
                    if (tower.Grade == targetGrade)
                    {
                        towersOfGrade.Add(tower);
                        Debug.Log($"[Merge] Found tower: {tower.Name} ({tower.Grade})");
                    }
                }
            }
            
            if (towersOfGrade.Count == 0)
            {
                Debug.LogWarning($"[Merge] No towers found for grade: {targetGrade}");
                return null;
            }
            
            // 랜덤 선택
            int randomIndex = Random.Range(0, towersOfGrade.Count);
            Debug.Log($"[Merge] Selected random tower: {towersOfGrade[randomIndex].Name} (index {randomIndex}/{towersOfGrade.Count})");
            return towersOfGrade[randomIndex];
        }
        
        // 같은 이름의 상위 등급 타워 찾기
        private TowerData GetUpgradedTower(string unitName, GradeType targetGrade)
        {
            if (GameDataSO.Instance == null)
            {
                Debug.LogError("[Merge] GameDataSO.Instance is null!");
                return null;
            }
            
            Debug.Log($"[Merge] Looking for upgraded tower: {unitName} at grade {targetGrade}");
            
            // 같은 이름의 상위 등급 타워 찾기
            if (GameDataSO.Instance.Towers != null)
            {
                foreach (var tower in GameDataSO.Instance.Towers)
                {
                    Debug.Log($"[Merge] Checking tower: {tower.Name} ({tower.Grade})");
                    if (tower.Name == unitName && tower.Grade == targetGrade)
                    {
                        Debug.Log($"[Merge] Found upgraded tower: {tower.Name} ({tower.Grade})");
                        return tower;
                    }
                }
            }
            
            Debug.LogWarning($"[Merge] No upgraded tower found for {unitName} at grade {targetGrade}");
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
