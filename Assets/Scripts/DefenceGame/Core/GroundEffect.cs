using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DefenceGame.Core
{
    public class GroundEffect : MonoBehaviour
    {
        [Header("Ground Effect Settings")]
        public float damagePerSecond = 10f; // 초당 데미지
        public float duration = 3f; // 지속 시간
        public float radius = 1f; // 영향 범위
        public float tickInterval = 0.5f; // 데미지 적용 간격
        
        [Header("Visual")]
        public SpriteRenderer effectRenderer;
        public Color effectColor = new Color(1f, 0.3f, 0.3f, 0.5f); // 붉은색 반투명
        public float fadeOutDuration = 0.5f; // 사라질 때 페이드 아웃 시간
        
        private float elapsedTime = 0f;
        private float tickTimer = 0f;
        private HashSet<Enemy> enemiesInEffect = new HashSet<Enemy>();
        
        public void Initialize(float customDuration, float customDamage, float customRadius)
        {
            duration = customDuration;
            damagePerSecond = customDamage;
            radius = customRadius;
            
            // 시각적 설정
            SetupVisuals();
            
            Debug.Log($"[GroundEffect] Initialized - Duration: {duration}s, DPS: {damagePerSecond}, Radius: {radius}");
        }
        
        private void SetupVisuals()
        {
            // 트랜스폼 설정
            transform.localScale = Vector3.one * radius * 2f;
            
            // 스프라이트 렌더러 설정
            if (effectRenderer == null)
            {
                effectRenderer = GetComponent<SpriteRenderer>();
                if (effectRenderer == null)
                {
                    effectRenderer = gameObject.AddComponent<SpriteRenderer>();
                }
            }
            
            // 원형 스프라이트 생성 또는 기본 원 사용
            effectRenderer.color = effectColor;
            effectRenderer.sortingOrder = -1; // 유닛 뒤에 표시
        }
        
        private void Update()
        {
            elapsedTime += Time.deltaTime;
            tickTimer += Time.deltaTime;
            
            // 지속 시간 체크
            if (elapsedTime >= duration)
            {
                StartCoroutine(FadeOutAndDestroy());
                enabled = false;
                return;
            }
            
            // 틱 데미지 적용
            if (tickTimer >= tickInterval)
            {
                ApplyTickDamage();
                tickTimer = 0f;
            }
            
            // 범위 내 적 체크 (시각적 효과용)
            UpdateEnemiesInRange();
        }
        
        private void ApplyTickDamage()
        {
            float damage = damagePerSecond * tickInterval;
            
            // 범위 내 모든 적에게 데미지
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (Collider2D col in colliders)
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null && enemy.currentHealth > 0)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"[GroundEffect] Dealt {damage} damage to {enemy.enemyName}");
                }
            }
        }
        
        private void UpdateEnemiesInRange()
        {
            // 현재 범위 내 적들 업데이트
            enemiesInEffect.Clear();
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (Collider2D col in colliders)
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null && enemy.currentHealth > 0)
                {
                    enemiesInEffect.Add(enemy);
                }
            }
        }
        
        private IEnumerator FadeOutAndDestroy()
        {
            if (effectRenderer != null)
            {
                Color startColor = effectRenderer.color;
                float fadeTimer = 0f;
                
                while (fadeTimer < fadeOutDuration)
                {
                    fadeTimer += Time.deltaTime;
                    float alpha = Mathf.Lerp(startColor.a, 0f, fadeTimer / fadeOutDuration);
                    effectRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                    yield return null;
                }
            }
            
            Destroy(gameObject);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
