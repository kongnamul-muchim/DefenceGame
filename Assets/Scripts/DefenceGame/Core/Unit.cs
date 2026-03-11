using System.Collections;
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
        public bool isRanged = true; // true = use bullet, false = melee
        
        [Header("Components")]
        public SpriteRenderer spriteRenderer;
        public Transform attackPoint; // Where bullets spawn from
        
        [Header("Projectile")]
        public GameObject bulletPrefab;
        
        [Header("Visual")]
        public bool showAttackRange = true;
        public Color rangeColor = Color.green;
        public Color attackColor = Color.red;
        public SpriteRenderer rareColorRenderer; // 그림자 색상용 (Inspector에서 RareColor 연결)
        
        [Header("Hover Effect")]
        public float hoverScale = 1.1f; // 마우스 호버 시 커지는 정도
        public float hoverDuration = 0.1f; // 크기 변화 시간
        public float hoverDetectionRadius = 0.5f; // 마우스 감지 반경 (Collider 대신 사용)
        
        private float lastAttackTime;
        private Enemy targetEnemy;
        private LineRenderer rangeLineRenderer;
        
        // Drag variables
        private bool isDragging = false;
        private Camera mainCamera;
        
        // 원래 등급 색상 저장
        private Color gradeColor;
        private Color rareOriginalColor; // RareColor 원래 색상 저장
        
        // Hover
        private Vector3 originalScale;
        private bool isHovered = false;
        
        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            
            // RareColorRenderer 자동 찾기 (자식 오브젝트에서)
            if (rareColorRenderer == null)
            {
                Transform rareColorTransform = transform.Find("RareColor");
                if (rareColorTransform != null)
                {
                    rareColorRenderer = rareColorTransform.GetComponent<SpriteRenderer>();
                }
            }
            
            mainCamera = Camera.main;
            
            // Find attack point if not assigned
            if (attackPoint == null)
            {
                attackPoint = transform.Find("AttackPoint");
                if (attackPoint == null)
                {
                    attackPoint = transform; // Use unit position if no attack point
                }
            }
            
            // Setup range visualization
            SetupRangeVisualization();
            
            // Add collider for mouse detection if not present
            if (GetComponent<Collider2D>() == null)
            {
                gameObject.AddComponent<BoxCollider2D>();
            }
            
            // Save original scale for hover effect
            originalScale = transform.localScale;
        }
        
        private void SetupRangeVisualization()
        {
            if (!showAttackRange) return;
            
            rangeLineRenderer = gameObject.AddComponent<LineRenderer>();
            rangeLineRenderer.startWidth = 0.05f;
            rangeLineRenderer.endWidth = 0.05f;
            rangeLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            rangeLineRenderer.startColor = rangeColor;
            rangeLineRenderer.endColor = rangeColor;
            rangeLineRenderer.positionCount = 50;
            rangeLineRenderer.useWorldSpace = false;
            rangeLineRenderer.loop = true;
            
            DrawRangeCircle();
        }
        
        private void DrawRangeCircle()
        {
            if (rangeLineRenderer == null) return;
            
            for (int i = 0; i < 50; i++)
            {
                float angle = i * Mathf.PI * 2 / 50;
                float x = Mathf.Cos(angle) * range;
                float y = Mathf.Sin(angle) * range;
                rangeLineRenderer.SetPosition(i, new Vector3(x, y, 0));
            }
        }
        
        private void Update()
        {
            // 드래그 중에는 공격하지 않음
            if (!isDragging)
            {
                FindAndAttackTarget();
            }
            
            // 마우스 호버 체크 (Collider 대신 거리 기반)
            CheckMouseHover();
        }
        
        // 마우스 위치 체크하여 호버 상태 업데이트
        private void CheckMouseHover()
        {
            if (mainCamera == null) return;
            
            // 마우스 위치를 월드 좌표로 변환
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = transform.position.z; // Z축은 유닛과 동일하게
            
            // 유닛과 마우스 사이의 거리 계산
            float distanceToMouse = Vector2.Distance(transform.position, mouseWorldPos);
            
            // 감지 반경 내에 마우스가 있고 드래그 중이 아닐 때
            bool shouldHover = distanceToMouse <= hoverDetectionRadius && !isDragging;
            
            if (shouldHover && !isHovered)
            {
                // 마우스가 들어옴
                isHovered = true;
                StopCoroutine("ScaleUnit");
                StartCoroutine(ScaleUnit(originalScale * hoverScale));
            }
            else if (!shouldHover && isHovered)
            {
                // 마우스가 나감
                isHovered = false;
                StopCoroutine("ScaleUnit");
                StartCoroutine(ScaleUnit(originalScale));
            }
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
            switch (grade)
            {
                case GradeType.Common:
                    gradeColor = Color.white;
                    break;
                case GradeType.Uncommon:
                    gradeColor = new Color(0.3f, 1f, 0.3f); // 연한 초록색
                    break;
                case GradeType.Rare:
                    gradeColor = Color.blue;
                    break;
                case GradeType.Epic:
                    gradeColor = Color.magenta;
                    break;
                case GradeType.Legendary:
                    gradeColor = Color.yellow;
                    break;
            }
            
            // Unit 본체 색상 설정
            if (spriteRenderer != null)
            {
                spriteRenderer.color = gradeColor;
            }
            
            // RareColor(그림자) 색상도 동일하게 설정
            if (rareColorRenderer != null)
            {
                rareColorRenderer.color = gradeColor;
                rareOriginalColor = gradeColor; // 원래 색상 저장
            }
        }
        
        // 드래그 시 RareColor 색상 변경
        public void SetRareColor(Color color)
        {
            if (rareColorRenderer != null)
            {
                rareColorRenderer.color = color;
            }
        }
        
        // RareColor 원래 색상으로 복귀
        public void RestoreRareColor()
        {
            if (rareColorRenderer != null)
            {
                rareColorRenderer.color = rareOriginalColor;
            }
        }
        
        // 외부에서 색상 복귀 시 사용
        public void RestoreGradeColor()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = gradeColor;
            }
            RestoreRareColor();
        }
        
        public Color GetGradeColor()
        {
            return gradeColor;
        }
        
        // 드래그 시작 (UnitDragSystem에서 호출)
        public void OnDragStart()
        {
            isDragging = true;
        }
        
        // 드래그 종료 (UnitDragSystem에서 호출)
        public void OnDragEnd()
        {
            isDragging = false;
        }
        
        // 크기 변화 코루틴
        private IEnumerator ScaleUnit(Vector3 targetScale)
        {
            Vector3 startScale = transform.localScale;
            float elapsedTime = 0f;
            
            while (elapsedTime < hoverDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / hoverDuration;
                // 부드러운 보간
                t = Mathf.SmoothStep(0, 1, t);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            transform.localScale = targetScale;
        }
        
        private void OnDisable()
        {
            // 드래그 중에 비활성화되면 취소
            if (isDragging)
            {
                UnitDragSystem.Instance?.CancelDrag();
                isDragging = false;
            }
        }
        
        private void FindAndAttackTarget()
        {
            // Clear target if dead
            if (targetEnemy != null && targetEnemy.currentHealth <= 0)
            {
                targetEnemy = null;
            }
            
            // Clear target if out of range
            if (targetEnemy != null)
            {
                float distance = Vector3.Distance(transform.position, targetEnemy.transform.position);
                if (distance > range)
                {
                    targetEnemy = null;
                }
            }
            
            // Find new target if needed
            if (targetEnemy == null)
            {
                targetEnemy = FindClosestEnemy();
            }
            
            // Attack if we have a target
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
            Enemy closestEnemy = null;
            float closestDistance = range;
            
            // Find all enemies in the scene
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
            
            // Face the target
            FaceTarget(enemy.transform.position);
            
            if (isRanged && bulletPrefab != null)
            {
                // Ranged attack - spawn bullet
                SpawnBullet(enemy);
            }
            else
            {
                // Melee attack - instant damage
                enemy.TakeDamage(attackPower);
            }
            
            // Visual feedback
            ShowAttackEffect();
            
            Debug.Log($"[{grade}] {unitName} attacks {enemy.enemyName} for {attackPower} damage!");
        }
        
        private void SpawnBullet(Enemy target)
        {
            if (bulletPrefab == null) return;
            
            GameObject bullet = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Initialize(target, attackPower);
            }
        }
        
        private void FaceTarget(Vector3 targetPosition)
        {
            if (spriteRenderer == null) return;
            
            // Flip sprite based on target position
            if (targetPosition.x > transform.position.x)
            {
                spriteRenderer.flipX = true;  // Face right
            }
            else
            {
                spriteRenderer.flipX = false; // Face left
            }
        }
        
        private void ShowAttackEffect()
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
    }
}