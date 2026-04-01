using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefenceGame.Data
{
    [CreateAssetMenu(fileName = "GameData", menuName = "DefenceGame/GameData")]
    public class GameDataSO : ScriptableObject
    {
        private static GameDataSO _instance;
        public static GameDataSO Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Try to load from Resources folder
                    _instance = Resources.Load<GameDataSO>("GameData");
                    if (_instance == null)
                    {
                        Debug.LogError("[GameDataSO] GameData.asset not found in Resources folder! Please move it to Assets/Resources/");
                    }
                }
                return _instance;
            }
        }
        
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
        
        private void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this;
            }
        }
        
        public void ClearAll()
        {
            Enemies.Clear();
            Towers.Clear();
            Waves.Clear();
            GachaProbabilities.Clear();
            UnitGrades.Clear();
        }
        
        /// <summary>
        /// 특정 등급의 타워 목록 반환
        /// </summary>
        public List<TowerData> GetTowersByGrade(GradeType grade)
        {
            List<TowerData> result = new List<TowerData>();
            foreach (var tower in Towers)
            {
                if (tower.Grade == grade)
                {
                    result.Add(tower);
                }
            }
            return result;
        }
    }
}
