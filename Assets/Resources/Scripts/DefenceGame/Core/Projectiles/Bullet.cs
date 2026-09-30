using UnityEngine;

namespace DefenceGame.Core
{
    public class Bullet : MonoBehaviour
    {
        [Header("Settings")]
        public float speed = 10f;
        public float lifetime = 1f;
        [Tooltip("스프라이트 기본 방향 보정 (Arrow는 -90)")]
        public float rotationOffset = 0f;

        [Header("Effects")]
        [Tooltip("히트 이펙트 프리팹 (ParticleSystem)")]
        public GameObject hitEffectPrefab;

        private Enemy target;
        private float damage;
        private Vector3 direction;
        private float spawnTime;
        private bool isMoving = false;
        private bool isHoming = true; // 자동 추적 여부 (Archer는 false)
        private Unit attacker; // 공격한 유닛 (경험치 획득용)

        // Pierce (관통) 기능
        private int pierceCount = 0; // 관통 가능 횟수 (0 = 관통 없음)
        private int pierceRemaining = 0; // 남은 관통 횟수
        private System.Collections.Generic.List<Enemy> hitEnemies = new System.Collections.Generic.List<Enemy>(); // 이미 맞은 적 목록

        // Area damage (Mage 광역 공격)
        private int areaDamageRadius = 0; // 광역 데미지 반경 (0 = 없음, 1 = 1칸, 2 = 2칸)
        private LineRenderer areaRangeRenderer; // 광역 범위 시각화

        private void Awake()
        {
            // Rigidbody2D가 없으면 추가 (OnTriggerEnter2D 작동을 위해 필수)
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            // Collider2D가 없으면 CircleCollider2D 추가
            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
            {
                CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
                circleCol.isTrigger = true;
                circleCol.radius = 0.2f;
            }
            else if (!col.isTrigger)
            {
                col.isTrigger = true;
            }
        }

