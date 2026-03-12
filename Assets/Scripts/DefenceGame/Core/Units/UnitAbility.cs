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
        
        private void Awake()
        {
            unit = GetComponent<Unit>();
        }
        
        public void Initialize(string type)
        {
            towerType = type;
            ApplySpecialAbilities();
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
            
            // Reset abilities
            multiShotCount = 1;
            groundEffectDuration = 0f;
            rangeIncreaseValue = 0f;
            attackIncreaseValue = 0f;
            chainAttackCount = 0;
            speedIncreaseValue = 0f;
            
            var unlockedAbilities = SpecialAbilityManager.Instance.GetUnlockedAbilities(towerType, currentLevel);
            
            foreach (var ability in unlockedAbilities)
            {
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
                    // Laser 레벨 특수 능력
                    case SpecialAbilityType.AttackUp:
                        attackIncreaseValue = Mathf.Max(attackIncreaseValue, ability.value);
                        break;
                    case SpecialAbilityType.SpeedDown:
                        // 공격속도 감소 (음수로 저장)
                        speedIncreaseValue = Mathf.Min(speedIncreaseValue, -ability.value);
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
            }
            
            // Wizard: GroundEffect 확률 증가
            if (towerType == "Wizard")
            {
                finalGroundEffectChance = GradeMultiplier.ApplyMultiplier(groundEffectProcChance, grade);
            }
            
            // Tower: 공격력 상승치 증가
            if (towerType == "WizardTower" && attackIncreaseValue > 0)
            {
                finalAttackIncreaseValue = GradeMultiplier.ApplyMultiplier(attackIncreaseValue, grade);
            }
            
            // Laser: 등급 기반 확산(ChainAttack) 설정
            if (towerType == "Laser")
            {
                int gradeChainCount = GetGradeBasedChainAttackCount(grade);
                // 등급 기반 값이 있으면 그것을 사용, 없으면 SpecialAbilityManager의 값 사용
                // 단, Common은 확산 없음(0)
                if (grade == GradeType.Common)
                {
                    chainAttackCount = 0;
                }
                else
                {
                    // 등급 기반 값과 레벨 기반 값 중 큰 값 사용
                    chainAttackCount = Mathf.Max(chainAttackCount, gradeChainCount);
                }
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
                // 사거리 증가: 특수능력RangeIncrease × UnitGrades.RangeMultiplier
                if (rangeIncreaseValue > 0)
                {
                    float gradeRangeMultiplier = GetGradeRangeMultiplier(unit.grade);
                    unit.range += rangeIncreaseValue * gradeRangeMultiplier;
                }
                
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
                else if (speedIncreaseValue != 0)  // 0이 아닐 때만 적용 (양수/음수 모두)
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
                // 지속시간 5초로 리고, 데미지는 낮춤 (총 데미지 = attackPower)
                float damagePerSec = unit != null ? unit.attackPower * 0.2f : 10f;
                groundEffect.Initialize(5f, damagePerSec, 1.5f, unit); // attacker 전달
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
        
        /// <summary>
        /// 사거리 증가 값 반환 (Range 계산용)
        /// </summary>
        public float GetRangeIncreaseValue()
        {
            return rangeIncreaseValue;
        }
        
        /// <summary>
        /// UnitGrades 시트에서 해당 등급의 RangeMultiplier 값을 가져옴
        /// </summary>
        private float GetGradeRangeMultiplier(GradeType grade)
        {
            if (GameDataSO.Instance == null) return 1f;
            
            foreach (var unitGrade in GameDataSO.Instance.UnitGrades)
            {
                if (unitGrade.Grade == grade)
                {
                    return unitGrade.RangeMultiplier;
                }
            }
            
            return 1f;
        }
    }
}
