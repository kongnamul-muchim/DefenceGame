using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefenceGame.Data
{
    [CreateAssetMenu(fileName = "GameData", menuName = "DefenceGame/GameData")]
    public class GameDataSO : ScriptableObject
    {
        [Header("Enemies")]
        public List<EnemyData> Enemies = new List<EnemyData>();

        [Header("Towers")]
        public List<TowerData> Towers = new List<TowerData>();

        [Header("Waves")]
        public List<WaveData> Waves = new List<WaveData>();

        [Header("Gacha Probabilities")]
        public List<GachaData> GachaProbabilities = new List<GachaData>();

        [Header("Unit Grades")]
        public List<UnitGradeData> UnitGrades = new List<UnitGradeData>();

        public void ClearAll()
        {
            Enemies.Clear();
            Towers.Clear();
            Waves.Clear();
            GachaProbabilities.Clear();
            UnitGrades.Clear();
        }
    }
}
