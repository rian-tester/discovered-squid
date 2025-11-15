using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace FirstRound
{
    /// <summary>
    /// Manages the main menu UI, grid selection, and card flip animations
    /// Stats update dynamically based on selected grid from dropdowns
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Grid Selection Dropdowns")]
        [SerializeField] private TMP_Dropdown rowsDropdown;
        [SerializeField] private TMP_Dropdown columnsDropdown;

        [Header("UI References - All Time Best Panel")]
        [SerializeField] private TextMeshProUGUI playerIDText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI bestComboText;
        [SerializeField] private TextMeshProUGUI bestEfficiencyText;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button deleteSaveButton;
        [SerializeField] private Button exitButton;

        [Header("Animated Cards")]
        [SerializeField] private Card card1;
        [SerializeField] private Card card2;
        [SerializeField] private Card card3;
        [SerializeField] private Card card4;

        [Header("Card Animation Settings")]
        [SerializeField] private CardData[] demoCardData;
        [SerializeField] private Sprite cardBackSprite;
        [SerializeField] private float flipInterval = 1.5f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip flipSound;

        [Header("Scene Management")]
        [SerializeField] private int gameSceneIndex = 1;

        // Components
        private SaveLoadManager saveLoadManager;
        
        // Selected grid size
        private int selectedRows = 4;
        private int selectedColumns = 4;

        // State
        private bool isAnimating = false;

        #region Initialization

        private void Awake()
        {
            // Get or create SaveLoadManager
            saveLoadManager = FindObjectOfType<SaveLoadManager>();
            if (saveLoadManager == null)
            {
                Debug.Log("[MainMenuManager] Creating new SaveLoadManager");
                GameObject saveManagerObj = new GameObject("SaveLoadManager");
                saveLoadManager = saveManagerObj.AddComponent<SaveLoadManager>();
                DontDestroyOnLoad(saveManagerObj);
            }
            else
            {
                Debug.Log("[MainMenuManager] Found existing SaveLoadManager");
            }
        }

        private void Start()
        {
            // Initialize save system (will reload if returning from game)
            saveLoadManager.Initialize();

            // Setup dropdowns
            SetupDropdowns();

            // Setup UI
            UpdateAllTimeBestPanel();
            SetupButtons();

            // Initialize animated cards
            InitializeAnimatedCards();

            // Start continuous flip animation
            StartCoroutine(ContinuousCardFlipRoutine());
        }

        #endregion

        #region Dropdown Setup

        /// <summary>
        /// Setup dropdown options and listeners
        /// </summary>
        private void SetupDropdowns()
        {
            // Setup Rows Dropdown
            rowsDropdown.ClearOptions();
            List<string> rowOptions = new List<string> { "2", "4" };
            rowsDropdown.AddOptions(rowOptions);
            rowsDropdown.value = 1; // Default: 4 rows
            rowsDropdown.RefreshShownValue();

            // Setup Columns Dropdown
            columnsDropdown.ClearOptions();
            List<string> columnOptions = new List<string> { "2", "4", "6", "8", "10" };
            columnsDropdown.AddOptions(columnOptions);
            columnsDropdown.value = 1; // Default: 4 columns
            columnsDropdown.RefreshShownValue();

            // Add listeners - THIS IS CRITICAL FOR DYNAMIC STATS UPDATE
            rowsDropdown.onValueChanged.AddListener(OnGridSelectionChanged);
            columnsDropdown.onValueChanged.AddListener(OnGridSelectionChanged);

            Debug.Log("[MainMenuManager] Dropdown listeners attached");

            // Set initial values
            UpdateSelectedGridSize();
        }

        /// <summary>
        /// Called when dropdown selection changes
        /// Updates stats panel to show data for newly selected grid
        /// </summary>
        private void OnGridSelectionChanged(int index)
        {
            Debug.Log($"[MainMenuManager] Dropdown changed! Index: {index}");
            
            UpdateSelectedGridSize();
            
            Debug.Log($"[MainMenuManager] New grid: {selectedRows}x{selectedColumns}");
            
            // Update stats panel to show stats for selected grid
            UpdateAllTimeBestPanel();
            
            Debug.Log($"[MainMenuManager] Stats panel updated");
        }

        /// <summary>
        /// Update selected grid size from dropdowns
        /// </summary>
        private void UpdateSelectedGridSize()
        {
            selectedRows = int.Parse(rowsDropdown.options[rowsDropdown.value].text);
            selectedColumns = int.Parse(columnsDropdown.options[columnsDropdown.value].text);
            
            int totalCards = selectedRows * selectedColumns;
            Debug.Log($"[MainMenuManager] Grid selected: {selectedRows}x{selectedColumns} = {totalCards} cards");
        }

        #endregion

        #region UI Updates

        /// <summary>
        /// Updates the All Time Best panel with saved data for SELECTED grid
        /// This is called both on start and when dropdown selection changes
        /// </summary>
        private void UpdateAllTimeBestPanel()
        {
            Debug.Log($"[MainMenuManager] UpdateAllTimeBestPanel() for {selectedRows}x{selectedColumns}");
            
            // Get game data for currently selected grid size
            GameData currentGridData = saveLoadManager.GetGameData(selectedRows, selectedColumns);

            Debug.Log($"[MainMenuManager] GameData - HS: {currentGridData.highScore}, Combo: {currentGridData.bestCombo}, Eff: {currentGridData.highestEfficiency}");

            // Update UI
            playerIDText.text = $"Player: {SystemInfo.deviceName}";
            
            // Show stats for current grid, or "--" if no data
            highScoreText.text = currentGridData.highScore > 0 
                ? $"High Score: {currentGridData.highScore}" 
                : "High Score: --";
            
            bestComboText.text = currentGridData.bestCombo > 0 
                ? $"Best Combo: {currentGridData.bestCombo}" 
                : "Best Combo: --";
            
            bestEfficiencyText.text = currentGridData.highestEfficiency > 0f 
                ? $"Best Efficiency: {currentGridData.highestEfficiency:F1}%" 
                : "Best Efficiency: --";

            Debug.Log($"[MainMenuManager] Stats updated - {highScoreText.text}");
        }

        #endregion

        #region Button Handlers

        /// <summary>
        /// Setup button click listeners
        /// </summary>
        private void SetupButtons()
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
            deleteSaveButton.onClick.AddListener(OnDeleteSaveButtonClicked);
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        /// <summary>
        /// Handle Play button click - Pass grid size to game scene
        /// </summary>
        private void OnPlayButtonClicked()
        {
            Debug.Log($"[MainMenuManager] Play button clicked - Starting game with {selectedRows}x{selectedColumns} grid");
            
            // Store selected grid size in PlayerPrefs to pass to game scene
            PlayerPrefs.SetInt("SelectedRows", selectedRows);
            PlayerPrefs.SetInt("SelectedColumns", selectedColumns);
            PlayerPrefs.Save();
            
            // Load game scene
            SceneManager.LoadScene(gameSceneIndex);
        }

        /// <summary>
        /// Handle Delete Save button click - Delete data for selected grid only
        /// </summary>
        private void OnDeleteSaveButtonClicked()
        {
            Debug.Log($"[MainMenuManager] Delete Save button clicked for {selectedRows}x{selectedColumns} grid");
            
            // Delete save data for currently selected grid
            saveLoadManager.DeleteGridData(selectedRows, selectedColumns);
            
            // Refresh UI to show cleared stats
            UpdateAllTimeBestPanel();
            
            Debug.Log($"[MainMenuManager] Save data deleted for {selectedRows}x{selectedColumns} grid!");
        }

        /// <summary>
        /// Exit the game
        /// </summary>
        private void OnExitButtonClicked()
        {
            Application.Quit();
        }

        #endregion

        #region Card Animation

        /// <summary>
        /// Initialize the 4 animated cards
        /// </summary>
        private void InitializeAnimatedCards()
        {
            // Ensure we have enough demo card data
            if (demoCardData == null || demoCardData.Length < 2)
            {
                Debug.LogError("[MainMenuManager] Need at least 2 CardData objects for demo cards!");
                return;
            }

            // Initialize cards with demo data
            card1.Initialize(demoCardData[0], cardBackSprite, 0);
            card2.Initialize(demoCardData[1 % demoCardData.Length], cardBackSprite, 1);
            card3.Initialize(demoCardData[0], cardBackSprite, 2);
            card4.Initialize(demoCardData[1 % demoCardData.Length], cardBackSprite, 3);

            // Set initial states
            card1.SetState(CardState.FaceDown);
            card2.SetState(CardState.FaceUp);
            card3.SetState(CardState.FaceUp);
            card4.SetState(CardState.FaceDown);

            // Setup initial flip
            StartCoroutine(InitialFlipSetup());
        }

        /// <summary>
        /// Setup initial card states without animation
        /// </summary>
        private IEnumerator InitialFlipSetup()
        {
            yield return null;
            
            card2.FlipToFront();
            card3.FlipToFront();
        }

        /// <summary>
        /// Continuous card flip animation routine
        /// Cross pattern: Cards 1 & 4 flip together, Cards 2 & 3 flip together
        /// </summary>
        private IEnumerator ContinuousCardFlipRoutine()
        {
            yield return new WaitForSeconds(0.5f);

            while (true)
            {
                isAnimating = true;

                // Flip cards 1 & 4 to front, cards 2 & 3 to back
                PlayFlipSound();
                card1.FlipToFront();
                card4.FlipToFront();
                
                yield return new WaitForSeconds(0.15f);
                
                PlayFlipSound();
                card2.FlipToBack();
                card3.FlipToBack();

                yield return new WaitForSeconds(flipInterval);

                // Flip cards 1 & 4 to back, cards 2 & 3 to front
                PlayFlipSound();
                card1.FlipToBack();
                card4.FlipToBack();
                
                yield return new WaitForSeconds(0.15f);
                
                PlayFlipSound();
                card2.FlipToFront();
                card3.FlipToFront();

                yield return new WaitForSeconds(flipInterval);

                isAnimating = false;
            }
        }

        /// <summary>
        /// Play flip sound effect
        /// </summary>
        private void PlayFlipSound()
        {
            if (audioSource != null && flipSound != null)
            {
                audioSource.PlayOneShot(flipSound);
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            // Remove dropdown listeners
            if (rowsDropdown != null)
                rowsDropdown.onValueChanged.RemoveListener(OnGridSelectionChanged);
            
            if (columnsDropdown != null)
                columnsDropdown.onValueChanged.RemoveListener(OnGridSelectionChanged);

            // Remove button listeners
            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayButtonClicked);
            
            if (deleteSaveButton != null)
                deleteSaveButton.onClick.RemoveListener(OnDeleteSaveButtonClicked);
            if (exitButton != null)
                exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        #endregion
    }
}