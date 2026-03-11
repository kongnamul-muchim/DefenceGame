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
        private HashSet<Enemy> enemiesInEffect = new HashSet<Enemy>();
        private bool isDestroying = false;
        
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
            
            // 범위 내 모든 적에게 데미지 (trigger와 non-trigger 모두 체크)
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (Collider2D col in colliders)
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null && enemy.currentHealth > 0)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"[GroundEffect] Dealt {damage:F1} damage to {enemy.enemyName}");
                }
            }
            
            // Trigger colliders 체크 (별도로 체크 필요)
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true;
            List<Collider2D> triggerColliders = new List<Collider2D>();
            Physics2D.OverlapCircle(transform.position, radius, filter, triggerColliders);
            
            foreach (Collider2D col in triggerColliders)
            {
                // 이미 위에서 처리된 collider는 스킵
                if (colliders.Length > 0 && System.Array.Exists(colliders, c => c == col))
                    continue;
                    
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null && enemy.currentHealth > 0)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"[GroundEffect] Dealt {damage:F1} damage to {enemy.enemyName} (trigger)");
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
