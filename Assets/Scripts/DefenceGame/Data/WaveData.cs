using System;

namespace DefenceGame.Data
{
    [Serializable]
    public class WaveData
    {
        public int WaveNumber;
        public int EnemyId;
        public int Count;
        public float SpawnInterval;
        public float HealthMultiplier;
    }
}
