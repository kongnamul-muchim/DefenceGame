using UnityEngine;
using System.Collections.Generic;
using DefenceGame.Data;

namespace DefenceGame.Core
{
    public class SpecialAbilityManager : MonoBehaviour
    {
        public static SpecialAbilityManager Instance { get; private set; }
        
        // Tower-specific abilities
        private Dictionary<string, SpecialAbility[]> towerAbilities = new Dictionary<string, SpecialAbility[]>();
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAbilities();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeAbilities()
        {
            // Archer: 투사체 개수 증가 (MultiShot)
            towerAbilities["Archer"] = new SpecialAbility[]
            {
                new SpecialAbility(3, SpecialAbilityType.MultiShot, 2, "투사체 2개 발사"),
                new SpecialAbility(5, SpecialAbilityType.MultiShot, 3, "투사체 3개 발사"),
                new SpecialAbility(7, SpecialAbilityType.MultiShot, 4, "투사체 4개 발사")
            };
            
            // Mage: 지속 피해 바닥 생성 (GroundEffect)
            towerAbilities["Mage"] = new SpecialAbility[]
            {
                new SpecialAbility(3, SpecialAbilityType.GroundEffect, 3, "3초 지속 피해 바닥"),
                new SpecialAbility(5, SpecialAbilityType.GroundEffect, 5, "5초 지속 피해 바닥"),
                new SpecialAbility(7, SpecialAbilityType.GroundEffect, 7, "7초 지속 피해 바닥")
            };
            
            // MageTower: 사거리 증가 및 공격력 증가
            towerAbilities["MageTower"] = new SpecialAbility[]
            {
                new SpecialAbility(3, SpecialAbilityType.RangeIncrease, 1, "사거리 +1"),
                new SpecialAbility(5, SpecialAbilityType.AttackIncrease, 0.3f, "공격력 30% 증가"),
                new SpecialAbility(7, SpecialAbilityType.RangeIncrease, 2, "사거리 +2")
            };
            
            // Laser: 연계 공격 (ChainAttack)
            towerAbilities["Laser"] = new SpecialAbility[]
            {
                new SpecialAbility(3, SpecialAbilityType.ChainAttack, 1, "연계 1회 (주변 1명)"),
                new SpecialAbility(5, SpecialAbilityType.ChainAttack, 2, "연계 2회 (주변 2명)"),
                new SpecialAbility(7, SpecialAbilityType.ChainAttack, 3, "연계 3회 (주변 3명)")
            };
        }
        
        public SpecialAbility[] GetAbilitiesForTower(string towerType)
        {
            if (towerAbilities.ContainsKey(towerType))
            {
                return towerAbilities[towerType];
            }
            return new SpecialAbility[0];
        }
        
        public List<SpecialAbility> GetUnlockedAbilities(string towerType, int currentLevel)
        {
            List<SpecialAbility> unlockedAbilities = new List<SpecialAbility>();
            
            if (towerAbilities.ContainsKey(towerType))
            {
                foreach (var ability in towerAbilities[towerType])
                {
                    if (currentLevel >= ability.unlockLevel)
                    {
                        unlockedAbilities.Add(ability);
                    }
                }
            }
            
            return unlockedAbilities;
        }
        
        public SpecialAbility GetAbilityAtLevel(string towerType, int level)
        {
            if (towerAbilities.ContainsKey(towerType))
            {
                foreach (var ability in towerAbilities[towerType])
                {
                    if (ability.unlockLevel == level)
                    {
                        return ability;
                    }
                }
            }
            return null;
        }
    }
}
