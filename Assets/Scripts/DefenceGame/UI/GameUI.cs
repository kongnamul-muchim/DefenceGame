using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DefenceGame.Core;
using DefenceGame.Core.Managers;
using DefenceGame.Core.Systems.Economy;
using DefenceGame.Core.Systems.Progression;
using DefenceGame.Core.Systems.Defense;
using DefenceGame.Core.Systems.State;

namespace DefenceGame.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("References")]
        public GameManager gameManager;
        public WaveManager waveManager;
        
        [Header("UI Elements")]
        public TextMeshProUGUI castleHPText;
        public TextMeshProUGUI goldText;
        public TextMeshProUGUI waveText;
        public TextMeshProUGUI enemiesDefeatedText;
        public TextMeshProUGUI survivalTimeText;
        public TextMeshProUGUI scoreText;
        
        [Header("Game Over Panel")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI finalScoreText;
        public TextMeshProUGUI finalSurvivalTimeText;
        public TextMeshProUGUI finalEnemiesDefeatedText;
        public Button restartButton;
        
        private void Start()
        {
            if (gameManager == null)
                gameManager = GameManager.Instance;
            if (waveManager == null)
                waveManager = WaveManager.Instance;
            
            // Subscribe to events from system managers
            if (CastleManager.Instance != null)
            {
                CastleManager.Instance.OnCastleHPChanged += UpdateCastleHP;
            }
            
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.OnGoldChanged += UpdateGold;
            }
            
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += UpdateScore;
                ScoreManager.Instance.OnSurvivalTimeChanged += UpdateSurvivalTime;
                ScoreManager.Instance.OnEnemiesDefeatedChanged += UpdateEnemiesDefeated;
            }
            
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameOver += ShowGameOver;
            }
            
            if (waveManager != null)
            {
                waveManager.OnWaveStarted += UpdateWave;
            }
            
            // Setup restart button
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartGame);
            }
            
            // Hide game over panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            // Initialize UI
            UpdateAllUI();
        }
        
        private void OnDestroy()
        {
            if (CastleManager.Instance != null)
            {
                CastleManager.Instance.OnCastleHPChanged -= UpdateCastleHP;
            }
            
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.OnGoldChanged -= UpdateGold;
            }
            
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= UpdateScore;
                ScoreManager.Instance.OnSurvivalTimeChanged -= UpdateSurvivalTime;
                ScoreManager.Instance.OnEnemiesDefeatedChanged -= UpdateEnemiesDefeated;
            }
            
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameOver -= ShowGameOver;
            }
            
            if (waveManager != null)
            {
                waveManager.OnWaveStarted -= UpdateWave;
            }
        }
        
        private void Update()
        {
            // Update time every frame
            if (GameStateManager.Instance != null && !GameStateManager.Instance.IsGameOver)
            {
                if (ScoreManager.Instance != null)
                {
                    UpdateSurvivalTime(ScoreManager.Instance.SurvivalTime);
                }
            }
        }
        
        private void UpdateAllUI()
        {
            if (CastleManager.Instance != null)
            {
                UpdateCastleHP(CastleManager.Instance.CurrentCastleHP);
            }
            
            if (GoldManager.Instance != null)
            {
                UpdateGold(GoldManager.Instance.CurrentGold);
            }
            
            if (ScoreManager.Instance != null)
            {
                UpdateScore(ScoreManager.Instance.TotalScore);
                UpdateSurvivalTime(ScoreManager.Instance.SurvivalTime);
                UpdateEnemiesDefeated(ScoreManager.Instance.EnemiesDefeated);
            }
            
            if (waveManager != null)
            {
                UpdateWave(waveManager.CurrentWave);
            }
        }
        
        private void UpdateCastleHP(int hp)
        {
            if (castleHPText != null)
            {
                castleHPText.text = $"Castle HP: {hp}";
            }
        }
        
        private void UpdateGold(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"Gold: {gold}";
            }
        }
        
        private void UpdateWave(int wave)
        {
            if (waveText != null)
            {
                waveText.text = $"Wave: {wave}";
            }
        }
        
        private void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }
        
        private void UpdateEnemiesDefeated(int count)
        {
            if (enemiesDefeatedText != null)
            {
                enemiesDefeatedText.text = $"Enemies: {count}";
            }
        }
        
        private void UpdateSurvivalTime(float time)
        {
            if (survivalTimeText != null)
            {
                int minutes = Mathf.FloorToInt(time / 60);
                int seconds = Mathf.FloorToInt(time % 60);
                survivalTimeText.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }
        
        private void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                
                if (ScoreManager.Instance != null)
                {
                    finalScoreText.text = $"Final Score: {ScoreManager.Instance.TotalScore}";
                    finalSurvivalTimeText.text = $"Survival Time: {ScoreManager.Instance.SurvivalTime:F1}s";
                    finalEnemiesDefeatedText.text = $"Enemies Defeated: {ScoreManager.Instance.EnemiesDefeated}";
                }
            }
        }
        
        private void RestartGame()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.RestartGame();
            }
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            UpdateAllUI();
        }
    }
}