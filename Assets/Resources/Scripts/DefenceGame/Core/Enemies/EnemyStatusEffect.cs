using UnityEngine;
using System.Collections.Generic;

namespace DefenceGame.Core
{
    /// <summary>
    /// 적의 상태 효과를 관리하는 컴포넌트
    /// 단일 책임: 상태 효과(버프/디버프) 관리
    /// </summary>
    public class EnemyStatusEffect : MonoBehaviour
    {
        [Header("Visual")]
        private SpriteRenderer spriteRenderer;

        private Dictionary<MonoBehaviour, float> slowSources = new Dictionary<MonoBehaviour, float>();
        private float currentSlowMultiplier = 1f;

        public float CurrentSlowMultiplier => currentSlowMultiplier;

        public System.Action<float> OnSpeedChanged;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void ApplySlowEffect(float slowPercent, MonoBehaviour source)
        {
            if (source == null) return;
            if (slowSources.ContainsKey(source)) return;

            slowSources.Add(source, slowPercent);
            UpdateSlowMultiplier();

            // Visual effect
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.3f, 0.3f, 1f, 1f);
            }
        }

        public void RemoveSlowEffect(MonoBehaviour source)
        {
            if (source == null) return;

            slowSources.Remove(source);
            UpdateSlowMultiplier();

            if (slowSources.Count == 0 && spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
        }

        private void UpdateSlowMultiplier()
        {
            float maxSlow = 0f;
            foreach (var kvp in slowSources)
            {
                maxSlow = Mathf.Max(maxSlow, kvp.Value);
            }

            currentSlowMultiplier = Mathf.Max(0f, 1f - maxSlow);
            OnSpeedChanged?.Invoke(currentSlowMultiplier);
        }

        public void ResetEffects()
        {
            slowSources.Clear();
            currentSlowMultiplier = 1f;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
        }
    }
}
