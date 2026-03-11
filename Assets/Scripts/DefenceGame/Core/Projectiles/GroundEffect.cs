using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DefenceGame.Core
{
    public class GroundEffect : MonoBehaviour
    {
        [Header("Ground Effect Settings")]
        public float damagePerSecond = 10f; // 초당 데미지
        public float duration = 1.3f; // 지속 시간 (기본 1.3초)
        public float radius = 1.5f; // 영향 범위
        public float tickInterval = 0.3f; // 데미지 적용 간격
        
        [Header("Particle System")]
        public ParticleSystem effectParticles; // 파티클 시스템
        
        private float elapsedTime = 0f;
        private float tickTimer = 0f;
        private List<Enemy> enemiesInEffect = new List<Enemy>();
        private bool isDestroying = false;
        
        private void Awake()
        {
            // Rigidbody2D 추가 (Trigger 감지를 위해 필요)
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                rb.isKinematic = true;
            }
            
            // Trigger Collider 추가
            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col == null)
            {
                col = gameObject.AddComponent<CircleCollider2D>();
            }
            col.isTrigger = true;
            col.radius = radius;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && !enemiesInEffect.Contains(enemy))
            {
                enemiesInEffect.Add(enemy);
                Debug.Log($"[GroundEffect] Enemy entered: {enemy.enemyName}");
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && enemiesInEffect.Contains(enemy))
            {
                enemiesInEffect.Remove(enemy);
                Debug.Log($"[GroundEffect] Enemy exited: {enemy.enemyName}");
            }
        }
        
        public void Initialize(float customDuration, float customDamage, float customRadius)
        {
            duration = customDuration;
            damagePerSecond = customDamage;
            radius = customRadius;
            
            // 파티클 시스템 설정
            SetupParticleSystem();
            
            Debug.Log($"[GroundEffect] Initialized - Duration: {duration}s, DPS: {damagePerSecond}, Radius: {radius}");
        }
        
        private void SetupParticleSystem()
        {
            // 파티클 시스템 찾기 또는 생성
            if (effectParticles == null)
            {
                effectParticles = GetComponent<ParticleSystem>();
            }
            
            if (effectParticles != null)
            {
                // 파티클 설정
                var main = effectParticles.main;
                main.duration = duration;
                main.startLifetime = duration;
                main.startSize = radius * 2f;
                
                // 파티클 시작
                effectParticles.Play();
            }
            else
            {
                Debug.LogWarning("[GroundEffect] No ParticleSystem found!");
            }
            
            // 지속시간 후 자동 제거
            StartCoroutine(DestroyAfterDuration());
        }
        
        private IEnumerator DestroyAfterDuration()
        {
            yield return new WaitForSeconds(duration);
            
            if (!isDestroying)
            {
                isDestroying = true;
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            elapsedTime += Time.deltaTime;
            tickTimer += Time.deltaTime;
            
            // 틱 데미지 적용
            if (tickTimer >= tickInterval)
            {
                ApplyTickDamage();
                tickTimer = 0f;
            }
        }
        
        private void ApplyTickDamage()
        {
            float damage = damagePerSecond * tickInterval;
            
            // enemiesInEffect 리스트의 적들에게 데미지
            for (int i = enemiesInEffect.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemiesInEffect[i];
                if (enemy != null && enemy.currentHealth > 0)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"[GroundEffect] Dealt {damage:F1} damage to {enemy.enemyName}");
                }
                else
                {
                    // Remove dead or null enemies
                    enemiesInEffect.RemoveAt(i);
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
