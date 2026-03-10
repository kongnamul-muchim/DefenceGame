using UnityEngine;

namespace DefenceGame.Core
{
    public class UnitDragSystem : MonoBehaviour
    {
        public static UnitDragSystem Instance { get; private set; }
        
        [Header("Drag Settings")]
        public float dragScale = 1.1f; // 드래그 중 크기
        public Color validDropColor = new Color(1f, 1f, 1f, 0.7f); // 유효한 드롭 위치
        public Color invalidDropColor = new Color(1f, 0.3f, 0.3f, 0.7f); // 무효한 드롭 위치
        public Color mergeableColor = new Color(0.3f, 1f, 0.3f, 0.7f); // 합성 가능
        
        private Unit currentlyDraggedUnit;
        private Vector3 originalPosition;
        private Vector3 originalScale;
        private SpriteRenderer unitSpriteRenderer;
        private Color originalColor;
        private bool isDragging = false;
        
        public bool IsDragging => isDragging;
        public Unit CurrentlyDraggedUnit => currentlyDraggedUnit;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void StartDrag(Unit unit)
        {
            if (unit == null) return;
            
            currentlyDraggedUnit = unit;
            originalPosition = unit.transform.position;
            originalScale = unit.transform.localScale;
            
            unitSpriteRenderer = unit.GetComponent<SpriteRenderer>();
            if (unitSpriteRenderer != null)
            {
                originalColor = unitSpriteRenderer.color;
            }
            
            // 드래그 중 크기 조정
            unit.transform.localScale = originalScale * dragScale;
            
            isDragging = true;
            
            Debug.Log($"Started dragging unit: {unit.unitName}");
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
            if (unitSpriteRenderer == null) return;
            
            Vector3 currentPos = currentlyDraggedUnit.transform.position;
            
            // 1. Wall 타일 확인
            if (!IsWallTile(currentPos))
            {
                unitSpriteRenderer.color = invalidDropColor;
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
                    unitSpriteRenderer.color = mergeableColor;
                }
                else
                {
                    unitSpriteRenderer.color = invalidDropColor;
                }
            }
            else if (targetUnit == null)
            {
                // 빈 공간 - 이동 가능
                unitSpriteRenderer.color = validDropColor;
            }
        }
        
        public void EndDrag(Unit unit)
        {
            if (!isDragging || currentlyDraggedUnit != unit) return;
            
            // 원래 크기로 복귀
            unit.transform.localScale = originalScale;
            
            Vector3 dropPosition = unit.transform.position;
            
            // 1. Wall 타일 확인
            if (!IsWallTile(dropPosition))
            {
                ReturnToOriginalPosition();
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
                    UnitMergeManager.Instance.MergeUnits(unit, targetUnit);
                }
                else
                {
                    // 합성 불가능 → 원위치 복귀
                    ReturnToOriginalPosition();
                }
            }
            else if (targetUnit == null)
            {
                // 4. 이동
                if (UnitPlacementManager.Instance != null)
                {
                    if (!UnitPlacementManager.Instance.MoveUnitToPosition(unit, dropPosition))
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
                
                // 색상 복귀
                if (unitSpriteRenderer != null)
                {
                    unitSpriteRenderer.color = originalColor;
                }
            }
        }
        
        private void ResetDragState()
        {
            // 색상 복귀
            if (unitSpriteRenderer != null)
            {
                unitSpriteRenderer.color = originalColor;
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
