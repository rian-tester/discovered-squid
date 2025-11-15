using UnityEngine;
using TMPro;
using System;

namespace FirstRound
{
    /// <summary>
    /// Manages all UI updates and display
    /// Subscribes to manager events and updates UI elements
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Top Bar UI")]
        [SerializeField] private TMP_Text playerIDText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text efficiencyText;
        
        [Header("Bottom Bar UI")]
        [SerializeField] private TMP_Text matchesText;
        [SerializeField] private TMP_Text turnsText;
        [SerializeField] private TMP_Text comboText;
        
        [Header("Optional UI")]
        [SerializeField] private TMP_Text gameOverText;
        [SerializeField] private GameObject gameOverPanel;
        
        // Managers references
        private ScoreManager scoreManager;
        private CardManager cardManager;
        
        // Timer tracking
        private float gameTime = 0f;
        private bool isGameActive = false;
        
        // Player data
        private string playerID;
        
        #region Initialization
        
        /// <summary>
        /// Initializes UI with manager references
        /// </summary>
        /// <param name="scoreManager"></param>
        /// <param name="cardManager"></param>
        public void Initialize(ScoreManager scoreManager, CardManager cardManager)
        {
            this.scoreManager = scoreManager;
            this.cardManager = cardManager;
            
            // Generate player ID
            playerID = GeneratePlayerID();
            
            // Subscribe to score events
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged += UpdateScore;
                scoreManager.OnMatchesChanged += UpdateMatches;
                scoreManager.OnTurnsChanged += UpdateTurns;
                scoreManager.OnComboChanged += UpdateCombo;
            }
            
            // Subscribe to card manager events
            if (cardManager != null)
            {
                cardManager.OnAllCardsMatched += ShowGameOver;
            }
            
            // Initialize UI
            UpdatePlayerID();
            ResetUI();
            
