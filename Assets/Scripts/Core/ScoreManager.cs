using UnityEngine;
using System;

namespace FirstRound
{
    /// <summary>
    /// Manages game scoring, turns tracking, and combo system
    /// Integrates with CardManager to track matches and calculate scores
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [Header("Scoring Configuration")]
        [SerializeField] private int matchScore = 100;
        [SerializeField] private int comboMultiplier = 50;
        [SerializeField] private int maxCombo = 10;
        
        // Score tracking
        private int currentScore = 0;
        private int totalMatches = 0;
        private int totalTurns = 0;
        private int currentCombo = 0;
        private int highestCombo = 0;
        
        // Events
        public event Action<int> OnScoreChanged;
        public event Action<int> OnMatchesChanged;
        public event Action<int> OnTurnsChanged;
        public event Action<int> OnComboChanged;
        
        #region Initialization
        
        /// <summary>
        /// Subscribes to CardManager events
        /// </summary>
        /// <param name="cardManager"></param>
        public void Initialize(CardManager cardManager)
        {
            if (cardManager == null)
            {
                Debug.LogError("CardManager is null! Cannot initialize ScoreManager.");
                return;
            }
            
            cardManager.OnCardsMatched += HandleMatch;
            cardManager.OnCardsMismatched += HandleMismatch;
            cardManager.OnAllCardsMatched += HandleGameComplete;
        }
        
        /// <summary>
        /// Unsubscribes from CardManager events
        /// </summary>
        /// <param name="cardManager"></param>
        public void Cleanup(CardManager cardManager)
        {
            if (cardManager != null)
            {
                cardManager.OnCardsMatched -= HandleMatch;
                cardManager.OnCardsMismatched -= HandleMismatch;
                cardManager.OnAllCardsMatched -= HandleGameComplete;
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handles successful match event
        /// </summary>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        private void HandleMatch(Card card1, Card card2)
        {
            // Increment turn counter
            totalTurns++;
            OnTurnsChanged?.Invoke(totalTurns);
            
            // Increment match counter
            totalMatches++;
            OnMatchesChanged?.Invoke(totalMatches);
            
            // Increment combo
            currentCombo++;
            if (currentCombo > highestCombo)
            {
                highestCombo = currentCombo;
            }
            OnComboChanged?.Invoke(currentCombo);
            
            // Calculate score with combo bonus
            int earnedScore = CalculateMatchScore();
            currentScore += earnedScore;
            OnScoreChanged?.Invoke(currentScore);
            
            Debug.Log($"Match! Score +{earnedScore} | Combo: x{currentCombo}");
        }
        
        /// <summary>
        /// Handles mismatch event
        /// </summary>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        private void HandleMismatch(Card card1, Card card2)
        {
            // Increment turn counter
            totalTurns++;
            OnTurnsChanged?.Invoke(totalTurns);
            
            // Reset combo on mismatch
            if (currentCombo > 0)
            {
                Debug.Log($"Combo broken! Was at x{currentCombo}");
                currentCombo = 0;
                OnComboChanged?.Invoke(currentCombo);
            }
        }
        
        /// <summary>
        /// Handles game completion event
        /// </summary>
        private void HandleGameComplete()
        {
            Debug.Log("=== GAME COMPLETE ===");
            Debug.Log($"Final Score: {currentScore}");
            Debug.Log($"Total Matches: {totalMatches}");
            Debug.Log($"Total Turns: {totalTurns}");
            Debug.Log($"Highest Combo: x{highestCombo}");
            Debug.Log($"Efficiency: {CalculateEfficiency()}%");
        }
        
        #endregion
        
        #region Score Calculation
        
        /// <summary>
        /// Calculates score for a match including combo bonus
        /// </summary>
        private int CalculateMatchScore()
        {
            int baseScore = matchScore;
            
            // Apply combo multiplier (capped at maxCombo)
            int effectiveCombo = Mathf.Min(currentCombo, maxCombo);
            int comboBonus = (effectiveCombo - 1) * comboMultiplier;
            
            return baseScore + comboBonus;
        }
        
        /// <summary>
        /// Calculates game efficiency percentage
        /// </summary>
        private float CalculateEfficiency()
        {
            if (totalTurns == 0) return 0f;
            
            // Perfect game = totalTurns equals totalMatches
            float efficiency = ((float)totalMatches / (float)totalTurns) * 100f;
            return Mathf.Round(efficiency * 100f) / 100f;
        }
        
        #endregion
        
        #region Reset
        
        /// <summary>
        /// Resets all scoring data
        /// </summary>
        public void ResetScore()
        {
            currentScore = 0;
            totalMatches = 0;
            totalTurns = 0;
            currentCombo = 0;
            highestCombo = 0;
            
            OnScoreChanged?.Invoke(currentScore);
            OnMatchesChanged?.Invoke(totalMatches);
            OnTurnsChanged?.Invoke(totalTurns);
            OnComboChanged?.Invoke(currentCombo);
        }
        
        #endregion
        
        #region Getters
        
        public int GetCurrentScore() => currentScore;
        
        public int GetTotalMatches() => totalMatches;
        
        public int GetTotalTurns() => totalTurns;
        
        public int GetCurrentCombo() => currentCombo;
        
        public int GetHighestCombo() => highestCombo;
        
        public float GetEfficiency() => CalculateEfficiency();
        
        #endregion
        
        #region Configuration Setters
        
        public void SetMatchScore(int score)
        {
            matchScore = score;
        }
        
        public void SetComboMultiplier(int multiplier)
        {
            comboMultiplier = multiplier;
        }
        
        public void SetMaxCombo(int max)
        {
            maxCombo = max;
        }
        
        #endregion
    }
}