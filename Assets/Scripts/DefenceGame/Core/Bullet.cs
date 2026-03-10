using UnityEngine;

namespace DefenceGame.Core
{
    public class Bullet : MonoBehaviour
    {
        [Header("Settings")]
        public float speed = 10f;
        public float lifetime = 5f;
        
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
            
            // Move bullet
            transform.position += direction * speed * Time.deltaTime;
            
            // Check if hit target
            if (target != null && target.currentHealth > 0)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (distanceToTarget < 0.5f)
                {
                    HitTarget();
                }
            }
            else
            {
                // Target dead or null, destroy bullet
                Destroy(gameObject);
            }
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