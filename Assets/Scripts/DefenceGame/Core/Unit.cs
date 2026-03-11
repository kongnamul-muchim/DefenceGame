using System.Collections;
using System.Collections.Generic;
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
        
        // Special Abilities (특수 능력)
        // Archer
        private int multiShotCount = 1; // 투사체 발사 개수 (기본 1)
        
        // Mage
        private float groundEffectDuration = 0f; // 지속 피해 바닥 지속 시간
        private GameObject groundEffectPrefab; // 바닥 효과 프리팹
        
        // MageTower
        private float rangeIncreaseValue = 0f; // 사거리 증가 값
        private float attackIncreaseValue = 0f; // 공격력 증가 값
        
        // Laser
        private int chainAttackCount = 0; // 연계 공격 횟수 (주변 전이)
        private float chainAttackRange = 3f; // 연계 공격 범위
        
        // Deprecated 능력들 (이전 코드와의 호환성)
        private int pierceCount = 0; // [Deprecated] 관통 횟수
        private int areaDamageRadius = 0; // [Deprecated] 광역 데미지 반경
        private float attackBuffValue = 0f; // [Deprecated] 공격력 버프 값
        private float speedBuffValue = 0f; // [Deprecated] 공격속도 버프 값
        private float speedIncreaseValue = 0f; // [Deprecated] 공격속도 증가 값
        private float slowEffectValue = 0f; // [Deprecated] 이동속도 감소 값
        
        private string towerType = ""; // 타워 타입 (Archer, Mage, MageTower, Laser)
        
        // MageTower 버프/디버프 관련
        private float buffRange = 3f; // MageTower 버프 범위 (3x3)
        private LineRenderer buffRangeRenderer; // 버프 범위 시각화
        private SpriteRenderer buffAreaRenderer; // 버프 영역 표시 (반투명 사각형)
        
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
            
            // 버프 범위 시각화는 Initialize에서 설정 (towerType 설정 후)
        }
        
        private void SetupBuffRangeVisualization()
        {
            // MageTower만 버프 범위 시각화
            if (towerType != "MageTower") 
            {
                return;
            }
            
            // 기존 LineRenderer 확인
            if (buffRangeRenderer == null)
            {
                buffRangeRenderer = GetComponent<LineRenderer>();
                // rangeLineRenderer와 같지 않은지 확인
                if (buffRangeRenderer == rangeLineRenderer)
                {
                    buffRangeRenderer = null;
                }
            }
            
            // 없으면 새로 추가
            if (buffRangeRenderer == null)
            {
                buffRangeRenderer = gameObject.AddComponent<LineRenderer>();
            }
            
            // 여전히 null이면 오류 로그하고 리턴
            if (buffRangeRenderer == null)
            {
                Debug.LogError($"[{unitName}] Failed to create LineRenderer for buff range!");
                return;
            }
            
            // 다른 LineRenderer와 중복되지 않도록 확인
            LineRenderer[] existingRenderers = GetComponents<LineRenderer>();
            if (existingRenderers.Length > 2)
            {
                foreach (var renderer in existingRenderers)
                {
                    if (renderer != rangeLineRenderer && renderer != buffRangeRenderer)
                    {
                        Destroy(renderer);
                    }
                }
            }
            
            buffRangeRenderer.startWidth = 0.1f;
            buffRangeRenderer.endWidth = 0.1f;
            buffRangeRenderer.material = new Material(Shader.Find("Sprites/Default"));
            buffRangeRenderer.startColor = new Color(1, 0.3f, 0, 1f);
            buffRangeRenderer.endColor = new Color(1, 0.3f, 0, 1f);
            buffRangeRenderer.positionCount = 5;
            buffRangeRenderer.useWorldSpace = false;
            buffRangeRenderer.loop = true;
            buffRangeRenderer.sortingOrder = 1000;
            buffRangeRenderer.sortingLayerName = "Default";
            
            DrawBuffRangeSquare();
            
            // 초기에는 비활성화
            buffRangeRenderer.enabled = false;
            
            Debug.Log($"[{unitName}] Buff range visualization setup complete - buffRange={buffRange}");
        }
        
        private void DrawBuffRangeSquare()
        {
            if (buffRangeRenderer == null) return;
            
            float halfRange = buffRange / 2f;
            // 3x3 정사각형 그리기
            buffRangeRenderer.SetPosition(0, new Vector3(-halfRange, -halfRange, 0));
            buffRangeRenderer.SetPosition(1, new Vector3(halfRange, -halfRange, 0));
            buffRangeRenderer.SetPosition(2, new Vector3(halfRange, halfRange, 0));
            buffRangeRenderer.SetPosition(3, new Vector3(-halfRange, halfRange, 0));
            buffRangeRenderer.SetPosition(4, new Vector3(-halfRange, -halfRange, 0));
        }
        
        private void SetupRangeVisualization()
        {
            if (!showAttackRange) return;
            
            // 기존 LineRenderer 확인
            if (rangeLineRenderer == null)
            {
                rangeLineRenderer = GetComponent<LineRenderer>();
            }
            
            // 없으면 새로 추가
            if (rangeLineRenderer == null)
            {
                rangeLineRenderer = gameObject.AddComponent<LineRenderer>();
            }
            
            // 여전히 null이면 오류 로그하고 리턴
            if (rangeLineRenderer == null)
            {
                Debug.LogError($"[{unitName}] Failed to create LineRenderer for range visualization!");
                return;
            }
            
            rangeLineRenderer.startWidth = 0.05f;
            rangeLineRenderer.endWidth = 0.05f;
            rangeLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            rangeLineRenderer.startColor = rangeColor;
            rangeLineRenderer.endColor = rangeColor;
            rangeLineRenderer.positionCount = 50;
            rangeLineRenderer.useWorldSpace = false;
            rangeLineRenderer.loop = true;
            
            DrawRangeCircle();
            
            // 초기에는 비활성화 (마우스 호버 시에만 표시)
            rangeLineRenderer.enabled = false;
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
                // MageTower는 버프/디버프만 처리
                if (towerType == "MageTower")
                {
                    ApplyMageTowerEffects();
                }
                else
                {
                    FindAndAttackTarget();
                }
            }
            
            // 마우스 호버 체크 (Collider 대신 거리 기반)
            CheckMouseHover();
            
            // 범위 시각화 업데이트
            UpdateRangeVisualization();
        }
        
        private void UpdateRangeVisualization()
        {
            // MageTower 버프 범위 업데이트
            if (towerType == "MageTower" && buffRangeRenderer != null)
            {
                DrawBuffRangeSquare();
            }
            
            // 일반 공격 범위 업데이트
            DrawRangeCircle();
        }
        
        private void ApplyMageTowerEffects()
        {
            // MageTower: 버프 범위 내 아군 유닛에 공격력 버프, 적에게 이동속도 감소
            
            // 디버그: 매 프레임 값 확인 (필요시 활성화)
            // if (Time.frameCount % 60 == 0)
            // {
            //     Debug.Log($"[MageTower {unitName}] attackBuffValue={attackBuffValue}, slowEffectValue={slowEffectValue}, buffRange={buffRange}");
            // }
            
            // 아군 유닛 버프 적용
            if (attackBuffValue > 0)
            {
                Unit[] allUnits = GameObject.FindObjectsOfType<Unit>();
                foreach (Unit unit in allUnits)
                {
                    if (unit == this) continue; // 자신은 제외
                    if (unit.towerType == "MageTower") continue; // 다른 MageTower 제외
                    
                    float distance = Vector3.Distance(transform.position, unit.transform.position);
                    if (distance <= buffRange / 2f)
                    {
                        // 버프 적용
                        unit.ReceiveAttackBuff(attackBuffValue, this);
                    }
                    else
                    {
                        // 버프 제거
                        unit.RemoveAttackBuff(this);
                    }
                }
            }
            
            // 적 디버프(느려짐) 적용
            if (slowEffectValue > 0)
            {
                Enemy[] allEnemies = GameObject.FindObjectsOfType<Enemy>();
                int slowedCount = 0;
                foreach (Enemy enemy in allEnemies)
                {
                    if (enemy.currentHealth <= 0) continue;
                    
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance <= buffRange / 2f)
                    {
                        // slow effect apply
                        enemy.ApplySlowEffect(slowEffectValue, this);
                        slowedCount++;
                    }
                    else
                    {
                        // 느려짐 제거
                        enemy.RemoveSlowEffect(this);
                    }
                }
                
                // 1초마다 로그 출력 (필요시 활성화)
                // if (Time.frameCount % 60 == 0 && slowedCount > 0)
                // {
                //     Debug.Log($"[{unitName}] Slowed {slowedCount} enemies by {slowEffectValue * 100}%");
                // }
            }
            // else if (Time.frameCount % 60 == 0)
            // {
            //     Debug.Log($"[{unitName}] slowEffectValue is 0 or negative, skipping slow effect");
            // }
        }
        
        // 버프 받기 (다른 MageTower에서 호출)
        public void ReceiveAttackBuff(float buffPercent, Unit source)
        {
            // 실제로는 버프 소스를 추적하여 중첩 방지
            // 여기서는 간단히 공격력 증가만 적용
            float buffMultiplier = 1f + buffPercent;
            // TODO: 버프 소스 추적 및 중첩 방지 로직
        }
        
        // 버프 제거
        public void RemoveAttackBuff(Unit source)
        {
            // TODO: 버프 제거 로직
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
                
                // 사거리 범위 표시 활성화
                if (rangeLineRenderer != null)
                {
                    rangeLineRenderer.enabled = true;
                }
                
                // MageTower 버프 범위 표시 활성화
                if (towerType == "MageTower" && buffRangeRenderer != null)
                {
                    buffRangeRenderer.enabled = true;
                }
            }
            else if (!shouldHover && isHovered)
            {
                // 마우스가 나감
                isHovered = false;
                StopCoroutine("ScaleUnit");
                StartCoroutine(ScaleUnit(originalScale));
                
                // 사거리 범위 표시 비활성화
                if (rangeLineRenderer != null)
                {
                    rangeLineRenderer.enabled = false;
                }
                
                // MageTower 버프 범위 표시 비활성화
                if (towerType == "MageTower" && buffRangeRenderer != null)
                {
                    buffRangeRenderer.enabled = false;
                }
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
            
            // 타워 타입 설정 (Name에서 추출)
            towerType = ExtractTowerType(data.Name);
            
            // Set sprite color based on grade
            SetGradeColor();
            
            // 특수 능력 적용 (초기화 시 현재 레벨 확인)
            ApplySpecialAbilities();
            
            // MageTower인 경우 버프 범위 시각화 설정
            if (towerType == "MageTower")
            {
                SetupBuffRangeVisualization();
            }
            
            // 레벨업 이벤트 구독
            if (TowerLevelManager.Instance != null)
            {
                TowerLevelManager.Instance.OnTowerLevelUp += OnTowerLevelUp;
                TowerLevelManager.Instance.OnTowerExpChanged += OnTowerExpChanged;
            }
            
            Debug.Log($"Unit initialized: {unitName}, Type: {towerType}, Grade: {grade}");
        }
        
        private string ExtractTowerType(string name)
        {
            // Name에서 타워 타입 추출 (예: "Archer_Common" -> "Archer")
            // 순서 중요: 더 구체적인 이름을 먼저 체크
            if (name.Contains("Archer")) return "Archer";
            if (name.Contains("WizardTower")) return "MageTower"; // WizardTower -> MageTower로 매핑
            if (name.Contains("Wizard")) return "Mage"; // Wizard -> Mage로 매핑
            if (name.Contains("MageTower")) return "MageTower";
            if (name.Contains("Mage")) return "Mage";
            if (name.Contains("Laser")) return "Laser";
            return "";
        }
        
        private void OnTowerLevelUp(string type, int level)
        {
            // 자신의 타워 타입이 레벨업되면 능력 재적용
            if (type == towerType)
            {
                Debug.Log($"[{unitName}] Level up event received: {type} -> level {level}");
                ApplySpecialAbilities();
                
                // MageTower인 경우 버프 범위 시각화 재설정
                if (towerType == "MageTower")
                {
                    SetupBuffRangeVisualization();
                }
            }
        }
        
        private void OnTowerExpChanged(string type)
        {
            // 경험치 변경 시 필요한 처리
            if (type == towerType)
            {
                Debug.Log($"[{unitName}] Exp changed for {type}");
            }
        }
        
        private void ApplySpecialAbilities()
        {
            Debug.Log($"[ApplySpecialAbilities] {unitName} - towerType={towerType}");
            
            if (string.IsNullOrEmpty(towerType)) 
            {
                Debug.LogWarning($"[{unitName}] towerType is null or empty!");
                return;
            }
            if (SpecialAbilityManager.Instance == null) 
            {
                Debug.LogWarning($"[{unitName}] SpecialAbilityManager.Instance is null!");
                return;
            }
            if (TowerLevelManager.Instance == null) 
            {
                Debug.LogWarning($"[{unitName}] TowerLevelManager.Instance is null!");
                return;
            }
            
            // 현재 레벨 가져오기
            int currentLevel = TowerLevelManager.Instance.GetTowerLevel(towerType);
            Debug.Log($"[{unitName}] Current level for {towerType}: {currentLevel}");
            
            // 새로운 능력 초기화
            multiShotCount = 1;
            groundEffectDuration = 0f;
            rangeIncreaseValue = 0f;
            attackIncreaseValue = 0f;
            chainAttackCount = 0;
            
            // Deprecated 능력 초기화 (하위호환)
            pierceCount = 0;
            areaDamageRadius = 0;
            attackBuffValue = 0f;
            speedBuffValue = 0f;
            speedIncreaseValue = 0f;
            slowEffectValue = 0f;
            
            // 해금된 능력 가져오기
            var unlockedAbilities = SpecialAbilityManager.Instance.GetUnlockedAbilities(towerType, currentLevel);
            
            Debug.Log($"[{unitName}] Found {unlockedAbilities.Count} unlocked abilities for {towerType} at level {currentLevel}");
            
            foreach (var ability in unlockedAbilities)
            {
                Debug.Log($"[{unitName}] Applying ability: {ability.abilityType} = {ability.value} (Lv.{ability.unlockLevel})");
                
                switch (ability.abilityType)
                {
                    // 새로운 능력들
                    case SpecialAbilityType.MultiShot:
                        multiShotCount = Mathf.Max(multiShotCount, (int)ability.value);
                        break;
                    case SpecialAbilityType.GroundEffect:
                        groundEffectDuration = Mathf.Max(groundEffectDuration, ability.value);
                        break;
                    case SpecialAbilityType.ChainAttack:
                        chainAttackCount = Mathf.Max(chainAttackCount, (int)ability.value);
                        break;
                    
                    // 공통 능력들
                    case SpecialAbilityType.RangeIncrease:
                        rangeIncreaseValue = Mathf.Max(rangeIncreaseValue, ability.value);
                        break;
                    case SpecialAbilityType.AttackIncrease:
                        attackIncreaseValue = Mathf.Max(attackIncreaseValue, ability.value);
                        break;
                        
                    // Deprecated 능력들 (하위호환)
                    case SpecialAbilityType.Pierce:
                        pierceCount = Mathf.Max(pierceCount, (int)ability.value);
                        break;
                    case SpecialAbilityType.AreaDamage:
                        areaDamageRadius = Mathf.Max(areaDamageRadius, (int)ability.value);
                        break;
                    case SpecialAbilityType.AttackBuff:
                        attackBuffValue = Mathf.Max(attackBuffValue, ability.value);
                        break;
                    case SpecialAbilityType.SpeedBuff:
                        speedBuffValue = Mathf.Max(speedBuffValue, ability.value);
                        break;
                    case SpecialAbilityType.SpeedIncrease:
                        speedIncreaseValue = Mathf.Max(speedIncreaseValue, ability.value);
                        break;
                    case SpecialAbilityType.SlowEffect:
                        slowEffectValue = Mathf.Max(slowEffectValue, ability.value);
                        break;
                }
            }
            
            // 능력치 적용
            ApplyBuffs();
            
            // 능력 해금 로그
            Debug.Log($"[{unitName}] Abilities: MultiShot={multiShotCount}, GroundEffect={groundEffectDuration}s, Chain={chainAttackCount}, Range+={rangeIncreaseValue}, Atk+={attackIncreaseValue}");
        }
        
        private void ApplyBuffs()
        {
            // 사거리 증가 적용
            float baseRange = range;
            range = baseRange + rangeIncreaseValue;
            
            // 공격력 증가/버프 적용
            float totalAttackMultiplier = 1f + attackIncreaseValue;
            
            // 공격속도 증가 적용
            float totalSpeedMultiplier = 1f + speedIncreaseValue;
            
            Debug.Log($"[{unitName}] Buffs applied - Range: {baseRange}→{range}, Pierce: {pierceCount}, Area: {areaDamageRadius}, Slow: {slowEffectValue}, AtkBuff: {attackBuffValue}");
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
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (TowerLevelManager.Instance != null)
            {
                TowerLevelManager.Instance.OnTowerLevelUp -= OnTowerLevelUp;
                TowerLevelManager.Instance.OnTowerExpChanged -= OnTowerExpChanged;
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
                // Ranged attack - spawn bullet(s)
                // Archer: MultiShot 처리
                if (towerType == "Archer" && multiShotCount > 1)
                {
                    SpawnMultipleBullets(enemy, multiShotCount);
                }
                // Mage: GroundEffect 처리 (지속 피해 바닥 생성)
                else if (towerType == "Mage" && groundEffectDuration > 0)
                {
                    SpawnGroundEffect(enemy.transform.position);
                    // 일반 공격도 함께 수행
                    SpawnBullet(enemy);
                }
                else
                {
                    SpawnBullet(enemy);
                }
            }
            else
            {
                // Melee attack - instant damage
                enemy.TakeDamage(attackPower);
                
                // Laser: 연계 공격 처리
                if (towerType == "Laser" && chainAttackCount > 0)
                {
                    PerformChainAttack(enemy);
                }
            }
            
            // Visual feedback
            ShowAttackEffect();
            
            // Debug.Log($"[{grade}] {unitName} attacks {enemy.enemyName} for {attackPower} damage!");
        }
        
        private void SpawnBullet(Enemy target)
        {
            if (bulletPrefab == null) 
            {
                // Debug.LogWarning($"[{unitName}] bulletPrefab is null!");
                return;
            }
            
            // Debug.Log($"[{unitName}] Spawning bullet - areaDamageRadius={areaDamageRadius}, pierceCount={pierceCount}");
            
            GameObject bullet = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                // 광역 데미지가 있으면 표시 (필요시 활성화)
                // if (areaDamageRadius > 0)
                // {
                //     Debug.Log($"[{unitName}] Firing area damage bullet (radius: {areaDamageRadius})");
                // }
                // else
                // {
                //     Debug.Log($"[{unitName}] Firing normal bullet (no area damage)");
                // }
                
                bulletComponent.Initialize(target, attackPower, pierceCount, areaDamageRadius);
            }
            else
            {
                // Debug.LogError($"[{unitName}] Bullet component not found on prefab!");
            }
        }
        
        // Archer: 다중 발사 (MultiShot)
        private void SpawnMultipleBullets(Enemy target, int count)
        {
            if (bulletPrefab == null || count <= 1) return;
            
            Debug.Log($"[{unitName}] MultiShot {count} bullets!");
            
            // 주변 적들 찾기
            Enemy[] allEnemies = GameObject.FindObjectsOfType<Enemy>();
            List<Enemy> validTargets = new List<Enemy>();
            
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy == null || enemy.currentHealth <= 0) continue;
                if (enemy == target) continue;
                
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= range)
                {
                    validTargets.Add(enemy);
                }
            }
            
            // 첫 번째 투사체는 원래 타겟에게
            GameObject bullet = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Initialize(target, attackPower, pierceCount, areaDamageRadius);
            }
            
            // 나머지 투사체는 주변 적들에게 (없으면 원래 타겟에게)
            for (int i = 1; i < count; i++)
            {
                Enemy bulletTarget = (i - 1 < validTargets.Count) ? validTargets[i - 1] : target;
                
                GameObject extraBullet = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);
                Bullet extraBulletComponent = extraBullet.GetComponent<Bullet>();
                if (extraBulletComponent != null)
                {
                    extraBulletComponent.Initialize(bulletTarget, attackPower, pierceCount, areaDamageRadius);
                }
            }
        }
        
        // Laser: 연계 공격 (ChainAttack)
        private void PerformChainAttack(Enemy hitEnemy)
        {
            if (chainAttackCount <= 0 || hitEnemy == null) return;
            
            Debug.Log($"[{unitName}] Chain Attack starting from {hitEnemy.enemyName}!");
            
            Enemy currentTarget = hitEnemy;
            HashSet<Enemy> hitEnemies = new HashSet<Enemy> { hitEnemy };
            
            for (int i = 0; i < chainAttackCount; i++)
            {
                // 현재 타겟 주변의 적 찾기
                Enemy[] allEnemies = GameObject.FindObjectsOfType<Enemy>();
                Enemy nextTarget = null;
                float closestDistance = chainAttackRange;
                
                foreach (Enemy enemy in allEnemies)
                {
                    if (enemy == null || enemy.currentHealth <= 0) continue;
                    if (hitEnemies.Contains(enemy)) continue;
                    
                    float distance = Vector3.Distance(currentTarget.transform.position, enemy.transform.position);
                    if (distance <= chainAttackRange && distance < closestDistance)
                    {
                        closestDistance = distance;
                        nextTarget = enemy;
                    }
                }
                
                if (nextTarget != null)
                {
                    // 연계 공격 피해 적용
                    nextTarget.TakeDamage(attackPower * 0.8f); // 80% 피해
                    hitEnemies.Add(nextTarget);
                    
                    // 시각적 효과 (라인 렌더러로 연결선 표시)
                    ShowChainEffect(currentTarget.transform.position, nextTarget.transform.position);
                    
                    Debug.Log($"[{unitName}] Chain hit {nextTarget.enemyName}!");
                    currentTarget = nextTarget;
                }
                else
                {
                    break; // 더 이상 연계할 적이 없음
                }
            }
        }
        
        // 연계 공격 시각적 효과
        private void ShowChainEffect(Vector3 from, Vector3 to)
        {
            // 간단한 라인 렌더러로 연결선 표시
            GameObject lineObj = new GameObject("ChainEffect");
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = Color.cyan;
            line.endColor = Color.cyan;
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            line.sortingOrder = 100;
            
            // 0.2초 후 제거
            Destroy(lineObj, 0.2f);
        }
        
        // Mage: 지속 피해 바닥 생성 (GroundEffect)
        private void SpawnGroundEffect(Vector3 position)
        {
            if (groundEffectPrefab == null)
            {
                // 프리팹이 없으면 기본 GroundEffect 생성
                GameObject groundEffectObj = new GameObject("GroundEffect");
                groundEffectObj.transform.position = position;
                GroundEffect groundEffect = groundEffectObj.AddComponent<GroundEffect>();
                
                // 지속 시간과 데미지 설정
                float damagePerSec = attackPower * 0.5f; // 공격력의 50%를 초당 데미지로
                groundEffect.Initialize(groundEffectDuration, damagePerSec, 1.5f);
                
                Debug.Log($"[{unitName}] Spawned GroundEffect at {position} for {groundEffectDuration}s");
            }
            else
            {
                // 프리팹이 있으면 인스턴스화
                GameObject groundEffectObj = Instantiate(groundEffectPrefab, position, Quaternion.identity);
                GroundEffect groundEffect = groundEffectObj.GetComponent<GroundEffect>();
                if (groundEffect != null)
                {
                    float damagePerSec = attackPower * 0.5f;
                    groundEffect.Initialize(groundEffectDuration, damagePerSec, 1.5f);
                }
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