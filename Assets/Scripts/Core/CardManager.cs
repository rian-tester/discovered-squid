using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace FirstRound
{
    /// <summary>
    /// Manages card selection, matching logic, and game flow
    /// Handles continuous card flipping without blocking input using queue system
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        [Header("Match Settings")]
        [SerializeField] private float mismatchFlipBackDelay = 1f;
        [SerializeField] private float matchDisappearDelay = 0.5f;
        
        // Card tracking
        private List<Card> allCards = new List<Card>();
        private Queue<Card> cardQueue = new Queue<Card>();
        private HashSet<Card> cardsInQueue = new HashSet<Card>();
        private HashSet<Card> matchedCards = new HashSet<Card>();
        
        // Current processing pair
        private Card currentFirstCard = null;
        private Card currentSecondCard = null;
        
        // State management
        private bool isProcessingPair = false;
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
            cardQueue.Clear();
            cardsInQueue.Clear();
            matchedCards.Clear();
            currentFirstCard = null;
            currentSecondCard = null;
            totalMatches = 0;
        }
        
        #endregion
        
        #region Card Selection Logic
        
        /// <summary>
        /// Handles card click events and adds to queue
        /// </summary>
        /// <param name="card"></param>
        private void HandleCardClicked(Card card)
        {
            // Prevent clicking matched cards
            if (matchedCards.Contains(card))
                return;
            
            // Prevent clicking same card multiple times
            if (cardsInQueue.Contains(card))
                return;
            
            // Prevent clicking cards in current processing pair
            if (card == currentFirstCard || card == currentSecondCard)
                return;
            
            // Add to queue
            cardQueue.Enqueue(card);
            cardsInQueue.Add(card);
            
            // Flip the card immediately for visual feedback
            card.FlipToFront();
            
            // Start processing if not already running
            if (!isProcessingPair)
            {
                StartCoroutine(ProcessQueue());
            }
        }
        
        #endregion
        
        #region Queue Processing
        
        /// <summary>
        /// Processes card pairs from the queue in order
        /// </summary>
        private IEnumerator ProcessQueue()
        {
            while (cardQueue.Count >= 2)
            {
                isProcessingPair = true;
                
                // Dequeue two cards
                currentFirstCard = cardQueue.Dequeue();
                currentSecondCard = cardQueue.Dequeue();
                
                // Remove from tracking set
                cardsInQueue.Remove(currentFirstCard);
                cardsInQueue.Remove(currentSecondCard);
                
                // Wait a brief moment to show both cards
                yield return new WaitForSeconds(0.3f);
                
                // Check if cards match by ID
                if (currentFirstCard.GetCardID() == currentSecondCard.GetCardID())
                {
                    // Match found
                    HandleMatch(currentFirstCard, currentSecondCard);
                }
                else
                {
                    // No match
                    HandleMismatch(currentFirstCard, currentSecondCard);
                }
                
                // Wait before processing next pair
                yield return new WaitForSeconds(0.2f);
            }
            
            // Clear current pair references
            currentFirstCard = null;
            currentSecondCard = null;
            isProcessingPair = false;
        }
        
        #endregion
        
        #region Match Processing
        
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
            
            cardQueue.Clear();
            cardsInQueue.Clear();
            matchedCards.Clear();
            currentFirstCard = null;
            currentSecondCard = null;
            isProcessingPair = false;
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
        
        public int GetQueuedCardCount() => cardQueue.Count;
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            ClearAllCards();
        }
        
        #endregion
    }
}