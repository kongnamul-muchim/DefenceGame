using UnityEngine;
using System.Collections.Generic;
using DefenceGame.Data;

namespace DefenceGame.Core
{
    public class UnitAbility : MonoBehaviour
    {
        [Header("Ability Values")]
        public int multiShotCount = 1;
        public float groundEffectDuration = 0f;
        public float rangeIncreaseValue = 0f;
        public float attackIncreaseValue = 0f;
        public int chainAttackCount = 0;
        public float speedIncreaseValue = 0f;
        
        [Header("Ability Settings")]
        public float groundEffectProcChance = 0.1f;
        public GameObject groundEffectPrefab;
        
        // 등급 기반 계산된 최종 값들
        [Header("Grade-Based Final Values")]
        [SerializeField] private float finalSpeedIncreaseValue = 0f;
        [SerializeField] private float finalGroundEffectChance = 0f;
        [SerializeField] private float finalAttackIncreaseValue = 0f;
        
        private string towerType = "";
        private Unit unit;
        
        [Header("Debug Info")]
        [SerializeField] private int currentTowerLevel = 0;
        [SerializeField] private string currentTowerType = "";
        
        [Header("Test Mode")]
        [Tooltip("테스트 모드: Initialize 시 현재 타워 레벨을 자동으로 적용")]
        public bool autoApplyCurrentLevel = true;
        
        private void Awake()
        {
            unit = GetComponent<Unit>();
        }
        
        public void Initialize(string type)
        {
            towerType = type;
            currentTowerType = type;
            
            // 테스트 모드: 현재 타워 레벨 자동 적용
            if (autoApplyCurrentLevel && TowerLevelManager.Instance != null)
            {
                int actualLevel = TowerLevelManager.Instance.GetTowerLevel(towerType);
                if (actualLevel > 0)
                {
                    ForceApplyLevel(actualLevel);
                    Debug.Log($"[UnitAbility] {towerType} 테스트 모드로 레벨 {actualLevel} 자동 적용");
                    return;
                }
            }
            
            ApplySpecialAbilities();
        }
        
        /// <summary>
        /// 특정 레벨의 특수능력 강제 적용 (테스트용)
        /// </summary>
        public void ForceApplyLevel(int level)
        {
            currentTowerLevel = level;
            
            if (string.IsNullOrEmpty(towerType)) return;
            if (SpecialAbilityManager.Instance == null) return;
            
            // Reset abilities
            multiShotCount = 1;
            groundEffectDuration = 0f;
            rangeIncreaseValue = 0f;
            attackIncreaseValue = 0f;
            chainAttackCount = 0;
            speedIncreaseValue = 0f;
            
            var unlockedAbilities = SpecialAbilityManager.Instance.GetUnlockedAbilities(towerType, level);
            
            Debug.Log($"[UnitAbility] {towerType} 레벨 {level} 능력 적용: {unlockedAbilities.Count}개");
            
            foreach (var ability in unlockedAbilities)
            {
                Debug.Log($"[UnitAbility] 적용: {ability.abilityType} = {ability.value}");
                switch (ability.abilityType)
                {
                    case SpecialAbilityType.MultiShot:
                        multiShotCount = Mathf.Max(multiShotCount, (int)ability.value);
                        break;
                    case SpecialAbilityType.GroundEffect:
                        groundEffectDuration = Mathf.Max(groundEffectDuration, ability.value);
                        break;
                    case SpecialAbilityType.ChainAttack:
                        chainAttackCount = Mathf.Max(chainAttackCount, (int)ability.value);
                        break;
                    case SpecialAbilityType.RangeIncrease:
                        rangeIncreaseValue = Mathf.Max(rangeIncreaseValue, ability.value);
                        break;
                    case SpecialAbilityType.AttackIncrease:
                        attackIncreaseValue = Mathf.Max(attackIncreaseValue, ability.value);
                        break;
                    case SpecialAbilityType.SpeedIncrease:
                        speedIncreaseValue = Mathf.Max(speedIncreaseValue, ability.value);
                        break;
                }
            }
            
            // 등급 기반 최종 값 계산
            CalculateGradeBasedValues();
            
            ApplyBuffs();
        }
        
        public void OnLevelUp(string type, int level)
        {
            if (type == towerType)
            {
                ApplySpecialAbilities();
            }
        }
        
        private void ApplySpecialAbilities()
        {
            if (string.IsNullOrEmpty(towerType)) return;
            if (SpecialAbilityManager.Instance == null) return;
            if (TowerLevelManager.Instance == null) return;
            
            int currentLevel = TowerLevelManager.Instance.GetTowerLevel(towerType);
            currentTowerLevel = currentLevel; // 인스펙터 표시용
            
            Debug.Log($"[UnitAbility] {towerType} 현재 레벨: {currentLevel}");
            
            // Reset abilities
            multiShotCount = 1;
            groundEffectDuration = 0f;
            rangeIncreaseValue = 0f;
            attackIncreaseValue = 0f;
            chainAttackCount = 0;
            speedIncreaseValue = 0f;
            
            var unlockedAbilities = SpecialAbilityManager.Instance.GetUnlockedAbilities(towerType, currentLevel);
            
            Debug.Log($"[UnitAbility] {towerType} 해제된 능력 수: {unlockedAbilities.Count}");
            
            foreach (var ability in unlockedAbilities)
            {
                Debug.Log($"[UnitAbility] 적용 중: {ability.abilityType} = {ability.value}");
                switch (ability.abilityType)
                {
                    case SpecialAbilityType.MultiShot:
                        multiShotCount = Mathf.Max(multiShotCount, (int)ability.value);
                        break;
                    case SpecialAbilityType.GroundEffect:
                        groundEffectDuration = Mathf.Max(groundEffectDuration, ability.value);
                        break;
                    case SpecialAbilityType.ChainAttack:
                        chainAttackCount = Mathf.Max(chainAttackCount, (int)ability.value);
                        break;
                    case SpecialAbilityType.RangeIncrease:
                        rangeIncreaseValue = Mathf.Max(rangeIncreaseValue, ability.value);
                        break;
                    case SpecialAbilityType.AttackIncrease:
                        attackIncreaseValue = Mathf.Max(attackIncreaseValue, ability.value);
                        break;
                    case SpecialAbilityType.SpeedIncrease:
                        speedIncreaseValue = Mathf.Max(speedIncreaseValue, ability.value);
                        break;
                }
            }
            
            // 등급 기반 최종 값 계산
            CalculateGradeBasedValues();
            
            ApplyBuffs();
        }
        
