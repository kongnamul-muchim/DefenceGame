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
        
        [Header("Components")]
        public SpriteRenderer spriteRenderer;
        
        private float lastAttackTime;
        private Enemy targetEnemy;
        
        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }
        
        private void Update()
        {
            FindAndAttackTarget();
        }
        
        public void Initialize(TowerData data)
        {
            id = data.Id;
            unitName = data.Name;
            attackPower = data.AttackPower;
            attackSpeed = data.AttackSpeed;
            range = data.Range;
            grade = data.Grade;
            
            // Set sprite color based on grade
            SetGradeColor();
            
            Debug.Log($"Unit initialized: {unitName}, ATK: {attackPower}, SPD: {attackSpeed}, RNG: {range}");
        }
        
        private void SetGradeColor()
        {
            if (spriteRenderer == null) return;
            
            switch (grade)
            {
                case GradeType.Common:
                    spriteRenderer.color = Color.white;
                    break;
                case GradeType.Rare:
                    spriteRenderer.color = Color.blue;
                    break;
                case GradeType.Epic:
                    spriteRenderer.color = Color.magenta;
                    break;
                case GradeType.Legendary:
                    spriteRenderer.color = Color.yellow;
                    break;
            }
        }
        
        private void FindAndAttackTarget()
        {
            // TODO: Implement targeting and attack logic
            // For now, just find closest enemy
            if (targetEnemy == null || targetEnemy.currentHealth <= 0)
            {
                targetEnemy = FindClosestEnemy();
            }
            
            if (targetEnemy != null)
            {
                float distance = Vector3.Distance(transform.position, targetEnemy.transform.position);
                if (distance <= range)
                {
                    Attack(targetEnemy);
                }
            }
        }
        
        private Enemy FindClosestEnemy()
        {
            // TODO: Implement enemy finding logic
            // For now, return null
            return null;
        }
        
        private void Attack(Enemy enemy)
        {
            if (Time.time - lastAttackTime < 1f / attackSpeed) return;
            
            lastAttackTime = Time.time;
            enemy.TakeDamage(attackPower);
            
            Debug.Log($"{unitName} attacks {enemy.enemyName} for {attackPower} damage!");
        }
    }
}