        public void Initialize(Enemy targetEnemy, float damageAmount, int pierce = 0, int areaRadius = 0, Unit attackerUnit = null)
        {
            target = targetEnemy;
            damage = damageAmount;
            pierceCount = pierce;
            pierceRemaining = pierce;
            areaDamageRadius = areaRadius;
            attacker = attackerUnit;
            hitEnemies.Clear();
            spawnTime = Time.time;

            // Debug.Log($"[Bullet] Initialized - pierce={pierce}, areaRadius={areaRadius}, damage={damageAmount}");

            // 광역 범위 시각화 설정
            if (areaDamageRadius > 0)
            {
                // Debug.Log($"[Bullet] Setting up area visualization with radius {areaDamageRadius}");
                SetupAreaVisualization();
            }

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

        // 부채꼴 발사용 초기화 (자동추적 안함)
        public void InitializeWithDirection(Enemy targetEnemy, float damageAmount, int pierce, int areaRadius, Vector3 moveDirection, Unit attackerUnit = null)
        {
            target = targetEnemy;
            damage = damageAmount;
            pierceCount = pierce;
            pierceRemaining = pierce;
            areaDamageRadius = areaRadius;
            attacker = attackerUnit;
            hitEnemies.Clear();
            spawnTime = Time.time;
            isHoming = false; // 자동 추적 비활성화

            // 고정 방향 설정
            direction = moveDirection.normalized;
            isMoving = true;

            // 회전 설정
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

            // 광역 범위 시각화 설정
            if (areaDamageRadius > 0)
            {
                SetupAreaVisualization();
            }
        }

        // Laser 연계 공격용
        private float chainDamageMultiplier = 1.0f; // 등급 기반 데미지 배율
        private int currentChainIndex = 0; // 현재 연계 단계 초기화
        public void InitializeChain(Enemy targetEnemy, float damageAmount, int chainCount, float damageMultiplier = 1.0f, Unit attackerUnit = null)
        {
            target = targetEnemy;
            damage = damageAmount * damageMultiplier; // 등급 기반 데미지 적용
            pierceCount = chainCount;
            pierceRemaining = chainCount;
            chainDamageMultiplier = damageMultiplier;
            currentChainIndex = 0;
            areaDamageRadius = 0;
            attacker = attackerUnit;
            hitEnemies.Clear();
            spawnTime = Time.time;

            if (target != null)
            {
                direction = (target.transform.position - transform.position).normalized;
                isMoving = true;

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetupAreaVisualization()
        {
            // Debug.Log($"[Bullet] SetupAreaVisualization called - areaDamageRadius={areaDamageRadius}");

            // 광역 공격 범위 원형 시각화
            areaRangeRenderer = gameObject.AddComponent<LineRenderer>();
            areaRangeRenderer.startWidth = 0.08f; // 더 두껍게
            areaRangeRenderer.endWidth = 0.08f;
            areaRangeRenderer.material = new Material(Shader.Find("Sprites/Default"));
            areaRangeRenderer.startColor = new Color(1, 0, 1, 0.8f); // 보라색 더 진하게
            areaRangeRenderer.endColor = new Color(1, 0, 1, 0.8f);
            areaRangeRenderer.positionCount = 50;
            areaRangeRenderer.useWorldSpace = false;
            areaRangeRenderer.loop = true;
            areaRangeRenderer.sortingOrder = 1000; // 높은 sorting order

            DrawAreaCircle();

            // Debug.Log($"[Bullet] Area visualization setup complete");
        }

        private void DrawAreaCircle()
        {
            if (areaRangeRenderer == null) return;

            float radius = areaDamageRadius * 2.5f; // 광역 범위 확대 (기존 1.5f -> 2.5f)
            for (int i = 0; i < 50; i++)
            {
                float angle = i * Mathf.PI * 2 / 50;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                areaRangeRenderer.SetPosition(i, new Vector3(x, y, 0));
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

            // 자동 추적일 때만 방향 업데이트
            if (isHoming && target != null && target.currentHealth > 0)
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
            else if (!isHoming)
            {
                // 고정 방향 발사 - 경로상의 적 체크
                CheckHitOnPath();
            }

            // Move bullet
            transform.position += direction * speed * Time.deltaTime;
        }

        // 고정 방향 발사 시 경로상의 적 체크
        private void CheckHitOnPath()
        {
            // 앞쪽으로 레이캐스트
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 0.5f);
            if (hit.collider != null)
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null && enemy.currentHealth > 0 && !hitEnemies.Contains(enemy))
                {
                    target = enemy;
                    HitTarget();
                }
            }
        }

        private void HitTarget()
        {
            if (target != null && !hitEnemies.Contains(target))
            {
                // 광역 데미지 적용
                if (areaDamageRadius > 0)
                {
                    ApplyAreaDamage();
                }
                else
                {
                    // 단일 대상 데미지 - attacker 정보 전달
                    target.TakeDamage(damage, attacker);
                }
                hitEnemies.Add(target);

                // Spawn hit effect
                SpawnHitEffect();

                // 관통/연계 체크
                if (pierceRemaining > 0)
                {
                    pierceRemaining--;
                    currentChainIndex++;
                    // 다음 타겟 찾기
                    Enemy nextTarget = FindNextTarget();
                    if (nextTarget != null)
                    {
                        // 연계 공격 시각 효과
                        ShowChainEffect(target.transform.position, nextTarget.transform.position);

                        // 연계 단계에 따른 추가 데미지 감소 (등급이 낮을수록 더 많이 감소)
                        // 예: Common(0.7) -> 0.7, Uncommon(0.85) -> 0.85, Rare(1.0) -> 1.0 유지
                        float chainDamageFactor = Mathf.Pow(chainDamageMultiplier, currentChainIndex);
                        damage *= chainDamageFactor;

                        target = nextTarget;
                        return; // 계속 진행
                    }
                }
            }

            Destroy(gameObject);
        }

        // 연계 공격 시각 효과
        private void ShowChainEffect(Vector3 from, Vector3 to)
        {
            // 라인 렌더러로 연결선 표시
            GameObject lineObj = new GameObject("ChainEffect");
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.startWidth = 0.15f;
            line.endWidth = 0.15f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = Color.cyan;
            line.endColor = Color.cyan;
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            line.sortingOrder = 100;

            // 0.3초 후 제거
            Destroy(lineObj, 0.3f);
        }

        private void ApplyAreaDamage()
        {
            // 타겟 위치에 광역 데미지 적용
            Vector3 center = target.transform.position;
            float radius = areaDamageRadius * 2.5f; // 광역 범위 확대 (기존 1.5f -> 2.5f)

            // 디버그 로그 (필요시 활성화)
            // Debug.Log($"[Area Damage] Center: {center}, Radius: {radius}, Damage: {damage}");

            // 범위 내 모든 적 찾기
            Enemy[] allEnemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            int hitCount = 0;

            foreach (Enemy enemy in allEnemies)
            {
                if (enemy == null || enemy.currentHealth <= 0) continue;

                float distance = Vector3.Distance(center, enemy.transform.position);
                if (distance <= radius)
                {
                    enemy.TakeDamage(damage, attacker);
                    hitCount++;
                }
            }

            // Debug.Log($"[Area Damage] Total {hitCount} enemies hit");
        }

        private Enemy FindNextTarget()
        {
            Enemy closestEnemy = null;
            float closestDistance = 5f; // 주변 5유닛 내에서 검색

            Enemy[] allEnemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);

            foreach (Enemy enemy in allEnemies)
            {
                if (enemy == null || enemy.currentHealth <= 0) continue;
                if (hitEnemies.Contains(enemy)) continue; // 이미 맞은 적은 제외

                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }

        private void SpawnHitEffect()
        {
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

                // Sorting Order 높게 설정하여 Tilemap 위에 표시
                ParticleSystemRenderer renderer = effect.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = 1000;
                }

                // Auto destroy after 2 seconds
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(effect, 2f);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && enemy.currentHealth > 0 && !hitEnemies.Contains(enemy))
            {
                target = enemy;
                HitTarget();
            }
        }
    }
}