using UnityEngine;

namespace DefenceGame.Data
{
    public enum SpecialAbilityType
    {
        None,
        // Archer: 투사체 개수 증가
        MultiShot,      // 다중 발사 (투사체 개수 증가)
        // Mage: 지속 피해 바닥 생성
        GroundEffect,   // 지속 피해 바닥 생성
        // MageTower: 사거리 증가 및 공격력 증가
        RangeIncrease,  // 사거리 증가
        AttackIncrease, // 공격력 증가
        // Laser: 연계 공격 (주변에 전이) + 레벨 특수 능력
        ChainAttack,    // 연계 공격 (전이) - 등급 기반
        AttackUp,       // 레벨 특수: 공격력 증가
        SpeedDown,      // 레벨 특수: 공격속도 감소
        // Deprecated (기존 능력들)
        Pierce,         // [Deprecated] 관통
        AreaDamage,     // [Deprecated] 광역
        AttackBuff,     // [Deprecated] 공격력 버프
        SpeedBuff,      // [Deprecated] 공격속도 버프
        SpeedIncrease,  // [Deprecated] 공격속도 증가
        SlowEffect      // [Deprecated] 이동속도 감소
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
