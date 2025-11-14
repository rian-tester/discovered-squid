using UnityEngine;
using FirstRound;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Test script for ScoreManager with TextMeshPro UI display
/// </summary>
public class ScoreManagerTester : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private ScoreManager scoreManager;
    
    [Header("UI Elements - TextMeshPro")]
    [SerializeField] private TMP_Text matchesText;
    [SerializeField] private TMP_Text turnsText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text playerIDText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text efficiencyText;
    
    [Header("Test Configuration")]
    [SerializeField] private bool autoGenerateOnStart = true;
    
    // Player data
    private string playerID;
    private float gameTime = 0f;
    private bool isGameActive = false;
    
    private void Start()
    {
        if (!ValidateReferences())
        {
            Debug.LogError("Missing references! Check inspector.");
            return;
        }
        
        // Generate player ID
        playerID = GeneratePlayerID();
        if (playerIDText != null)
            playerIDText.text = "Player: " + playerID;
        
        // Initialize ScoreManager with CardManager
        scoreManager.Initialize(cardManager);
        
        // Subscribe to score events for UI updates
        scoreManager.OnScoreChanged += UpdateScoreUI;
        scoreManager.OnMatchesChanged += UpdateMatchesUI;
        scoreManager.OnTurnsChanged += UpdateTurnsUI;
        scoreManager.OnComboChanged += UpdateComboUI;
        
        if (autoGenerateOnStart)
        {
            GenerateNewGrid();
        }
        
        Debug.Log("=== ScoreManager Test Ready ===");
        Debug.Log($"Player ID: {playerID}");
    }
    
    private bool ValidateReferences()
    {
        if (gridManager == null || cardManager == null || scoreManager == null)
        {
            Debug.LogError("Missing manager references!");
            return false;
        }
        return true;
    }
    
    private void Update()
    {
        // Update timer
        if (isGameActive)
        {
            gameTime += Time.deltaTime;
            UpdateTimerUI();
        }
        
        // Keyboard controls
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Starting new game...");
            GenerateNewGrid();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Resetting scores...");
            scoreManager.ResetScore();
            UpdateAllUI();
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowGameInfo();
        }
    }
    
    #region Game Generation
    
    private void GenerateNewGrid()
    {
        cardManager.ClearAllCards();
        scoreManager.ResetScore();
        
        gameTime = 0f;
        isGameActive = true;
        
        List<Card> cards = gridManager.GenerateGrid();
        
        if (cards == null || cards.Count == 0)
        {
            Debug.LogError("Failed to generate grid!");
            return;
        }
        
        cardManager.RegisterCards(cards);
        
        cardManager.OnCardsMatched += OnCardsMatched;
        cardManager.OnCardsMismatched += OnCardsMismatched;
        cardManager.OnAllCardsMatched += OnAllCardsMatched;
        
        Debug.Log("New game started!");
        UpdateAllUI();
    }
    
    #endregion
    
    #region Player ID Generation
    
    private string GeneratePlayerID()
    {
        string machineID = SystemInfo.deviceUniqueIdentifier;
        return "PC-" + machineID.Substring(0, 6).ToUpper();
    }
    
    #endregion
    
    #region UI Updates
    
    private void UpdateScoreUI(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }
    
    private void UpdateMatchesUI(int matches)
    {
        if (matchesText != null)
            matchesText.text = matches.ToString();
    }
    
    private void UpdateTurnsUI(int turns)
    {
        if (turnsText != null)
            turnsText.text = turns.ToString();
    }
    
    private void UpdateComboUI(int combo)
    {
        if (comboText != null)
        {
            if (combo > 1)
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
    
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = (int)(gameTime / 60f);
            int seconds = (int)(gameTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
    
    private void UpdateEfficiencyUI()
    {
        if (efficiencyText != null)
        {
            float efficiency = scoreManager.GetEfficiency();
            efficiencyText.text = $"{efficiency:F0}%";
            
            // Color code the efficiency text
            efficiencyText.color = GetEfficiencyColor(efficiency);
        }
    }
    
    private void UpdateAllUI()
    {
        UpdateScoreUI(scoreManager.GetCurrentScore());
        UpdateMatchesUI(scoreManager.GetTotalMatches());
        UpdateTurnsUI(scoreManager.GetTotalTurns());
        UpdateComboUI(scoreManager.GetCurrentCombo());
        UpdateEfficiencyUI();
    }
    
    #endregion
    
    #region Flair Functions
    
    private Color GetComboColor(int combo)
    {
        if (combo >= 5) return new Color(1f, 0.3f, 0f); // Red-Orange
        if (combo >= 3) return new Color(1f, 0.8f, 0f); // Gold
        return Color.white;
    }
    
    private string GetComboText(int combo)
    {
        if (combo >= 5) return $"COMBO x{combo}!!";
        if (combo >= 3) return $"COMBO x{combo}!";
        return $"Combo x{combo}";
    }
    
    private Color GetEfficiencyColor(float efficiency)
    {
        if (efficiency >= 90f) return new Color(0f, 1f, 0f); // Green - Excellent
        if (efficiency >= 75f) return new Color(0.5f, 1f, 0f); // Yellow-Green - Great
        if (efficiency >= 60f) return new Color(1f, 0.8f, 0f); // Yellow - Good
        if (efficiency >= 40f) return new Color(1f, 0.5f, 0f); // Orange - Okay
        return new Color(1f, 0f, 0f); // Red - Poor
    }
    
    private string GetPerformanceBadge()
    {
        float efficiency = scoreManager.GetEfficiency();
        int combo = scoreManager.GetHighestCombo();
        
        if (efficiency == 100f) return "PERFECT!";
        if (efficiency >= 90f && combo >= 5) return "MASTER!";
        if (efficiency >= 80f) return "EXPERT!";
        if (efficiency >= 70f) return "SKILLED!";
        return "GOOD TRY!";
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnCardsMatched(Card card1, Card card2)
    {
        Debug.Log($"<color=green>MATCH!</color> Score: {scoreManager.GetCurrentScore()}");
        UpdateEfficiencyUI();
    }
    
    private void OnCardsMismatched(Card card1, Card card2)
    {
        Debug.Log($"<color=red>MISMATCH!</color>");
        UpdateEfficiencyUI();
    }
    
    private void OnAllCardsMatched()
    {
        isGameActive = false;
        Debug.Log("<color=yellow>*** GAME COMPLETE! ***</color>");
        ShowGameInfo();
    }
    
    #endregion
    
    private void ShowGameInfo()
    {
        Debug.Log("=== GAME STATISTICS ===");
        Debug.Log($"Player: {playerID}");
        Debug.Log($"Time: {gameTime:F2} seconds");
        Debug.Log($"Final Score: {scoreManager.GetCurrentScore()}");
        Debug.Log($"Total Matches: {scoreManager.GetTotalMatches()}");
        Debug.Log($"Total Turns: {scoreManager.GetTotalTurns()}");
        Debug.Log($"Highest Combo: x{scoreManager.GetHighestCombo()}");
        Debug.Log($"Efficiency: {scoreManager.GetEfficiency()}%");
        Debug.Log($"Rating: {GetPerformanceBadge()}");
    }
    
    private void OnDestroy()
    {
        if (scoreManager != null)
        {
            scoreManager.Cleanup(cardManager);
            scoreManager.OnScoreChanged -= UpdateScoreUI;
            scoreManager.OnMatchesChanged -= UpdateMatchesUI;
            scoreManager.OnTurnsChanged -= UpdateTurnsUI;
            scoreManager.OnComboChanged -= UpdateComboUI;
        }
        
        if (cardManager != null)
        {
            cardManager.OnCardsMatched -= OnCardsMatched;
            cardManager.OnCardsMismatched -= OnCardsMismatched;
            cardManager.OnAllCardsMatched -= OnAllCardsMatched;
        }
    }
}