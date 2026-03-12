using System;
using UnityEngine;
using DefenceGame.Core.Systems.Economy;
using DefenceGame.Core.Systems.Progression;
using DefenceGame.Core.Systems.Defense;

namespace DefenceGame.Core.Systems.State
{
    public enum GameState
    {
        Playing,
        GameOver
    }
    
    /// <summary>
    /// 게임 상태 관리 시스템 - 단일 책임: 게임 상태 전환 및 조율
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }
        
        [SerializeField] private GameState currentState = GameState.Playing;
        
        public event Action<GameState> OnGameStateChanged;
        public event Action OnGameOver;
        
        public GameState CurrentState => currentState;
        public bool IsGameOver => currentState == GameState.GameOver;
        
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
        
        private void Start()
        {
            // Subscribe to castle destruction
            if (CastleManager.Instance != null)
            {
                CastleManager.Instance.OnCastleDestroyed += OnCastleDestroyed;
            }
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            
            if (CastleManager.Instance != null)
            {
                CastleManager.Instance.OnCastleDestroyed -= OnCastleDestroyed;
            }
        }
        
        public void InitializeGame()
        {
            currentState = GameState.Playing;
            
            // Initialize all systems
            if (GoldManager.Instance != null)
                GoldManager.Instance.Initialize();
            
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.Initialize();
            
            if (CastleManager.Instance != null)
                CastleManager.Instance.Initialize();
            
            OnGameStateChanged?.Invoke(currentState);
        }
        
        private void OnCastleDestroyed()
        {
            GameOver();
        }
        
        public void GameOver()
        {
            if (currentState == GameState.GameOver) return;
            
            currentState = GameState.GameOver;
            
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.StopTracking();
            
            OnGameStateChanged?.Invoke(currentState);
            OnGameOver?.Invoke();
        }
        
        public void RestartGame()
        {
            InitializeGame();
        }
    }
}
