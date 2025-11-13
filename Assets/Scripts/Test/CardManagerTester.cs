using UnityEngine;
using FirstRound;
using System.Collections.Generic;

/// <summary>
/// Test script for CardManager functionality
/// Tests card matching, mismatching, and continuous flipping
/// </summary>
public class CardManagerTester : MonoBehaviour
{
    [Header("Card Manager")]
    [SerializeField] private CardManager cardManager;
    
    [Header("Test Cards")]
    [SerializeField] private Card card1;
    [SerializeField] private Card card2;
    [SerializeField] private Card card3;
    [SerializeField] private Card card4;
    
    [Header("Card Data (2 Pairs)")]
    [SerializeField] private CardData cardData1; // Cherry - ID 1
    [SerializeField] private CardData cardData2; // Cherry - ID 1
    [SerializeField] private CardData cardData3; // Apple - ID 2
    [SerializeField] private CardData cardData4; // Apple - ID 2
    
    [Header("Visual")]
    [SerializeField] private Sprite backSprite;
    
    private void Start()
    {
        if (!ValidateReferences())
        {
            Debug.LogError("Missing references! Check inspector.");
            return;
        }
        
        InitializeTest();
    }
    
    private bool ValidateReferences()
    {
        return cardManager != null && 
               card1 != null && card2 != null && card3 != null && card4 != null &&
               cardData1 != null && cardData2 != null && cardData3 != null && cardData4 != null &&
               backSprite != null;
    }
    
    private void InitializeTest()
    {
        // Initialize all cards
        card1.Initialize(cardData1, backSprite, 0);
        card2.Initialize(cardData2, backSprite, 1);
        card3.Initialize(cardData3, backSprite, 2);
        card4.Initialize(cardData4, backSprite, 3);
        
        // Register cards with manager
        List<Card> cards = new List<Card> { card1, card2, card3, card4 };
        cardManager.RegisterCards(cards);
        
        // Subscribe to events
        cardManager.OnCardsMatched += OnCardsMatched;
        cardManager.OnCardsMismatched += OnCardsMismatched;
        cardManager.OnAllCardsMatched += OnAllCardsMatched;
        
        Debug.Log("=== CardManager Test Initialized ===");
        Debug.Log("Card1 & Card2 = Cherry (ID: 1) - Should MATCH");
        Debug.Log("Card3 & Card4 = Apple (ID: 2) - Should MATCH");
        Debug.Log("Click any 2 cards to test matching!");
    }
    
    private void OnCardsMatched(Card card1, Card card2)
    {
        Debug.Log($"<color=green>✓ MATCH!</color> Cards matched: ID {card1.GetCardID()}");
        Debug.Log($"Total matches: {cardManager.GetTotalMatches()}");
    }
    
    private void OnCardsMismatched(Card card1, Card card2)
    {
        Debug.Log($"<color=red>✗ MISMATCH!</color> Card IDs: {card1.GetCardID()} vs {card2.GetCardID()}");
        Debug.Log("Cards will flip back in 1 second...");
    }
    
    private void OnAllCardsMatched()
    {
        Debug.Log("<color=yellow>★ ALL CARDS MATCHED! GAME COMPLETE! ★</color>");
    }
    
    private void Update()
    {
        // Press R to reset game
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("=== RESETTING GAME ===");
            cardManager.ResetGame();
            
            // Re-initialize cards
            card1.Initialize(cardData1, backSprite, 0);
            card2.Initialize(cardData2, backSprite, 1);
            card3.Initialize(cardData3, backSprite, 2);
            card4.Initialize(cardData4, backSprite, 3);
        }
        
        // Press I to show info
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log($"--- Game Info ---");
            Debug.Log($"Total Cards: {cardManager.GetTotalCardCount()}");
            Debug.Log($"Matched Cards: {cardManager.GetMatchedCardCount()}");
            Debug.Log($"Total Matches: {cardManager.GetTotalMatches()}");
            Debug.Log($"Game Complete: {cardManager.IsGameComplete()}");
        }
    }
    
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