using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace FirstRound
{
    /// <summary>
    /// Manages card selection, matching logic, and game flow
    /// Handles continuous card flipping without blocking input
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        [Header("Match Settings")]
        [SerializeField] private float mismatchFlipBackDelay = 1f;
        [SerializeField] private float matchDisappearDelay = 0.5f;
        
        // Card tracking
        private List<Card> allCards = new List<Card>();
        private List<Card> flippedCards = new List<Card>();
        private HashSet<Card> matchedCards = new HashSet<Card>();
        
        // State management
        private bool isProcessingMatch = false;
        private int totalMatches = 0;
        
        // Events
        public event Action<Card, Card> OnCardsMatched;
        public event Action<Card, Card> OnCardsMismatched;
        public event Action OnAllCardsMatched;
        
        #region Initialization
        
        /// <summary>
        /// Registers a card with the manager
        /// </summary>
        /// <param name="card"></param>
        public void RegisterCard(Card card)
        {
            if (!allCards.Contains(card))
            {
                allCards.Add(card);
                card.OnCardClicked += HandleCardClicked;
            }
        }
        
        /// <summary>
        /// Unregisters a card from the manager
        /// </summary>
        /// <param name="card"></param>
        public void UnregisterCard(Card card)
        {
            if (allCards.Contains(card))
            {
                allCards.Remove(card);
                card.OnCardClicked -= HandleCardClicked;
            }
        }
        
        /// <summary>
        /// Registers multiple cards at once
        /// </summary>
        /// <param name="cards"></param>
        public void RegisterCards(List<Card> cards)
        {
            foreach (Card card in cards)
            {
                RegisterCard(card);
            }
        }
        
        /// <summary>
        /// Clears all registered cards
        /// </summary>
        public void ClearAllCards()
        {
            foreach (Card card in allCards)
            {
                card.OnCardClicked -= HandleCardClicked;
            }
            
            allCards.Clear();
            flippedCards.Clear();
            matchedCards.Clear();
            totalMatches = 0;
        }
        
        #endregion
        
        #region Card Selection Logic
        
        /// <summary>
        /// Handles card click events
        /// </summary>
        /// <param name="card"></param>
        private void HandleCardClicked(Card card)
        {
            // Prevent clicking same card twice
            if (flippedCards.Contains(card))
                return;
            
            // Prevent clicking matched cards
            if (matchedCards.Contains(card))
                return;
            
            // Flip the card
            card.FlipToFront();
            flippedCards.Add(card);
            
            // Check if we have 2 cards flipped
            if (flippedCards.Count >= 2 && !isProcessingMatch)
            {
                StartCoroutine(ProcessMatchCheck());
            }
        }
        
        #endregion
        
        #region Match Processing
        
        /// <summary>
        /// Checks if the two flipped cards match
        /// </summary>
        private IEnumerator ProcessMatchCheck()
        {
            isProcessingMatch = true;
            
            // Wait a brief moment to show both cards
            yield return new WaitForSeconds(0.3f);
            
            Card firstCard = flippedCards[0];
            Card secondCard = flippedCards[1];
            
            // Check if cards match by ID
            if (firstCard.GetCardID() == secondCard.GetCardID())
            {
                // Match found
                HandleMatch(firstCard, secondCard);
            }
            else
            {
                // No match
                HandleMismatch(firstCard, secondCard);
            }
            
            isProcessingMatch = false;
        }
        
        /// <summary>
        /// Handles matched cards
        /// </summary>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        private void HandleMatch(Card card1, Card card2)
        {
            // Add to matched set
            matchedCards.Add(card1);
            matchedCards.Add(card2);
            
            // Remove from flipped cards
            flippedCards.Remove(card1);
            flippedCards.Remove(card2);
            
            // Trigger event
            OnCardsMatched?.Invoke(card1, card2);
            
            // Play match animations
            StartCoroutine(PlayMatchAnimations(card1, card2));
            
            totalMatches++;
            
            // Check if all cards are matched
            if (matchedCards.Count == allCards.Count)
            {
                OnAllCardsMatched?.Invoke();
            }
        }
        
        /// <summary>
        /// Handles mismatched cards
        /// </summary>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        private void HandleMismatch(Card card1, Card card2)
        {
            // Trigger event
            OnCardsMismatched?.Invoke(card1, card2);
            
            // Flip cards back after delay
            StartCoroutine(FlipCardsBack(card1, card2));
        }
        
        /// <summary>
        /// Plays match animations for both cards
        /// </summary>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        private IEnumerator PlayMatchAnimations(Card card1, Card card2)
        {
            yield return new WaitForSeconds(matchDisappearDelay);
            
            card1.PlayMatchedAnimation();
            card2.PlayMatchedAnimation();
        }
        
        /// <summary>
        /// Flips mismatched cards back to face down
        /// </summary>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        private IEnumerator FlipCardsBack(Card card1, Card card2)
        {
            yield return new WaitForSeconds(mismatchFlipBackDelay);
            
            // Remove from flipped cards
            flippedCards.Remove(card1);
            flippedCards.Remove(card2);
            
            // Flip back
            card1.FlipToBack();
            card2.FlipToBack();
        }
        
        #endregion
        
        #region Reset and Utilities
        
        /// <summary>
        /// Resets the game state
        /// </summary>
        public void ResetGame()
        {
            StopAllCoroutines();
            
            flippedCards.Clear();
            matchedCards.Clear();
            isProcessingMatch = false;
            totalMatches = 0;
            
            // Reset all cards to face down
            foreach (Card card in allCards)
            {
                if (card.GetState() != CardState.FaceDown)
                {
                    card.FlipToBack();
                }
            }
        }
        
        /// <summary>
        /// Shuffles the card data using Fisher-Yates algorithm
        /// </summary>
        /// <param name="cardDataList"></param>
        public static void ShuffleCards<T>(List<T> cardDataList)
        {
            System.Random random = new System.Random();
            int n = cardDataList.Count;
            
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                T temp = cardDataList[i];
                cardDataList[i] = cardDataList[j];
                cardDataList[j] = temp;
            }
        }
        
        #endregion
        
        #region Getters
        
        public int GetTotalMatches() => totalMatches;
        
        public int GetMatchedCardCount() => matchedCards.Count;
        
        public int GetTotalCardCount() => allCards.Count;
        
        public bool IsGameComplete() => matchedCards.Count == allCards.Count && allCards.Count > 0;
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            ClearAllCards();
        }
        
        #endregion
    }
}