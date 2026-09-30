using UnityEngine;
using System.Collections;

namespace DefenceGame.Core
{
    public class UnitUI : MonoBehaviour
    {
        [Header("Visual")]
        public bool showAttackRange = true;
        public Color rangeColor = Color.green;
        public Color attackColor = Color.red;

        [Header("Hover Effect")]
        public float hoverScale = 1.1f;
        public float hoverDuration = 0.1f;
        public float hoverDetectionRadius = 0.5f;

        private LineRenderer rangeLineRenderer;
        private Camera mainCamera;
        private Vector3 originalScale;
        private bool isHovered = false;
        private SpriteRenderer spriteRenderer;
        private SpriteRenderer rareColorRenderer;
        private Unit unit;

        private void Awake()
        {
            unit = GetComponent<Unit>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            mainCamera = Camera.main;
            originalScale = transform.localScale;

            Transform rareColorTransform = transform.Find("RareColor");
            if (rareColorTransform != null)
            {
                rareColorRenderer = rareColorTransform.GetComponent<SpriteRenderer>();
            }

            SetupRangeVisualization();
        }

        private void SetupRangeVisualization()
        {
            if (!showAttackRange) return;

            rangeLineRenderer = GetComponent<LineRenderer>();
            if (rangeLineRenderer == null)
            {
                rangeLineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            if (rangeLineRenderer == null) return;

            rangeLineRenderer.startWidth = 0.05f;
            rangeLineRenderer.endWidth = 0.05f;
            rangeLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            rangeLineRenderer.startColor = rangeColor;
            rangeLineRenderer.endColor = rangeColor;
            rangeLineRenderer.positionCount = 50;
            rangeLineRenderer.useWorldSpace = false;
            rangeLineRenderer.loop = true;
            rangeLineRenderer.enabled = false;

            DrawRangeCircle();
        }

        private void DrawRangeCircle()
        {
            if (rangeLineRenderer == null || unit == null) return;

            for (int i = 0; i < 50; i++)
            {
                float angle = i * Mathf.PI * 2 / 50;
                rangeLineRenderer.SetPosition(i, new Vector3(
                    Mathf.Cos(angle) * unit.range,
                    Mathf.Sin(angle) * unit.range, 0));
            }
        }

        public void UpdateVisualization()
        {
            DrawRangeCircle();
        }

        public void CheckMouseHover()
        {
            if (mainCamera == null) return;
            if (unit != null && unit.IsDragging()) return;

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = transform.position.z;

            float distanceToMouse = Vector2.Distance(transform.position, mouseWorldPos);
            bool shouldHover = distanceToMouse <= hoverDetectionRadius;

            if (shouldHover && !isHovered)
            {
                isHovered = true;
                StartCoroutine(ScaleUnit(originalScale * hoverScale));
                if (rangeLineRenderer != null) rangeLineRenderer.enabled = true;
            }
            else if (!shouldHover && isHovered)
            {
                isHovered = false;
                StartCoroutine(ScaleUnit(originalScale));
                if (rangeLineRenderer != null) rangeLineRenderer.enabled = false;
            }
        }

        private IEnumerator ScaleUnit(Vector3 targetScale)
        {
            Vector3 startScale = transform.localScale;
            float elapsedTime = 0f;

            while (elapsedTime < hoverDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsedTime / hoverDuration);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
        }

        public void ShowAttackEffect()
        {
            if (rangeLineRenderer != null)
            {
                StartCoroutine(FlashRangeColor());
            }
        }

        private IEnumerator FlashRangeColor()
        {
            rangeLineRenderer.startColor = attackColor;
            rangeLineRenderer.endColor = attackColor;

            yield return new WaitForSeconds(0.1f);

            if (rangeLineRenderer != null)
            {
                rangeLineRenderer.startColor = rangeColor;
                rangeLineRenderer.endColor = rangeColor;
            }
        }

        public void SetGradeColor(Color color)
        {
            if (spriteRenderer != null) spriteRenderer.color = color;
            if (rareColorRenderer != null) rareColorRenderer.color = color;
        }

        public void SetRareColor(Color color)
        {
            if (rareColorRenderer != null) rareColorRenderer.color = color;
        }

        public void RestoreGradeColor()
        {
            if (unit == null) return;
            Color originalColor = unit.GetGradeColor();
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            if (rareColorRenderer != null) rareColorRenderer.color = originalColor;
        }

        public void FaceTarget(Vector3 targetPosition)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = targetPosition.x > transform.position.x;
            }
        }

        public bool IsHovered()
        {
            return isHovered;
        }
    }
}
