using UnityEngine;

namespace DefenceGame.Data
{
    public enum SpecialAbilityType
    {
        None,
        Pierce,         // 관통 (Archer, Laser)
        AreaDamage,     // 광역 (Mage)
        AttackBuff,     // 공격력 버프 (MageTower)
        SpeedBuff,      // 공격속도 버프 (MageTower)
        RangeIncrease,  // 사거리 증가 (Laser, MageTower)
        AttackIncrease, // 공격력 증가
        SpeedIncrease,  // 공격속도 증가
        SlowEffect      // 이동속도 감소 (MageTower)
    }
    
    [System.Serializable]
    public class SpecialAbility
    {
        public int unlockLevel;
        public SpecialAbilityType abilityType;
        public float value;
        public string description;
        
        public SpecialAbility(int level, SpecialAbilityType type, float val, string desc)
        {
            unlockLevel = level;
            abilityType = type;
            value = val;
            description = desc;
        }
    }
}
