using UnityEngine;
using Recycle.Core.Systems.Economy;
using Recycle.Core.Systems.Progression;
using Recycle.Core.Systems.Defense;
using Recycle.Core.Systems.State;

namespace Recycle.Core
{
    /// <summary>
    /// GameManager - Facade 패턴
    /// 기존 코드와의 호환성을 위해 유지되며, 실제 로직은 각 시스템 매니저에 위임
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("References")]
        public Transform castleTransform;
        public Vector2 castleSize = new Vector2(3f, 3f);
        
        // Facade properties - delegate to system managers
        public GameState CurrentState => GameStateManager.Instance?.CurrentState ?? GameState.Playing;
        public int CurrentCastleHP => CastleManager.Instance?.CurrentCastleHP ?? 0;
        public float SurvivalTime => ScoreManager.Instance?.SurvivalTime ?? 0f;
        public int EnemiesDefeated => ScoreManager.Instance?.EnemiesDefeated ?? 0;
        public int TotalScore => ScoreManager.Instance?.TotalScore ?? 0;
        public int CurrentGold => GoldManager.Instance?.CurrentGold ?? 0;
        public bool IsGameOver => GameStateManager.Instance?.IsGameOver ?? false;
        
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
            // Pass castle settings to CastleManager
            if (CastleManager.Instance != null)
            {
                CastleManager.Instance.castleTransform = castleTransform;
                CastleManager.Instance.castleSize = castleSize;
            }
            
            InitializeGame();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        public void InitializeGame()
        {
            GameStateManager.Instance?.InitializeGame();
        }
        
        // Facade methods - delegate to system managers
        public void AddGold(int amount)
        {
            GoldManager.Instance?.AddGold(amount);
        }
        
        public bool SpendGold(int amount)
        {
            return GoldManager.Instance?.SpendGold(amount) ?? false;
        }
        
        public void DamageCastle(int damage = 1)
        {
            CastleManager.Instance?.DamageCastle(damage);
        }
        
        public void EnemyDefeated(int rewardScore = 10, int rewardGold = 10)
        {
            ScoreManager.Instance?.RecordEnemyDefeated();
            GoldManager.Instance?.AddGold(rewardGold);
        }
        
        public void GameOver()
        {
            GameStateManager.Instance?.GameOver();
        }
        
        public void RestartGame()
        {
            GameStateManager.Instance?.RestartGame();
        }
        
        public bool IsInCastleBounds(Vector3 position)
        {
            return CastleManager.Instance?.IsInCastleBounds(position) ?? false;
        }
    }
}
