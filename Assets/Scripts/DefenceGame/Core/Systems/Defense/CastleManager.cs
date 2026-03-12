using System;
using UnityEngine;

namespace DefenceGame.Core.Systems.Defense
{
    /// <summary>
    /// 성 방어 시스템 - 단일 책임: 성의 체력과 위치 관리
    /// </summary>
    public class CastleManager : MonoBehaviour
    {
        public static CastleManager Instance { get; private set; }
        
        [Header("Castle Settings")]
        public int maxCastleHP = 20;
        public Transform castleTransform;
        public Vector2 castleSize = new Vector2(3f, 3f);
        
        [SerializeField] private int currentCastleHP;
        
        public event Action<int> OnCastleHPChanged;
        public event Action OnCastleDestroyed;
        
        public int CurrentCastleHP => currentCastleHP;
        public int MaxCastleHP => maxCastleHP;
        public bool IsDestroyed => currentCastleHP <= 0;
        
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
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        public void Initialize()
        {
            currentCastleHP = maxCastleHP;
            OnCastleHPChanged?.Invoke(currentCastleHP);
        }
        
        public void DamageCastle(int damage = 1)
        {
            if (IsDestroyed) return;
            
            currentCastleHP -= damage;
            OnCastleHPChanged?.Invoke(currentCastleHP);
            
            if (currentCastleHP <= 0)
            {
                OnCastleDestroyed?.Invoke();
            }
        }
        
        public bool IsInCastleBounds(Vector3 position)
        {
            if (castleTransform == null) return false;
            
            Vector3 castlePos = castleTransform.position;
            float halfWidth = castleSize.x / 2f;
            float halfHeight = castleSize.y / 2f;
            
            return position.x >= castlePos.x - halfWidth &&
                   position.x <= castlePos.x + halfWidth &&
                   position.y >= castlePos.y - halfHeight &&
                   position.y <= castlePos.y + halfHeight;
        }
        
        public Vector3 GetCastlePosition()
        {
            return castleTransform != null ? castleTransform.position : Vector3.zero;
        }
    }
}
