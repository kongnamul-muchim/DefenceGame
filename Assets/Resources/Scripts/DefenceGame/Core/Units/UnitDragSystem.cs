using UnityEngine;

namespace DefenceGame.Core
{
    public class UnitDragSystem : MonoBehaviour
    {
        public static UnitDragSystem Instance { get; private set; }
        
        [Header("Drag Settings")]
        public float dragScale = 1.3f; // 드래그 중 크기 증가
        public Color validDropColor = new Color(1f, 1f, 1f, 0.5f); // 유효한 드롭 위치 (반투명 흰색)
        public Color invalidDropColor = new Color(1f, 0.2f, 0.2f, 0.6f); // 무효한 드롭 위치 (반투명 빨간색)
        public Color mergeableColor = new Color(0.2f, 1f, 0.2f, 0.6f); // 합성 가능 (반투명 녹색)
        
        [Header("Raycast Settings")]
        public LayerMask unitLayer; // Unit 레이어 마스크
        
        private Unit currentlyDraggedUnit;
        private Vector3 originalPosition;
        private Vector3 originalScale;
        private SpriteRenderer unitSpriteRenderer;
        private Color originalColor;
        private bool isDragging = false;
        private Camera mainCamera;
        
        public bool IsDragging => isDragging;
        public Unit CurrentlyDraggedUnit => currentlyDraggedUnit;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                mainCamera = Camera.main;
                
                // Unit 레이어 마스크 설정 (Layer 6 = Unit)
                unitLayer = LayerMask.GetMask("Unit");
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            HandleMouseInput();
            
            // 드래그 중일 때 마우스 위치 추적
            if (isDragging && currentlyDraggedUnit != null)
            {
                if (Input.GetMouseButton(0))
                {
                    Vector3 mousePos = Input.mousePosition;
                    mousePos.z = -mainCamera.transform.position.z;
                    Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
                    worldPos.z = originalPosition.z;
                    UpdateDragPosition(worldPos);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    EndDrag(currentlyDraggedUnit);
                }
            }
        }
        
        private void HandleMouseInput()
        {
            // 마우스 클릭 시작
            if (Input.GetMouseButtonDown(0) && !isDragging)
            {
                TryStartDrag();
            }
        }
        
        private void TryStartDrag()
        {
            // Raycast로 Unit 감지
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -mainCamera.transform.position.z;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, unitLayer);
            
