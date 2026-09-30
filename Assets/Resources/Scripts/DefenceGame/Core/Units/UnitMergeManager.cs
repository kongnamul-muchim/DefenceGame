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
        [Range(0f, 1f)]
        public float sameUnitChance = 0.25f; // 같은 유닛이 나올 확률 (25%), 나머지 75%는 다른 3개 유닛 중 랜덤

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

            // 3. 원래 유닛 이름 저장
            string unitName = draggedUnit.unitName;
            GradeType originalGrade = draggedUnit.grade;

            // 4. 새 유닛 선택 (같은 유닛 확률 또는 랜덤 유닛)
            TowerData newTower = null;

            // 같은 유닛이 나올지 랜덤 유닛이 나올지 결정
            float randomValue = Random.value;
            bool shouldGetSameUnit = randomValue <= sameUnitChance;

            Debug.Log($"[Merge] Random roll: {randomValue:F2}, Same unit chance: {sameUnitChance:F2}, Will get same unit: {shouldGetSameUnit}");

            if (shouldGetSameUnit)
            {
                // 같은 이름의 상위 등급 유닛 찾기 시도
                newTower = GetUpgradedTower(unitName, nextGrade);
                if (newTower != null)
                {
                    Debug.Log($"[Merge] Same unit selected: {newTower.Name}");
                }
                else
                {
                    Debug.Log($"[Merge] No same unit found, falling back to random");
                }
            }

            // 같은 유닛이 없거나 랜덤 유닛을 선택한 경우
            if (newTower == null)
            {
                // 현재 유닛을 제외한 다른 유닛 중에서 랜덤 선택
                newTower = GetRandomTowerByGradeExcept(nextGrade, unitName);
                if (newTower == null)
                {
                    // 다른 유닛이 없으면 현재 유닛이라도 반환
                    newTower = GetUpgradedTower(unitName, nextGrade);
                }
                Debug.Log($"[Merge] Random tower selected: {newTower?.Name ?? "NULL"}");
            }

            // 새 유닛 데이터가 없으면 합성 취소
            if (newTower == null)
            {
                Debug.LogError($"[Merge] FAILED - No tower found for grade {nextGrade}. Merge cancelled.");
                return;
            }

            Debug.Log($"[Merge] New tower selected: {newTower.Name} ({newTower.Grade})");

            // 4. 이펙트 재생
            PlayMergeEffect(mergePosition);

            // 5. 기존 유닛들 제거 (새 유닛이 확실히 있을 때만)
            if (UnitPlacementManager.Instance != null)
            {
                UnitPlacementManager.Instance.RemoveUnit(draggedUnit);
                UnitPlacementManager.Instance.RemoveUnit(targetUnit);
            }

            Destroy(draggedUnit.gameObject);
            Destroy(targetUnit.gameObject);

            // 6. 새 유닛 생성
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

        // 특정 등급의 랜덤 타워 선택 (현재 유닛 제외)
        private TowerData GetRandomTowerByGradeExcept(GradeType targetGrade, string excludeUnitName)
        {
            if (GameDataSO.Instance == null)
            {
                Debug.LogError("[Merge] GameDataSO.Instance is null!");
                return null;
            }

            Debug.Log($"[Merge] Looking for random tower of grade {targetGrade}, excluding: {excludeUnitName}");

            // 해당 등급의 모든 타워 가져오기 (현재 유닛 제외)
            List<TowerData> towersOfGrade = new List<TowerData>();
            if (GameDataSO.Instance.Towers != null)
            {
                foreach (var tower in GameDataSO.Instance.Towers)
                {
                    if (tower.Grade == targetGrade && tower.Name != excludeUnitName)
                    {
                        towersOfGrade.Add(tower);
                        Debug.Log($"[Merge] Found other tower: {tower.Name}");
                    }
                }
            }

            if (towersOfGrade.Count == 0)
            {
                Debug.LogWarning($"[Merge] No other towers found for grade {targetGrade}");
                return null;
            }

            // 랜덤 선택 (남은 3개 중 하나)
            int randomIndex = Random.Range(0, towersOfGrade.Count);
            Debug.Log($"[Merge] Selected other tower: {towersOfGrade[randomIndex].Name} ({randomIndex + 1}/{towersOfGrade.Count})");
            return towersOfGrade[randomIndex];
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

                // Sorting Order 높게 설정하여 Tilemap 위에 표시
                ParticleSystemRenderer renderer = effect.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = 1000; // Tilemap 위에 표시
                }

                Destroy(effect.gameObject, effect.main.duration);
            }

            if (mergeSound != null)
            {
                AudioSource.PlayClipAtPoint(mergeSound, position);
            }
        }
    }
}
