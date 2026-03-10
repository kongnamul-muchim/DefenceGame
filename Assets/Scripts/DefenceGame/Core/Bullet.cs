using UnityEngine;

namespace DefenceGame.Core
{
    public class Bullet : MonoBehaviour
    {
        [Header("Settings")]
        public float speed = 10f;
        public float lifetime = 5f;
        [Tooltip("스프라이트 기본 방향 보정 (Arrow는 -90)")]
        public float rotationOffset = 0f;
        
        private Enemy target;
        private float damage;
        private Vector3 direction;
        private float spawnTime;
        private bool isMoving = false;
        
        public void Initialize(Enemy targetEnemy, float damageAmount)
        {
            target = targetEnemy;
            damage = damageAmount;
            spawnTime = Time.time;
            
            if (target != null)
            {
                // Calculate direction to target
                direction = (target.transform.position - transform.position).normalized;
                isMoving = true;
            }
            else
            {
                // No target, destroy immediately
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            if (!isMoving) return;

            // Check lifetime
            if (Time.time - spawnTime > lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // Update direction to follow target if still alive
            if (target != null && target.currentHealth > 0)
            {
                direction = (target.transform.position - transform.position).normalized;

                // Rotate bullet to face target (with sprite offset)
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

                // Check if hit target
                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (distanceToTarget < 0.5f)
                {
                    HitTarget();
                    return;
                }
            }

            // Move bullet
            transform.position += direction * speed * Time.deltaTime;
        }
        
        private void HitTarget()
        {
            if (target != null)
            {
                target.TakeDamage(damage);
            }
            
            Destroy(gameObject);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && enemy == target)
            {
                HitTarget();
            }
        }
    }
}