using System;

namespace DefenceGame.Data
{
    [Serializable]
    public class TowerLevelData
    {
        public int level;
        public int currentExp;
        public int expToNextLevel;
        
        public TowerLevelData()
        {
            level = 0;
            currentExp = 0;
            expToNextLevel = CalculateExpToNextLevel(0);
        }
        
        public static int CalculateExpToNextLevel(int currentLevel)
        {
            // Formula: 100 + (currentLevel × 50)
            return 100 + (currentLevel * 50);
        }
        
        public bool AddExp(int exp)
        {
            currentExp += exp;
            bool leveledUp = false;
            
            // Check for level up with overflow handling
            while (currentExp >= expToNextLevel)
            {
                currentExp -= expToNextLevel;
                level++;
                expToNextLevel = CalculateExpToNextLevel(level);
                leveledUp = true;
            }
            
            return leveledUp;
        }
        
        public float GetExpPercentage()
        {
            if (expToNextLevel <= 0) return 1f;
            return (float)currentExp / expToNextLevel;
        }
    }
}