            if (hit.collider != null)
            {
                Unit unit = hit.collider.GetComponent<Unit>();
                if (unit != null)
                {
                    // 게임 오버 체크
                    if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                        return;
                    
                    StartDrag(unit);
                }
            }
        }
        
        public void StartDrag(Unit unit)
        {
            if (unit == null || isDragging) return;
            
            currentlyDraggedUnit = unit;
            originalPosition = unit.transform.position;
            originalScale = unit.transform.localScale;
            
            unitSpriteRenderer = unit.GetComponent<SpriteRenderer>();
            if (unitSpriteRenderer != null)
            {
                // Unit 클래스에서 저장한 등급별 색상을 가져옴
                originalColor = unit.GetGradeColor();
            }
            
            // 드래그 중 크기 조정
            unit.transform.localScale = originalScale * dragScale;
            
            // 드래그 중 반투명 효과 적용
            if (unitSpriteRenderer != null)
            {
                Color dragColor = originalColor;
                dragColor.a = 0.7f; // 반투명
                unitSpriteRenderer.color = dragColor;
            }
            
            // RareColor도 반투명하게 변경
            Color rareDragColor = unit.GetGradeColor();
            rareDragColor.a = 0.5f;
            unit.SetRareColor(rareDragColor);
            
            // Unit에 드래그 상태 설정
            unit.OnDragStart();
            
            isDragging = true;
            
            Debug.Log($"Started dragging unit: {unit.unitName} at {originalPosition}");
        }
        
        public void UpdateDragPosition(Vector3 worldPosition)
        {
            if (!isDragging || currentlyDraggedUnit == null) return;
            
            // 마우스 위치로 이동
            currentlyDraggedUnit.transform.position = new Vector3(worldPosition.x, worldPosition.y, originalPosition.z);
            
            // 드롭 위치 시각적 피드백
            UpdateVisualFeedback();
        }
        
        private void UpdateVisualFeedback()
        {
            if (unitSpriteRenderer == null || currentlyDraggedUnit == null) return;
            
            Vector3 currentPos = currentlyDraggedUnit.transform.position;
            
            // 1. Wall 타일 확인
            if (!IsWallTile(currentPos))
            {
                // Wall 타일이 아님 - 빨간색
                unitSpriteRenderer.color = invalidDropColor;
                currentlyDraggedUnit.SetRareColor(invalidDropColor);
                return;
            }
            
            // 2. 드롭 위치에 유닛 확인
            Unit targetUnit = UnitPlacementManager.Instance?.GetUnitAtPosition(currentPos);
            
            if (targetUnit != null && targetUnit != currentlyDraggedUnit)
            {
                // 합성 가능 여부 확인
                if (UnitMergeManager.Instance != null && 
                    UnitMergeManager.Instance.CanMerge(currentlyDraggedUnit, targetUnit))
                {
                    // 합성 가능 - 녹색
                    unitSpriteRenderer.color = mergeableColor;
                    currentlyDraggedUnit.SetRareColor(mergeableColor);
                }
                else
                {
                    // 합성 불가능 - 빨간색
                    unitSpriteRenderer.color = invalidDropColor;
                    currentlyDraggedUnit.SetRareColor(invalidDropColor);
                }
            }
            else if (targetUnit == null)
            {
                // 빈 공간 - 이동 가능 (흰색 반투명)
                unitSpriteRenderer.color = validDropColor;
                currentlyDraggedUnit.SetRareColor(validDropColor);
            }
        }
        
        public void EndDrag(Unit unit)
        {
            if (!isDragging || currentlyDraggedUnit != unit) return;
            
            // 원래 크기로 복귀
            unit.transform.localScale = originalScale;
            
            Vector3 dropPosition = unit.transform.position;
            
            // 색상 복귀 (드래그 종료 전에 원래 색상으로)
            if (currentlyDraggedUnit != null)
            {
                currentlyDraggedUnit.RestoreGradeColor();
            }
            
            // 1. Wall 타일 확인
            if (!IsWallTile(dropPosition))
            {
                ReturnToOriginalPosition();
                ResetDragState();
                return;
            }
            
            // 2. 드롭 위치에 유닛 확인
            Unit targetUnit = UnitPlacementManager.Instance?.GetUnitAtPosition(dropPosition);
            
            if (targetUnit != null && targetUnit != unit)
            {
                // 3. 합성 시도
                if (UnitMergeManager.Instance != null && 
                    UnitMergeManager.Instance.CanMerge(unit, targetUnit))
                {
                    // 합성 전에 드래그 상태 초기화 (유닛이 파괴되기 전에)
                    Unit draggedUnitRef = currentlyDraggedUnit;
                    currentlyDraggedUnit = null;
                    isDragging = false;
                    unitSpriteRenderer = null;
                    
                    // 합성 실행
                    UnitMergeManager.Instance.MergeUnits(unit, targetUnit);
                    return;
                }
                else
                {
                    // 합성 불가능 → 원위치 복귀
                    ReturnToOriginalPosition();
                    ResetDragState();
                    return;
                }
            }
            else if (targetUnit == null)
            {
                // 4. 이동
                if (UnitPlacementManager.Instance != null)
                {
                    // 정확한 위치로 스냅
                    Vector3 snappedPosition = GetSnappedPosition(dropPosition);
                    if (UnitPlacementManager.Instance.MoveUnitToPosition(unit, snappedPosition))
                    {
                        // 이동 성공 - 색상 복귀
                        unit.RestoreGradeColor();
                    }
                    else
                    {
                        // 이동 실패 → 원위치 복귀
                        ReturnToOriginalPosition();
                    }
                }
                else
                {
                    ReturnToOriginalPosition();
                }
            }
            
            ResetDragState();
        }
        
        private Vector3 GetSnappedPosition(Vector3 position)
        {
            if (GridSystem.Instance == null || GridSystem.Instance.wallTilemap == null)
                return position;
            
            // 타일 중심으로 스냅
            Vector3Int cellPos = GridSystem.Instance.wallTilemap.WorldToCell(position);
            Vector3 worldPos = GridSystem.Instance.wallTilemap.CellToWorld(cellPos);
            worldPos += new Vector3(0.5f, 0.5f, 0.5f); // 타일 중심
            
            return worldPos;
        }
        
        private bool IsWallTile(Vector3 position)
        {
            if (GridSystem.Instance == null) return false;
            
            GridSystem.Node node = GridSystem.Instance.GetNodeFromWorldPosition(position);
            if (node == null) return false;
            
            // Wall 타일인지 확인 (walkable이 아니면 Wall)
            return !node.isWalkable;
        }
        
        private void ReturnToOriginalPosition()
        {
            if (currentlyDraggedUnit != null)
            {
                currentlyDraggedUnit.transform.position = originalPosition;
                
                // 색상 복귀 - Unit의 등급 색상으로 복귀
                currentlyDraggedUnit.RestoreGradeColor();
            }
        }
        
        private void ResetDragState()
        {
            // Unit에 드래그 종료 알림
            if (currentlyDraggedUnit != null && currentlyDraggedUnit.gameObject != null)
            {
                currentlyDraggedUnit.OnDragEnd();
            }
            
            // 색상 복귀 (혹시 모르게 한 번 더) - 단 유닛이 파괴되지 않은 경우에만
            if (currentlyDraggedUnit != null && currentlyDraggedUnit.gameObject != null)
            {
                currentlyDraggedUnit.RestoreGradeColor();
            }
            
            currentlyDraggedUnit = null;
            unitSpriteRenderer = null;
            isDragging = false;
            
            Debug.Log("Drag ended");
        }
        
        public void CancelDrag()
        {
            if (isDragging && currentlyDraggedUnit != null)
            {
                ReturnToOriginalPosition();
                ResetDragState();
            }
        }
    }
}