            Debug.Log("UIManager initialized");
        }
        
        /// <summary>
        /// Cleans up event subscriptions
        /// </summary>
        public void Cleanup()
        {
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged -= UpdateScore;
                scoreManager.OnMatchesChanged -= UpdateMatches;
                scoreManager.OnTurnsChanged -= UpdateTurns;
                scoreManager.OnComboChanged -= UpdateCombo;
            }
            
            if (cardManager != null)
            {
                cardManager.OnAllCardsMatched -= ShowGameOver;
            }
        }
        
        #endregion
        
        #region Player ID
        
        /// <summary>
        /// Generates unique player identifier
        /// </summary>
        private string GeneratePlayerID()
        {
            string machineID = SystemInfo.deviceUniqueIdentifier;
            return "PC-" + machineID.Substring(0, 6).ToUpper();
        }
        
        /// <summary>
        /// Updates player ID display
        /// </summary>
        private void UpdatePlayerID()
        {
            if (playerIDText != null)
            {
                playerIDText.text = "Player: " + playerID;
            }
        }
        
        #endregion
        
        #region Game Timer
        
        /// <summary>
        /// Starts the game timer
        /// </summary>
        public void StartTimer()
        {
            gameTime = 0f;
            isGameActive = true;
        }
        
        /// <summary>
        /// Stops the game timer
        /// </summary>
        public void StopTimer()
        {
            isGameActive = false;
        }
        
        /// <summary>
        /// Resets the game timer
        /// </summary>
        public void ResetTimer()
        {
            gameTime = 0f;
            UpdateTimer();
        }
        
        private void Update()
        {
            if (isGameActive)
            {
                gameTime += Time.deltaTime;
                UpdateTimer();
            }
        }
        
        /// <summary>
        /// Updates timer display
        /// </summary>
        private void UpdateTimer()
        {
            if (timerText != null)
            {
                int minutes = (int)(gameTime / 60f);
                int seconds = (int)(gameTime % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }
        
        #endregion
        
        #region Score UI Updates
        
        /// <summary>
        /// Updates score display
        /// </summary>
        /// <param name="score"></param>
        private void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = "Score: " + score;
            }
        }
        
        /// <summary>
        /// Updates matches display
        /// </summary>
        /// <param name="matches"></param>
        private void UpdateMatches(int matches)
        {
            if (matchesText != null)
            {
                matchesText.text = matches.ToString();
            }
            
            UpdateEfficiency();
        }
        
        /// <summary>
        /// Updates turns display
        /// </summary>
        /// <param name="turns"></param>
        private void UpdateTurns(int turns)
        {
            if (turnsText != null)
            {
                turnsText.text = turns.ToString();
            }
            
            UpdateEfficiency();
        }
        
        /// <summary>
        /// Updates combo display with color coding
        /// </summary>
        /// <param name="combo"></param>
        private void UpdateCombo(int combo)
        {
            if (comboText != null)
            {
                if (combo >= 2)
                {
                    comboText.text = GetComboText(combo);
                    comboText.color = GetComboColor(combo);
                }
                else
                {
                    comboText.text = "";
                }
            }
        }
        
        /// <summary>
        /// Updates efficiency display with color coding
        /// </summary>
        private void UpdateEfficiency()
        {
            if (efficiencyText != null && scoreManager != null)
            {
                int turns = scoreManager.GetTotalTurns();
                
                if (turns == 0)
                {
                    // Before first turn
                    efficiencyText.text = "--";
                    efficiencyText.color = Color.white;
                }
                else
                {
                    // Normal display
                    float efficiency = scoreManager.GetEfficiency();
                    efficiencyText.text = $"{efficiency:F0}%";
                    efficiencyText.color = GetEfficiencyColor(efficiency);
                }
            }
        }
        
        #endregion
        
        #region Visual Flair
        
        /// <summary>
        /// Gets combo text based on streak level
        /// </summary>
        /// <param name="combo"></param>
        private string GetComboText(int combo)
        {
            if (combo >= 5) return $"COMBO x{combo}!!";
            if (combo >= 3) return $"COMBO x{combo}!";
            return $"Combo x{combo}";
        }
        
        /// <summary>
        /// Gets combo color based on streak level
        /// </summary>
        /// <param name="combo"></param>
        private Color GetComboColor(int combo)
        {
            if (combo >= 5) return new Color(1f, 0.3f, 0f); // Red-Orange
            if (combo >= 3) return new Color(1f, 0.8f, 0f); // Gold
            return Color.white;
        }
        
        /// <summary>
        /// Gets efficiency color based on performance
        /// </summary>
        /// <param name="efficiency"></param>
        private Color GetEfficiencyColor(float efficiency)
        {
            if (efficiency >= 90f) return new Color(0f, 1f, 0f); // Green
            if (efficiency >= 75f) return new Color(0.5f, 1f, 0f); // Yellow-Green
            if (efficiency >= 60f) return new Color(1f, 0.8f, 0f); // Yellow
            if (efficiency >= 40f) return new Color(1f, 0.5f, 0f); // Orange
            return new Color(1f, 0f, 0f); // Red
        }
        
        #endregion
        
        #region Game Over
        
        /// <summary>
        /// Shows game over screen
        /// </summary>
        private void ShowGameOver()
        {
            StopTimer();
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            
            if (gameOverText != null && scoreManager != null)
            {
                string performanceBadge = GetPerformanceBadge();
                gameOverText.text = $"GAME COMPLETE!\n\n" +
                                   $"Score: {scoreManager.GetCurrentScore()}\n" +
                                   $"Time: {gameTime:F1}s\n" +
                                   $"Efficiency: {scoreManager.GetEfficiency():F0}%\n" +
                                   $"Best Combo: x{scoreManager.GetHighestCombo()}\n\n" +
                                   $"{performanceBadge}";
            }
            
            Debug.Log("=== GAME OVER ===");
        }
        
        /// <summary>
        /// Gets performance badge based on efficiency and combo
        /// </summary>
        private string GetPerformanceBadge()
        {
            if (scoreManager == null) return "";
            
            float efficiency = scoreManager.GetEfficiency();
            int combo = scoreManager.GetHighestCombo();
            
            if (efficiency == 100f) return "PERFECT!";
            if (efficiency >= 90f && combo >= 5) return "MASTER!";
            if (efficiency >= 80f) return "EXPERT!";
            if (efficiency >= 70f) return "SKILLED!";
            return "GOOD TRY!";
        }
        
        /// <summary>
        /// Hides game over screen
        /// </summary>
        public void HideGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }
        
        #endregion
        
        #region Reset UI
        
        /// <summary>
        /// Resets all UI elements to initial state
        /// </summary>
        public void ResetUI()
        {
            if (scoreText != null) scoreText.text = "Score: 0";
            if (matchesText != null) matchesText.text = "0";
            if (turnsText != null) turnsText.text = "0";
            if (comboText != null) comboText.text = "";
            UpdateEfficiency();
            
            ResetTimer();
            HideGameOver();
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            Cleanup();
        }
        
        #endregion
    }
}