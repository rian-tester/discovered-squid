using UnityEngine;
using FirstRound;
using System.Collections.Generic;

namespace FirstRound
{
    /// <summary>
    /// Central game controller - Singleton pattern
    /// Coordinates all managers and handles game flow
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // Singleton instance
        public static GameManager Instance { get; private set; }
        
        [Header("Managers")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private CardManager cardManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private SaveLoadManager saveLoadManager;
        
        [Header("Game Settings")]
        [SerializeField] private int defaultRows = 4;
        [SerializeField] private int defaultColumns = 4;
        [SerializeField] private bool autoStartOnAwake = true;
        
        // Game state
        private GameState currentState = GameState.Idle;
        
        #region Singleton Setup
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("GameManager initialized as singleton");
        }
        
        #endregion
        
        #region Initialization
        
        private void Start()
        {
            if (!ValidateReferences())
            {
                Debug.LogError("GameManager: Missing manager references!");
                return;
            }
            
            InitializeManagers();
            
            if (autoStartOnAwake)
            {
                StartNewGame();
            }
            
            Debug.Log("=== GameManager Ready ===");
        }
        
        /// <summary>
        /// Validates all manager references
        /// </summary>
        private bool ValidateReferences()
        {
            bool valid = true;
            
            if (gridManager == null)
            {
                Debug.LogError("GridManager is not assigned!");
                valid = false;
            }
            
            if (cardManager == null)
            {
                Debug.LogError("CardManager is not assigned!");
                valid = false;
            }
            
            if (scoreManager == null)
            {
                Debug.LogError("ScoreManager is not assigned!");
                valid = false;
            }
            
            if (uiManager == null)
            {
                Debug.LogError("UIManager is not assigned!");
                valid = false;
            }
            
            if (audioManager == null)
            {
                Debug.LogWarning("AudioManager is not assigned (optional)");
            }
            
            if (saveLoadManager == null)
            {
                Debug.LogWarning("SaveLoadManager is not assigned (optional)");
            }
            
            return valid;
        }
        
        /// <summary>
        /// Initializes all managers in correct order
        /// </summary>
        private void InitializeManagers()
        {
            // Initialize ScoreManager with CardManager
            scoreManager.Initialize(cardManager);
            
            // Initialize SaveLoadManager
            if (saveLoadManager != null)
            {
                saveLoadManager.Initialize();
            }
            
            // Initialize UIManager with all managers
            uiManager.Initialize(scoreManager, cardManager, saveLoadManager);
            
            // Initialize AudioManager
            if (audioManager != null)
            {
                audioManager.Initialize();
                cardManager.SetAudioManager(audioManager);
            }
            
            // Subscribe to game complete event
            if (cardManager != null)
            {
                cardManager.OnAllCardsMatched += OnGameComplete;
            }
            
            Debug.Log("All managers initialized successfully");
        }
        
        #endregion
        
        #region Game Flow
        
        /// <summary>
        /// Starts a new game with default grid size
        /// </summary>
        public void StartNewGame()
        {
            StartNewGame(defaultRows, defaultColumns);
        }
        
        /// <summary>
        /// Starts a new game with custom grid size
        /// </summary>
        public void StartNewGame(int rows, int columns)
        {
            Debug.Log($"Starting new game: {rows}x{columns}");
            
            // Set game state
            currentState = GameState.Playing;
            
            // Clear previous game
            cardManager.ClearAllCards();
            scoreManager.ResetScore();
            uiManager.ResetUI();
            
            // Set grid size
            gridManager.SetGridSize(rows, columns);
            
            // Generate grid
            List<Card> cards = gridManager.GenerateGrid();
            
            if (cards == null || cards.Count == 0)
            {
                Debug.LogError("Failed to generate grid!");
                currentState = GameState.Idle;
                return;
            }
            
            // Register cards with CardManager
            cardManager.RegisterCards(cards);
            
            // Start UI timer
            uiManager.StartTimer();
            
            // Play game start sound
            if (audioManager != null)
            {
                audioManager.PlayCardFlip();
            }
            
            Debug.Log("Game started successfully!");
        }
        
        /// <summary>
        /// Restarts the current game
        /// </summary>
        public void RestartGame()
        {
            StartNewGame(gridManager.GetRows(), gridManager.GetColumns());
        }
        
