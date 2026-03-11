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
                }
            }
            
            ApplyBuffs();
        }
        
        private void ApplyBuffs()
        {
            if (unit != null)
            {
                unit.range += rangeIncreaseValue;
                unit.attackPower *= (1f + attackIncreaseValue);
                unit.attackSpeed *= (1f + speedIncreaseValue);
            }
        }
        
        public bool ShouldTriggerGroundEffect()
        {
            return towerType == "Wizard" && groundEffectDuration > 0 && Random.value <= groundEffectProcChance;
        }
        
        public void SpawnGroundEffect(Vector3 position)
        {
            if (groundEffectPrefab == null) return;
            
            GameObject groundEffectObj = Instantiate(groundEffectPrefab, position, Quaternion.identity);
            GroundEffect groundEffect = groundEffectObj.GetComponent<GroundEffect>();
            if (groundEffect != null)
            {
                float damagePerSec = unit != null ? unit.attackPower * 0.5f : 10f;
                groundEffect.Initialize(1.3f, damagePerSec, 1.5f);
            }
        }
    }
}
