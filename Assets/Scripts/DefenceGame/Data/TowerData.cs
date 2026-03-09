using System;

namespace DefenceGame.Data
{
    [Serializable]
    public class TowerData
    {
        public int Id;
        public string Name;
        public float AttackPower;
        public float AttackSpeed;
        public float Range;
        public GradeType Grade;
    }
}
