using UnityEngine;

namespace DefenceGame.Core
{
    public class UnitDrag : MonoBehaviour
    {
        private bool isDragging = false;
        private Unit unit;

        private void Awake()
        {
            unit = GetComponent<Unit>();
        }

        public void OnDragStart()
        {
            isDragging = true;
        }

        public void OnDragEnd()
        {
            isDragging = false;
        }

        public bool IsDragging()
        {
            return isDragging;
        }

        private void OnDisable()
        {
            if (isDragging)
            {
                UnitDragSystem.Instance?.CancelDrag();
                isDragging = false;
            }
        }
    }
}
