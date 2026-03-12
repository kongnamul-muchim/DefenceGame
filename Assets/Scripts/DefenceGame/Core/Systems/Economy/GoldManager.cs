using System;
using UnityEngine;

namespace DefenceGame.Core.Systems.Economy
{
    /// <summary>
    /// 골드 관리 시스템 - 단일 책임: 골드의 획득 및 소비 관리
    /// </summary>
    public class GoldManager : MonoBehaviour
    {
        public static GoldManager Instance { get; private set; }
        
        [SerializeField] private int currentGold = 200;
        
        public event Action<int> OnGoldChanged;
        
        public int CurrentGold => currentGold;
        
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
        
        public void Initialize(int startingGold = 200)
        {
            currentGold = startingGold;
            OnGoldChanged?.Invoke(currentGold);
        }
        
        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            
            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
        }
        
        public bool CanSpend(int amount)
        {
            return currentGold >= amount;
        }
        
        public bool SpendGold(int amount)
        {
            if (CanSpend(amount))
            {
                currentGold -= amount;
                OnGoldChanged?.Invoke(currentGold);
                return true;
            }
            return false;
        }
    }
}
