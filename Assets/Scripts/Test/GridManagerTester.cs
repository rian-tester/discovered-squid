using UnityEngine;
using FirstRound;
using System.Collections.Generic;

/// <summary>
/// Test script for GridManager functionality
/// Tests grid generation, card spawning, and integration with CardManager
/// </summary>
public class GridManagerTester : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private CardManager cardManager;
    
    [Header("Test Configurations")]
    [SerializeField] private bool autoGenerateOnStart = true;
    [SerializeField] private bool enableAutoScale = true;
    [SerializeField] private Vector2 targetScaleArea = new Vector2(1600f, 1000f);
    
    private void Start()
    {
        if (gridManager == null || cardManager == null)
        {
            Debug.LogError("Please assign GridManager and CardManager!");
            return;
        }
        
        if (autoGenerateOnStart)
        {
            GenerateNewGrid();
        }
        
        Debug.Log("=== GridManager Test Ready ===");
        Debug.Log("Press G - Generate new grid");
        Debug.Log("Press 1 - 2x2 grid");
        Debug.Log("Press 2 - 3x3 grid (invalid - odd total)");
        Debug.Log("Press 3 - 4x4 grid");
        Debug.Log("Press 4 - 5x6 grid");
        Debug.Log("Press R - Reset current game");
        Debug.Log("Press I - Show grid info");
    }
    
    private void Update()
    {
        // Generate new grid
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Generating new grid...");
            GenerateNewGrid();
        }
        
        // Test different grid sizes
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Generating 2x2 grid...");
            GenerateGridWithSize(2, 2);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Attempting 3x3 grid (should fail - odd total)...");
            GenerateGridWithSize(3, 3);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Generating 4x4 grid...");
            GenerateGridWithSize(4, 4);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Generating 5x6 grid...");
            GenerateGridWithSize(5, 6);
        }
        
        // Reset game
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Resetting game...");
            cardManager.ResetGame();
        }
        
        // Show info
        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowGridInfo();
        }
    }
    
    /// <summary>
    /// Generates a new grid with current settings
    /// </summary>
    private void GenerateNewGrid()
    {
        // Clear previous game
        cardManager.ClearAllCards();
        
        // Generate grid
        List<Card> cards = gridManager.GenerateGrid();
        
        if (cards == null || cards.Count == 0)
        {
            Debug.LogError("Failed to generate grid!");
            return;
        }
        
        // Register all cards with CardManager
        cardManager.RegisterCards(cards);
        
        // Subscribe to events
        cardManager.OnCardsMatched += OnCardsMatched;
        cardManager.OnCardsMismatched += OnCardsMismatched;
        cardManager.OnAllCardsMatched += OnAllCardsMatched;
        
        // Optional: Auto-scale to fit
        if (enableAutoScale)
        {
            gridManager.ScaleToFit(targetScaleArea.x, targetScaleArea.y);
        }
        
        Debug.Log($"<color=green>✓ Grid generated successfully!</color>");
        Debug.Log($"Grid: {gridManager.GetRows()}x{gridManager.GetColumns()} = {cards.Count} cards");
        Debug.Log($"Pairs needed: {cards.Count / 2}");
    }
    
    /// <summary>
    /// Generates a grid with custom dimensions
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="columns"></param>
    private void GenerateGridWithSize(int rows, int columns)
    {
        gridManager.SetGridSize(rows, columns);
        GenerateNewGrid();
    }
    
    /// <summary>
    /// Displays current grid information
    /// </summary>
    private void ShowGridInfo()
    {
        Debug.Log("=== Grid Information ===");
        Debug.Log($"Grid Size: {gridManager.GetRows()}x{gridManager.GetColumns()}");
        Debug.Log($"Total Cards: {gridManager.GetTotalCards()}");
        Debug.Log($"Grid Dimensions: {gridManager.GetGridSize()}");
        Debug.Log($"Matched Cards: {cardManager.GetMatchedCardCount()}/{cardManager.GetTotalCardCount()}");
        Debug.Log($"Total Matches: {cardManager.GetTotalMatches()}");
        Debug.Log($"Game Complete: {cardManager.IsGameComplete()}");
    }
    
    #region Event Handlers
    
    private void OnCardsMatched(Card card1, Card card2)
    {
        Debug.Log($"<color=green>✓ MATCH!</color> Card ID: {card1.GetCardID()}");
        Debug.Log($"Progress: {cardManager.GetMatchedCardCount()}/{cardManager.GetTotalCardCount()} cards matched");
    }
    
    private void OnCardsMismatched(Card card1, Card card2)
    {
        Debug.Log($"<color=red>✗ MISMATCH!</color> IDs: {card1.GetCardID()} vs {card2.GetCardID()}");
    }
    
    private void OnAllCardsMatched()
    {
        Debug.Log("<color=yellow>★★★ ALL CARDS MATCHED! GAME COMPLETE! ★★★</color>");
    }
    
    #endregion
    
    private void OnDestroy()
    {
        if (cardManager != null)
        {
            cardManager.OnCardsMatched -= OnCardsMatched;
            cardManager.OnCardsMismatched -= OnCardsMismatched;
            cardManager.OnAllCardsMatched -= OnAllCardsMatched;
        }
    }
}