using UnityEngine;
using DefenceGame.Data;
using System.Collections.Generic;

namespace DefenceGame.Core
{
    public class TowerLevelManager : MonoBehaviour
    {
        public static TowerLevelManager Instance { get; private set; }
        
        [Header("Tower Types")]
        public string[] towerTypes = { "Archer", "Wizard", "WizardTower", "Laser" };
        
        [Header("Experience Settings")]
        public int commonExp = 50;
        public int uncommonExp = 100;
        public int rareExp = 150;
        public int epicExp = 200;
        public int legendaryExp = 0; // Legendary gives no exp
        
        // Tower level data storage
        private Dictionary<string, TowerLevelData> towerLevels = new Dictionary<string, TowerLevelData>();
        
        // Events
        public System.Action<string, int> OnTowerLevelUp; // towerType, newLevel
        public System.Action<string> OnTowerExpChanged; // towerType
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeTowerLevels();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeTowerLevels()
        {
            // Initialize level data for all tower types
            foreach (string towerType in towerTypes)
            {
                if (!towerLevels.ContainsKey(towerType))
                {
                    towerLevels[towerType] = new TowerLevelData();
                }
            }
        }
        
        public void AddExp(string towerType, GradeType grade)
        {
            if (!towerLevels.ContainsKey(towerType))
            {
                towerLevels[towerType] = new TowerLevelData();
            }
            
            // Legendary gives no exp
            if (grade == GradeType.Legendary)
            {
                return;
            }
            
            // Get exp amount based on grade
            int expAmount = GetExpForGrade(grade);
            
            // Add exp and check for level up
            bool leveledUp = towerLevels[towerType].AddExp(expAmount);
            
            // Notify exp change
            OnTowerExpChanged?.Invoke(towerType);
            
            // Notify level up if occurred
            if (leveledUp)
            {
                OnTowerLevelUp?.Invoke(towerType, towerLevels[towerType].level);
                Debug.Log($"Tower {towerType} leveled up to level {towerLevels[towerType].level}!");
            }
        }
        
        private int GetExpForGrade(GradeType grade)
        {
            switch (grade)
            {
                case GradeType.Common:
                    return commonExp;
                case GradeType.Uncommon:
                    return uncommonExp;
                case GradeType.Rare:
                    return rareExp;
                case GradeType.Epic:
                    return epicExp;
                case GradeType.Legendary:
                    return legendaryExp;
                default:
                    return commonExp;
            }
        }
        
        public TowerLevelData GetTowerLevelData(string towerType)
        {
            if (!towerLevels.ContainsKey(towerType))
            {
                towerLevels[towerType] = new TowerLevelData();
            }
            return towerLevels[towerType];
        }
        
        public int GetTowerLevel(string towerType)
        {
            if (towerLevels.ContainsKey(towerType))
            {
                return towerLevels[towerType].level;
            }
            return 0;
        }
        
        public float GetTowerExpPercentage(string towerType)
        {
            if (towerLevels.ContainsKey(towerType))
            {
                return towerLevels[towerType].GetExpPercentage();
            }
            return 0f;
        }
        
        public void ResetAllLevels()
        {
            towerLevels.Clear();
            InitializeTowerLevels();
        }
    }
}
