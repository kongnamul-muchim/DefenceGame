using System;

namespace DefenceGame.Data
{
    [Serializable]
    public class UnitGradeData
    {
        public GradeType Grade;
        public float DamageMultiplier;
        public float RangeMultiplier;
        public float AttackSpeedMultiplier;
    }
}
