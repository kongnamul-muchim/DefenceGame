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
            // Archer abilities
            towerAbilities["Archer"] = new SpecialAbility[]
            {
                new SpecialAbility(3, SpecialAbilityType.Pierce, 1, "관통 1"),
                new SpecialAbility(5, SpecialAbilityType.Pierce, 2, "관통 2"),
                new SpecialAbility(7, SpecialAbilityType.SpeedIncrease, 0.2f, "공격속도 20%")
            };
            
            // Mage abilities (Wizard)
            towerAbilities["Mage"] = new SpecialAbility[]
            {
                new SpecialAbility(3, SpecialAbilityType.AreaDamage, 1, "광역 1칸"),
                new SpecialAbility(5, SpecialAbilityType.AreaDamage, 2, "광역 2칸"),
                new SpecialAbility(7, SpecialAbilityType.AttackIncrease, 0.3f, "공격력 30%")
            };
            
            // MageTower abilities (WizardTower)
            towerAbilities["MageTower"] = new SpecialAbility[]
            {
                new SpecialAbility(3, SpecialAbilityType.AttackBuff, 0.1f, "공격력 버프 10%"),
                new SpecialAbility(5, SpecialAbilityType.SlowEffect, 0.3f, "이동속도 30% 감소"),
                new SpecialAbility(7, SpecialAbilityType.RangeIncrease, 1, "범위 +1")
            };
            
            // Laser abilities
            towerAbilities["Laser"] = new SpecialAbility[]
            {
                new SpecialAbility(3, SpecialAbilityType.RangeIncrease, 1, "사거리 +1"),
                new SpecialAbility(5, SpecialAbilityType.RangeIncrease, 2, "사거리 +2"),
                new SpecialAbility(7, SpecialAbilityType.Pierce, 1, "관통")
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
