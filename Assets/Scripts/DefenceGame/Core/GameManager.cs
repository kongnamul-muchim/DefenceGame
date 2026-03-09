using System;
using UnityEngine;
using BallShotGame.Core;
using BallShotGame.Events;

namespace DefenceGame.Core
{
    public enum GameState
    {
        Playing,
        GameOver
    }
    
    public class GameManager : MonoBehaviour, IService, IGameManagerService
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Game Settings")]
        public int maxCastleHP = 20;
        public Transform castleTransform;
        public Vector2 castleSize = new Vector2(3f, 3f);
        
        [Header("Current State")]
        [SerializeField] private GameState currentState = GameState.Playing;
        [SerializeField] private int currentCastleHP;
        [SerializeField] private float survivalTime;
        [SerializeField] private int enemiesDefeated;
        [SerializeField] private int totalScore;
        
        // Events
        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnCastleHPChanged;
        public event Action<int> OnScoreChanged;
        public event Action<float> OnSurvivalTimeChanged;
        public event Action OnGameOver;
        
        // Properties
        public GameState CurrentState => currentState;
        public int CurrentCastleHP => currentCastleHP;
        public float SurvivalTime => survivalTime;
        public int EnemiesDefeated => enemiesDefeated;
        public int TotalScore => totalScore;
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
            // Register as service
            GameService.Instance.Register<IGameManagerService>(this);
            
            InitializeGame();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                survivalTime += Time.deltaTime;
                OnSurvivalTimeChanged?.Invoke(survivalTime);
                
                // Update score based on survival time and enemies defeated
                CalculateScore();
            }
        }
        
        public void InitializeGame()
        {
            currentCastleHP = maxCastleHP;
            survivalTime = 0f;
            enemiesDefeated = 0;
            totalScore = 0;
            currentState = GameState.Playing;
            
            OnCastleHPChanged?.Invoke(currentCastleHP);
            OnScoreChanged?.Invoke(totalScore);
            OnSurvivalTimeChanged?.Invoke(survivalTime);
            
            Debug.Log("Game initialized!");
        }
        
        public void DamageCastle(int damage = 1)
        {
            if (currentState != GameState.Playing) return;
            
            currentCastleHP -= damage;
            OnCastleHPChanged?.Invoke(currentCastleHP);
            
            Debug.Log($"Castle damaged! HP: {currentCastleHP}/{maxCastleHP}");
            
            if (currentCastleHP <= 0)
            {
                GameOver();
            }
        }
        
        public void EnemyDefeated(int rewardScore = 10)
        {
            if (currentState != GameState.Playing) return;
            
            enemiesDefeated++;
            CalculateScore();
            
            Debug.Log($"Enemy defeated! Total: {enemiesDefeated}");
        }
        
        private void CalculateScore()
        {
            // Score formula: survival time * 10 + enemies defeated * 100
            totalScore = Mathf.FloorToInt(survivalTime * 10) + (enemiesDefeated * 100);
            OnScoreChanged?.Invoke(totalScore);
        }
        
        public void GameOver()
        {
            if (currentState == GameState.GameOver) return;
            
            currentState = GameState.GameOver;
            OnGameStateChanged?.Invoke(currentState);
            OnGameOver?.Invoke();
            
            Debug.Log($"Game Over! Survival Time: {survivalTime:F1}s, Score: {totalScore}");
            
            // Publish event
            EventBus.Instance.Publish(new GameOverEvent
            {
                SurvivalTime = survivalTime,
                TotalScore = totalScore,
                EnemiesDefeated = enemiesDefeated
            });
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
        
        public void RestartGame()
        {
            InitializeGame();
        }
        
        // IService Implementation
        public void Initialize()
        {
            Debug.Log("GameManager initialized as service");
        }
        
        public void Dispose()
        {
            Debug.Log("GameManager disposed");
        }
    }
    
    public interface IGameManagerService : IService
    {
        GameState CurrentState { get; }
        int CurrentCastleHP { get; }
        float SurvivalTime { get; }
        int TotalScore { get; }
        void DamageCastle(int damage = 1);
        void EnemyDefeated(int rewardScore = 10);
        void GameOver();
        void RestartGame();
        bool IsInCastleBounds(Vector3 position);
    }
    
    public struct GameOverEvent
    {
        public float SurvivalTime;
        public int TotalScore;
        public int EnemiesDefeated;
    }
}
