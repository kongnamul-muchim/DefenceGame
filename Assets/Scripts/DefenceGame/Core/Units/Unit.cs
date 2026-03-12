using UnityEngine;
using DefenceGame.Data;

namespace DefenceGame.Core
{
    public class Unit : MonoBehaviour
    {
        [Header("Unit Data")]
        public int id;
        public string unitName;
        public float attackPower;
        public float attackSpeed;
        public float range;
        public GradeType grade;
        public bool isRanged = true;
        public string TowerType { get; private set; } = "";
        
        [Header("Components")]
        public SpriteRenderer spriteRenderer;
        public Transform attackPoint;
        public GameObject bulletPrefab;
        public GameObject groundEffectPrefab;
        
        // Component references
        private UnitAttack unitAttack;
        private UnitUI unitUI;
        private UnitDrag unitDrag;
        private UnitAbility unitAbility;
        
        private void Awake()
        {
            // Get or add components
            unitAttack = GetComponent<UnitAttack>() ?? gameObject.AddComponent<UnitAttack>();
            unitUI = GetComponent<UnitUI>() ?? gameObject.AddComponent<UnitUI>();
            unitDrag = GetComponent<UnitDrag>() ?? gameObject.AddComponent<UnitDrag>();
            unitAbility = GetComponent<UnitAbility>() ?? gameObject.AddComponent<UnitAbility>();
            
            // Setup references
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (attackPoint == null) attackPoint = transform.Find("AttackPoint") ?? transform;
            
            // Add collider
            if (GetComponent<Collider2D>() == null)
            {
                gameObject.AddComponent<BoxCollider2D>();
            }
        }
        
        private void Update()
        {
            if (unitDrag != null && unitDrag.IsDragging()) return;
            
            // Update attack
            if (unitAttack != null)
            {
                unitAttack.attackPower = attackPower;
                unitAttack.attackSpeed = attackSpeed;
                unitAttack.range = range;
                unitAttack.isRanged = isRanged;
                unitAttack.bulletPrefab = bulletPrefab;
                unitAttack.attackPoint = attackPoint;
                unitAttack.UpdateAttack();
            }
            
            // Update UI
            if (unitUI != null)
            {
                unitUI.CheckMouseHover();
                unitUI.UpdateVisualization();
            }
        }
        
        public void Initialize(TowerData data)
        {
            id = data.Id;
            unitName = data.Name;
            grade = data.Grade;
            TowerType = ExtractTowerType(data.Name);
            
            // Calculate attack power: (기본공격력 + 레벨추가공격력) × 레어도공격력
            float gradeDamageMultiplier = GetGradeMultiplier(grade, "damage");
            attackPower = (data.AttackPower + data.LevelBonusAttackPower) * gradeDamageMultiplier;
            
            // Calculate range: Towers시트 Range값 × UnitGrades RangeMultiplier
            float gradeRangeMultiplier = GetGradeMultiplier(grade, "range");
            range = data.Range * gradeRangeMultiplier;
            
            // Attack speed from data
            attackSpeed = data.AttackSpeed;
            
            // Initialize components
            if (unitUI != null) unitUI.SetGradeColor(GetGradeColor(grade));
            if (unitAbility != null) unitAbility.Initialize(TowerType);
            
            // Subscribe to events
            if (TowerLevelManager.Instance != null)
            {
                TowerLevelManager.Instance.OnTowerLevelUp += OnTowerLevelUp;
            }
        }
        
        private float GetGradeMultiplier(GradeType grade, string type)
        {
            if (GameDataSO.Instance == null) return 1f;
            
            foreach (var unitGrade in GameDataSO.Instance.UnitGrades)
            {
                if (unitGrade.Grade == grade)
                {
                    if (type == "damage")
                        return unitGrade.DamageMultiplier;
                    else if (type == "range")
                        return unitGrade.RangeMultiplier;
                    else if (type == "speed")
                        return unitGrade.AttackSpeedMultiplier;
                }
            }
            
            return 1f;
        }
        
        private string ExtractTowerType(string name)
        {
            if (name.Contains("Archer")) return "Archer";
            if (name.Contains("WizardTower")) return "WizardTower";
            if (name.Contains("Wizard")) return "Wizard";
            if (name.Contains("Laser")) return "Laser";
            if (name.Contains("MageTower")) return "WizardTower";
            if (name.Contains("Mage")) return "Wizard";
            return "";
        }
        
        private void OnTowerLevelUp(string type, int level)
        {
            if (type == TowerType && unitAbility != null)
            {
                unitAbility.OnLevelUp(type, level);
            }
        }
        
        public Color GetGradeColor(GradeType grade)
        {
            switch (grade)
            {
                case GradeType.Common: return Color.white;
                case GradeType.Uncommon: return new Color(0.3f, 1f, 0.3f);
                case GradeType.Rare: return Color.blue;
                case GradeType.Epic: return Color.magenta;
                case GradeType.Legendary: return Color.yellow;
                default: return Color.white;
            }
        }
        
        public Color GetGradeColor()
        {
            return GetGradeColor(grade);
        }
        
        public void SetRareColor(Color color)
        {
            if (unitUI != null) unitUI.SetRareColor(color);
        }
        
        public void RestoreGradeColor()
        {
            if (unitUI != null) unitUI.RestoreGradeColor();
        }
        
        public void ShowAttackEffect()
        {
            if (unitUI != null) unitUI.ShowAttackEffect();
        }
        
        public bool IsDragging()
        {
            return unitDrag != null && unitDrag.IsDragging();
        }
        
        public void OnDragStart()
        {
            if (unitDrag != null) unitDrag.OnDragStart();
        }
        
        public void OnDragEnd()
        {
            if (unitDrag != null) unitDrag.OnDragEnd();
        }
        
        private void OnDestroy()
        {
            if (TowerLevelManager.Instance != null)
            {
                TowerLevelManager.Instance.OnTowerLevelUp -= OnTowerLevelUp;
            }
        }
    }
}
