using UnityEngine;
using System.Collections.Generic;

namespace DefenceGame.Core
{
    public class UnitAttack : MonoBehaviour
    {
        [Header("Attack Settings")]
        public float attackPower = 10f;
        public float attackSpeed = 1f;
        public float range = 5f;
        public bool isRanged = true;
        
        [Header("Projectile")]
        public GameObject bulletPrefab;
        public Transform attackPoint;
        
        private float lastAttackTime;
        private Enemy targetEnemy;
        private Unit unit;
        private UnitAbility ability;
        
        private void Awake()
        {
            unit = GetComponent<Unit>();
            ability = GetComponent<UnitAbility>();
            if (attackPoint == null)
            {
                attackPoint = transform.Find("AttackPoint") ?? transform;
            }
        }
        
        public void UpdateAttack()
        {
            FindAndAttackTarget();
        }
        
        private void FindAndAttackTarget()
        {
            // Clear target if dead or out of range
            if (targetEnemy != null && (targetEnemy.currentHealth <= 0 || 
                Vector3.Distance(transform.position, targetEnemy.transform.position) > range))
            {
                targetEnemy = null;
            }
            
            // Find new target if needed
            if (targetEnemy == null)
            {
                targetEnemy = FindClosestEnemy();
            }
            
            // Attack if we have a target
            if (targetEnemy != null && Vector3.Distance(transform.position, targetEnemy.transform.position) <= range)
            {
                Attack(targetEnemy);
            }
        }
        
        private Enemy FindClosestEnemy()
        {
            Enemy closestEnemy = null;
            float closestDistance = range;
            
            Enemy[] allEnemies = GameObject.FindObjectsOfType<Enemy>();
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy == null || enemy.currentHealth <= 0) continue;
                
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= range && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
            
            return closestEnemy;
        }
        
        private void Attack(Enemy enemy)
        {
            if (Time.time - lastAttackTime < 1f / attackSpeed) return;
            
            lastAttackTime = Time.time;
            FaceTarget(enemy.transform.position);
            
            if (isRanged && bulletPrefab != null)
            {
                ExecuteRangedAttack(enemy);
            }
            else
            {
                enemy.TakeDamage(attackPower);
            }
            
            unit?.ShowAttackEffect();
        }
        
        private void ExecuteRangedAttack(Enemy enemy)
        {
            string towerType = unit != null ? unit.TowerType : "";
            
            // Archer: MultiShot
            if (towerType == "Archer" && ability != null && ability.multiShotCount > 1)
            {
                SpawnMultipleBullets(enemy, ability.multiShotCount);
            }
            // Laser: ChainAttack
            else if (towerType == "Laser" && ability != null && ability.chainAttackCount > 0)
            {
                float damageMultiplier = ability.GetChainDamageMultiplier();
                SpawnChainBullet(enemy, ability.chainAttackCount, damageMultiplier);
            }
            // Wizard: GroundEffect (10% chance)
            else if (towerType == "Wizard" && ability != null && ability.ShouldTriggerGroundEffect())
            {
                ability.SpawnGroundEffect(enemy.transform.position);
                SpawnBullet(enemy);
            }
            else
            {
                SpawnBullet(enemy);
            }
        }
        
        private void SpawnBullet(Enemy target)
        {
            if (bulletPrefab == null || attackPoint == null) return;
            
            GameObject bullet = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Initialize(target, attackPower, 0, 0);
            }
        }
        
        private void SpawnMultipleBullets(Enemy target, int count)
        {
            if (bulletPrefab == null || count <= 1) return;
            
            float totalAngle = 60f;
            float angleStep = totalAngle / (count - 1);
            float startAngle = -totalAngle / 2f;
            
            Vector3 targetDirection = (target.transform.position - attackPoint.position).normalized;
            float baseAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            
            for (int i = 0; i < count; i++)
            {
                float currentAngle = baseAngle + startAngle + (angleStep * i);
                float radian = currentAngle * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0);
                
                GameObject bullet = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);
                Bullet bulletComponent = bullet.GetComponent<Bullet>();
                if (bulletComponent != null)
                {
                    bulletComponent.InitializeWithDirection(target, attackPower, 0, 0, direction);
                }
            }
        }
        
        private void SpawnChainBullet(Enemy target, int chainCount, float damageMultiplier = 1.0f)
        {
            if (bulletPrefab == null || target == null) return;
            
            GameObject bullet = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.InitializeChain(target, attackPower, chainCount, damageMultiplier);
            }
        }
        
        private void FaceTarget(Vector3 targetPosition)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) return;
            
            sr.flipX = targetPosition.x > transform.position.x;
        }
        
        public Enemy GetCurrentTarget()
        {
            return targetEnemy;
        }
        
        public void ClearTarget()
        {
            targetEnemy = null;
        }
    }
}
