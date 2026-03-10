using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DefenceGame.Core;

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
            
            // Subscribe to events
            if (gameManager != null)
            {
                gameManager.OnCastleHPChanged += UpdateCastleHP;
                gameManager.OnGoldChanged += UpdateGold;
                gameManager.OnScoreChanged += UpdateScore;
                gameManager.OnSurvivalTimeChanged += UpdateSurvivalTime;
                gameManager.OnGameOver += ShowGameOver;
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
            if (gameManager != null)
            {
                gameManager.OnCastleHPChanged -= UpdateCastleHP;
                gameManager.OnGoldChanged -= UpdateGold;
                gameManager.OnScoreChanged -= UpdateScore;
                gameManager.OnSurvivalTimeChanged -= UpdateSurvivalTime;
                gameManager.OnGameOver -= ShowGameOver;
            }
            
            if (waveManager != null)
            {
                waveManager.OnWaveStarted -= UpdateWave;
            }
        }
        
        private void Update()
        {
            // Update time every frame
            if (gameManager != null && !gameManager.IsGameOver)
            {
                UpdateSurvivalTime(gameManager.SurvivalTime);
            }
        }
        
        private void UpdateAllUI()
        {
            if (gameManager != null)
            {
                UpdateCastleHP(gameManager.CurrentCastleHP);
                UpdateGold(gameManager.CurrentGold);
                UpdateScore(gameManager.TotalScore);
                UpdateSurvivalTime(gameManager.SurvivalTime);
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
            
            if (enemiesDefeatedText != null && gameManager != null)
            {
                enemiesDefeatedText.text = $"Enemies: {gameManager.EnemiesDefeated}";
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
                
                if (gameManager != null)
                {
                    finalScoreText.text = $"Final Score: {gameManager.TotalScore}";
                    finalSurvivalTimeText.text = $"Survival Time: {gameManager.SurvivalTime:F1}s";
                    finalEnemiesDefeatedText.text = $"Enemies Defeated: {gameManager.EnemiesDefeated}";
                }
            }
        }
        
        private void RestartGame()
        {
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            UpdateAllUI();
        }
    }
}