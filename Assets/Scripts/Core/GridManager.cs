using UnityEngine;
using System.Collections.Generic;

namespace FirstRound
{
    /// <summary>
    /// Manages grid layout and card spawning
    /// Handles dynamic sizing for various grid configurations (2x2, 3x3, 5x6, etc.)
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField] private int rows = 4;
        [SerializeField] private int columns = 4;
        [SerializeField] private Vector2 cardSize = new Vector2(150f, 150f);
        [SerializeField] private Vector2 spacing = new Vector2(10f, 10f);
        
        [Header("Container")]
        [SerializeField] private RectTransform gridContainer;
        
        [Header("Card Prefab")]
        [SerializeField] private GameObject cardPrefab;
        
        [Header("Visual")]
        [SerializeField] private Sprite backSprite;
        
        [Header("Card Data Pool")]
        [SerializeField] private List<CardData> availableCardData = new List<CardData>();
        
        // Spawned cards tracking
        private List<Card> spawnedCards = new List<Card>();
        
        // Grid dimensions
        private Vector2 calculatedGridSize;
        
        #region Grid Generation
        
        /// <summary>
        /// Generates the grid with specified rows and columns
        /// </summary>
        public List<Card> GenerateGrid()
        {
            ClearGrid();
            
            int totalCards = rows * columns;
            
            // Validate even number of cards
            if (totalCards % 2 != 0)
            {
                Debug.LogError("Grid must have even number of cards for matching pairs!");
                return null;
            }
            
            // Validate card data availability
            int requiredPairs = totalCards / 2;
            if (availableCardData.Count < requiredPairs)
            {
                Debug.LogError($"Not enough card data! Need {requiredPairs}, have {availableCardData.Count}");
                return null;
            }
            
            // Generate card data pairs
            List<CardData> shuffledData = GenerateShuffledPairs(requiredPairs);
            
            // Calculate grid dimensions
            CalculateGridDimensions();
            
            // Spawn cards
            for (int i = 0; i < totalCards; i++)
            {
                int row = i / columns;
                int col = i % columns;
                
                Card card = SpawnCard(shuffledData[i], row, col, i);
                spawnedCards.Add(card);
            }
            
            return spawnedCards;
        }
        
        /// <summary>
        /// Generates the grid with custom configuration
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        public List<Card> GenerateGrid(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;
            return GenerateGrid();
        }
        
        #endregion
        
        #region Card Data Generation
        
        /// <summary>
        /// Generates pairs of card data and shuffles them using Fisher-Yates
        /// </summary>
        /// <param name="pairCount"></param>
        private List<CardData> GenerateShuffledPairs(int pairCount)
        {
            List<CardData> cardDataPairs = new List<CardData>();
            
            // Create pairs
            for (int i = 0; i < pairCount; i++)
            {
                CardData data = availableCardData[i];
                cardDataPairs.Add(data);
                cardDataPairs.Add(data);
            }
            
            // Shuffle using Fisher-Yates algorithm
            ShuffleList(cardDataPairs);
            
            return cardDataPairs;
        }
        
        /// <summary>
        /// Fisher-Yates shuffle algorithm for randomizing card positions
        /// </summary>
        /// <param name="list"></param>
        private void ShuffleList<T>(List<T> list)
        {
            System.Random random = new System.Random();
            int n = list.Count;
            
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
        
        #endregion
        
        #region Card Spawning
        
        /// <summary>
        /// Spawns a single card at specified grid position
        /// </summary>
        /// <param name="data"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="index"></param>
        private Card SpawnCard(CardData data, int row, int col, int index)
        {
            // Instantiate card
            GameObject cardObj = Instantiate(cardPrefab, gridContainer);
            cardObj.name = $"Card_{row}_{col}";
            
            // Get card component
            Card card = cardObj.GetComponent<Card>();
            if (card == null)
            {
                Debug.LogError("Card prefab must have Card component!");
                Destroy(cardObj);
                return null;
            }
            
            // Initialize card
            card.Initialize(data, backSprite, index);
            
            // Position card
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            PositionCard(cardRect, row, col);
            
            return card;
        }
        
        #endregion
        
        #region Grid Layout Calculations
        
        /// <summary>
        /// Calculates total grid dimensions based on card size and spacing
        /// </summary>
        private void CalculateGridDimensions()
        {
            float totalWidth = (columns * cardSize.x) + ((columns - 1) * spacing.x);
            float totalHeight = (rows * cardSize.y) + ((rows - 1) * spacing.y);
            
            calculatedGridSize = new Vector2(totalWidth, totalHeight);
            
            // Optionally resize container to fit grid
            if (gridContainer != null)
            {
                gridContainer.sizeDelta = calculatedGridSize;
            }
        }
        
        /// <summary>
        /// Positions a card at the specified grid coordinates
        /// </summary>
        /// <param name="cardRect"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        private void PositionCard(RectTransform cardRect, int row, int col)
        {
            // Set card size
            cardRect.sizeDelta = cardSize;
            
            // Calculate position (centered in grid)
            float startX = -(calculatedGridSize.x / 2f) + (cardSize.x / 2f);
            float startY = (calculatedGridSize.y / 2f) - (cardSize.y / 2f);
            
            float x = startX + (col * (cardSize.x + spacing.x));
            float y = startY - (row * (cardSize.y + spacing.y));
            
            cardRect.anchoredPosition = new Vector2(x, y);
        }
        
        /// <summary>
        /// Auto-scales cards to fit within a target area
        /// </summary>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        public void ScaleToFit(float targetWidth, float targetHeight)
        {
            CalculateGridDimensions();
            
            float scaleX = targetWidth / calculatedGridSize.x;
            float scaleY = targetHeight / calculatedGridSize.y;
            
            // Use the smaller scale to ensure grid fits in both dimensions
            float scale = Mathf.Min(scaleX, scaleY);
            scale = Mathf.Min(scale, 1f); // Don't scale up beyond original size
            
            // Apply scale to container
            if (gridContainer != null)
            {
                gridContainer.localScale = Vector3.one * scale;
            }
        }
        
        #endregion
        
        #region Grid Management
        
        /// <summary>
        /// Clears all spawned cards from the grid
        /// </summary>
        public void ClearGrid()
        {
            foreach (Card card in spawnedCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            
            spawnedCards.Clear();
        }
        
        /// <summary>
        /// Gets all currently spawned cards
        /// </summary>
        public List<Card> GetSpawnedCards() => spawnedCards;
        
        #endregion
        
        #region Configuration Setters
        
        public void SetGridSize(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;
        }
        
        public void SetCardSize(Vector2 size)
        {
            this.cardSize = size;
        }
        
        public void SetSpacing(Vector2 spacing)
        {
            this.spacing = spacing;
        }
        
        public void SetCardData(List<CardData> cardDataList)
        {
            this.availableCardData = cardDataList;
        }
        
        #endregion
        
        #region Getters
        
        public int GetRows() => rows;
        
        public int GetColumns() => columns;
        
        public int GetTotalCards() => rows * columns;
        
        public Vector2 GetGridSize() => calculatedGridSize;
        
        #endregion
    }
}