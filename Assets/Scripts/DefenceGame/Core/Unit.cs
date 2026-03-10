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
        
        private float lastAttackTime;
        private Enemy targetEnemy;
        private LineRenderer rangeLineRenderer;
        
        // Drag variables
        private bool isDragging = false;
        private Camera mainCamera;
        
        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
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
                case GradeType.Uncommon:
                    spriteRenderer.color = new Color(0.3f, 1f, 0.3f); // 연한 초록색
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
        
        // Mouse Drag Methods
        private void OnMouseDown()
        {
            // 게임 오버 상태 체크
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                return;
            
            isDragging = true;
            UnitDragSystem.Instance?.StartDrag(this);
        }
        
        private void OnMouseDrag()
        {
            if (!isDragging) return;
            
            // 마우스 위치를 월드 좌표로 변환
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -mainCamera.transform.position.z;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            worldPos.z = transform.position.z;
            
            // 드래그 시스템에 위치 업데이트
            UnitDragSystem.Instance?.UpdateDragPosition(worldPos);
        }
        
        private void OnMouseUp()
        {
            if (!isDragging) return;
            
            isDragging = false;
            UnitDragSystem.Instance?.EndDrag(this);
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