        /// <summary>
        /// 등급에 따라 능력 값 계산
        /// </summary>
        private void CalculateGradeBasedValues()
        {
            if (unit == null) return;
            
            GradeType grade = unit.grade;
            
            // Archer: 공격속도 증가폭 증가
            if (towerType == "Archer" && speedIncreaseValue > 0)
            {
                finalSpeedIncreaseValue = GradeMultiplier.ApplyMultiplier(speedIncreaseValue, grade);
                Debug.Log($"[UnitAbility] Archer Speed Increase: Base={speedIncreaseValue}, Grade={grade}, Final={finalSpeedIncreaseValue:F2}");
            }
            
            // Wizard: GroundEffect 확률 증가
            if (towerType == "Wizard")
            {
                finalGroundEffectChance = GradeMultiplier.ApplyMultiplier(groundEffectProcChance, grade);
                Debug.Log($"[UnitAbility] Wizard GroundEffect Chance: Base={groundEffectProcChance}, Grade={grade}, Final={finalGroundEffectChance:F2}");
            }
            
            // Tower: 공격력 상승치 증가
            if (towerType == "WizardTower" && attackIncreaseValue > 0)
            {
                finalAttackIncreaseValue = GradeMultiplier.ApplyMultiplier(attackIncreaseValue, grade);
                Debug.Log($"[UnitAbility] Tower Attack Increase: Base={attackIncreaseValue}, Grade={grade}, Final={finalAttackIncreaseValue:F2}");
            }
            
            // Laser: 등급 기반 확산(ChainAttack) 설정
            if (towerType == "Laser")
            {
                int gradeChainCount = GetGradeBasedChainAttackCount(grade);
                // SpecialAbilityManager에서 가져온 값과 등급 기반 값 중 큰 것 사용
                chainAttackCount = Mathf.Max(chainAttackCount, gradeChainCount);
                Debug.Log($"[UnitAbility] Laser ChainAttack: Grade={grade}, BaseCount={chainAttackCount}, GradeCount={gradeChainCount}, Final={chainAttackCount}");
            }
        }
        
        /// <summary>
        /// Laser 등급별 확산 공격 횟수 반환
        /// Common: 0, Uncommon: 1, Rare: 1, Epic: 2, Legendary: 3
        /// </summary>
        private int GetGradeBasedChainAttackCount(GradeType grade)
        {
            switch (grade)
            {
                case GradeType.Common:
                    return 0;
                case GradeType.Uncommon:
                    return 1;
                case GradeType.Rare:
                    return 1;
                case GradeType.Epic:
                    return 2;
                case GradeType.Legendary:
                    return 3;
                default:
                    return 0;
            }
        }
        
        private void ApplyBuffs()
        {
            if (unit != null)
            {
                unit.range += rangeIncreaseValue;
                
                // Tower는 등급 기반 공격력 증가 적용
                if (towerType == "WizardTower" && finalAttackIncreaseValue > 0)
                {
                    unit.attackPower *= (1f + finalAttackIncreaseValue);
                }
                else if (attackIncreaseValue > 0)
                {
                    unit.attackPower *= (1f + attackIncreaseValue);
                }
                
                // Archer는 등급 기반 공격속도 증가 적용
                if (towerType == "Archer" && finalSpeedIncreaseValue > 0)
                {
                    unit.attackSpeed *= (1f + finalSpeedIncreaseValue);
                }
                else if (speedIncreaseValue > 0)
                {
                    unit.attackSpeed *= (1f + speedIncreaseValue);
                }
            }
        }
        
        public bool ShouldTriggerGroundEffect()
        {
            // Wizard는 등급 기반 확률 적용
            float chance = (towerType == "Wizard" && finalGroundEffectChance > 0) 
                ? finalGroundEffectChance 
                : groundEffectProcChance;
            
            return towerType == "Wizard" && groundEffectDuration > 0 && Random.value <= chance;
        }
        
        public void SpawnGroundEffect(Vector3 position)
        {
            if (groundEffectPrefab == null) return;
            
            GameObject groundEffectObj = Instantiate(groundEffectPrefab, position, Quaternion.identity);
            GroundEffect groundEffect = groundEffectObj.GetComponent<GroundEffect>();
            if (groundEffect != null)
            {
                // 지속시간 5초로 늘리고, 데미지는 낮춤 (총 데미지 = attackPower)
                float damagePerSec = unit != null ? unit.attackPower * 0.2f : 10f;
                groundEffect.Initialize(5f, damagePerSec, 1.5f);
                Debug.Log($"[UnitAbility] GroundEffect spawned: Duration=5s, DPS={damagePerSec:F1}");
            }
        }
        
        /// <summary>
        /// Laser 연계 공격용 등급 기반 데미지 계산
        /// </summary>
        public float GetChainDamageMultiplier()
        {
            if (unit == null) return 1.0f;
            return GradeMultiplier.GetWeakMultiplier(unit.grade);
        }
    }
}
