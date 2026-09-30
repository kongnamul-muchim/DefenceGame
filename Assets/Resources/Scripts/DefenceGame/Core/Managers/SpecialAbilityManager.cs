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
            // Archer: 투사체 개수 증가 (MultiShot) + 공격속도 증가 (30%로 조정)
            towerAbilities["Archer"] = new SpecialAbility[]
            {
                new SpecialAbility(3, SpecialAbilityType.MultiShot, 2, "투사체 2개 발사"),
                new SpecialAbility(5, SpecialAbilityType.MultiShot, 3, "투사체 3개 발사"),
                new SpecialAbility(7, SpecialAbilityType.SpeedIncrease, 0.3f, "공격속도 30% 증가")
            };

            // Wizard: 지속 피해 바닥 생성 (GroundEffect)
            towerAbilities["Wizard"] = new SpecialAbility[]
            {
                new SpecialAbility(3, SpecialAbilityType.GroundEffect, 3, "3초 지속 피해 바닥"),
                new SpecialAbility(5, SpecialAbilityType.GroundEffect, 5, "5초 지속 피해 바닥"),
                new SpecialAbility(7, SpecialAbilityType.GroundEffect, 7, "7초 지속 피해 바닥")
            };

            // WizardTower: 사거리 증가 및 공격력 증가 (사거리 증가 절반으로 조정)
            towerAbilities["WizardTower"] = new SpecialAbility[]
            {
                new SpecialAbility(3, SpecialAbilityType.RangeIncrease, 0.5f, "사거리 +0.5"),
                new SpecialAbility(5, SpecialAbilityType.AttackIncrease, 0.3f, "공격력 30% 증가"),
                new SpecialAbility(7, SpecialAbilityType.RangeIncrease, 1f, "사거리 +1")
            };

            // Laser: 연계 공격은 등급 기반, 레벨 특수 능력은 데미지+ 공격속도-
            towerAbilities["Laser"] = new SpecialAbility[]
            {
                new SpecialAbility(3, SpecialAbilityType.AttackUp, 0.2f, "공격력 20% 증가"),
                new SpecialAbility(5, SpecialAbilityType.SpeedDown, 0.15f, "공격속도 15% 감소"),
                new SpecialAbility(7, SpecialAbilityType.AttackUp, 0.3f, "공격력 30% 증가")
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