        /// <summary>
        /// Pauses the game
        /// </summary>
        public void PauseGame()
        {
            if (currentState != GameState.Playing)
                return;
            
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            uiManager.StopTimer();
            
            Debug.Log("Game paused");
        }
        
        /// <summary>
        /// Resumes the game
        /// </summary>
        public void ResumeGame()
        {
            if (currentState != GameState.Paused)
                return;
            
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            uiManager.StartTimer();
            
            Debug.Log("Game resumed");
        }
        
        /// <summary>
        /// Ends the current game
        /// </summary>
        public void EndGame()
        {
            currentState = GameState.GameOver;
            uiManager.StopTimer();
            
            Debug.Log("=== GAME OVER ===");
            
            // Save game results
            if (saveLoadManager != null)
            {
                int rows = gridManager.GetRows();
                int columns = gridManager.GetColumns();
                int score = scoreManager.GetCurrentScore();
                int combo = scoreManager.GetHighestCombo();
                float efficiency = scoreManager.GetEfficiency();
                int turns = scoreManager.GetTotalTurns();
                int matches = scoreManager.GetTotalMatches();
                float gameTime = uiManager.GetGameTime();
                
                saveLoadManager.SaveGameResults(rows, columns, score, combo, 
                                                efficiency, turns, matches, gameTime);
            }
            
            // Play game over sound
            if (audioManager != null)
            {
                audioManager.PlayGameOver();
            }
            
            Debug.Log($"Final Score: {scoreManager.GetCurrentScore()}");
            Debug.Log($"Efficiency: {scoreManager.GetEfficiency()}%");
        }
        
        /// <summary>
        /// Quits to main menu
        /// </summary>
        public void QuitToMenu()
        {
            currentState = GameState.Menu;
            
            // TODO: Load menu scene
            // SceneManager.LoadScene("MainMenu");
            
            Debug.Log("Quit to menu");
        }
        
        #endregion
        
        #region Events Callback
        
        /// <summary>
        /// Called when all cards are matched
        /// </summary>
        private void OnGameComplete()
        {
            EndGame();
        }
        
        #endregion
        
        #region Input Handling
        
        private void Update()
        {
            HandleDebugInput();
        }
        
        /// <summary>
        /// Handles debug keyboard shortcuts
        /// </summary>
        private void HandleDebugInput()
        {
            // New game
            if (Input.GetKeyDown(KeyCode.G))
            {
                StartNewGame();
            }
            
            // Restart game
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }
            
            // Pause/Resume
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (currentState == GameState.Playing)
                    PauseGame();
                else if (currentState == GameState.Paused)
                    ResumeGame();
            }
            
            // Show info
            if (Input.GetKeyDown(KeyCode.I))
            {
                ShowGameInfo();
            }
        }
        
        #endregion
        
        #region Game Info
        
        /// <summary>
        /// Shows current game statistics
        /// </summary>
        private void ShowGameInfo()
        {
            Debug.Log("=== GAME INFO ===");
            Debug.Log($"State: {currentState}");
            Debug.Log($"Grid: {gridManager.GetRows()}x{gridManager.GetColumns()}");
            Debug.Log($"Score: {scoreManager.GetCurrentScore()}");
            Debug.Log($"Matches: {scoreManager.GetTotalMatches()}");
            Debug.Log($"Turns: {scoreManager.GetTotalTurns()}");
            Debug.Log($"Combo: x{scoreManager.GetCurrentCombo()}");
            Debug.Log($"Efficiency: {scoreManager.GetEfficiency()}%");
        }
        
        #endregion
        
        #region Getters
        
        public GameState GetCurrentState() => currentState;
        
        public GridManager GetGridManager() => gridManager;
        
        public CardManager GetCardManager() => cardManager;
        
        public ScoreManager GetScoreManager() => scoreManager;
        
        public UIManager GetUIManager() => uiManager;
        
        public AudioManager GetAudioManager() => audioManager;
        
        public SaveLoadManager GetSaveLoadManager() => saveLoadManager;
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            if (uiManager != null)
            {
                uiManager.Cleanup();
            }
            
            if (cardManager != null)
            {
                cardManager.OnAllCardsMatched -= OnGameComplete;
            }
            
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Game state enum
    /// </summary>
    public enum GameState
    {
        Idle,       // No game running
        Menu,       // In menu
        Playing,    // Game in progress
        Paused,     // Game paused
        GameOver    // Game finished
    }
}