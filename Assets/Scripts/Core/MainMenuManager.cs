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
        [SerializeField] private string gameSceneName = "Game"; 

        // Components
        private SaveLoadManager saveLoadManager;
        
        // Selected grid size (will be passed to game scene)
        private int selectedRows = 2;
        private int selectedColumns = 2;

        // State
        private bool isAnimating = false;

        #region Initialization

        private void Awake()
        {
            // Get or create SaveLoadManager
            saveLoadManager = FindObjectOfType<SaveLoadManager>();
            if (saveLoadManager == null)
            {
                GameObject saveManagerObj = new GameObject("SaveLoadManager");
                saveLoadManager = saveManagerObj.AddComponent<SaveLoadManager>();
                DontDestroyOnLoad(saveManagerObj);
            }
        }

        private void Start()
        {
            // Initialize save system
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
            rowsDropdown.value = 0; 
            rowsDropdown.RefreshShownValue();

            // Setup Columns Dropdown
            columnsDropdown.ClearOptions();
            List<string> columnOptions = new List<string> { "2", "4", "6", "8", "10" };
            columnsDropdown.AddOptions(columnOptions);
            columnsDropdown.value = 0; 
            columnsDropdown.RefreshShownValue();

            // Add listeners
            rowsDropdown.onValueChanged.AddListener(OnGridSelectionChanged);
            columnsDropdown.onValueChanged.AddListener(OnGridSelectionChanged);

            // Set initial values
            UpdateSelectedGridSize();
        }

        /// <summary>
        /// Called when dropdown selection changes
        /// </summary>
        private void OnGridSelectionChanged(int index)
        {
            UpdateSelectedGridSize();
            
            // Update stats panel to show stats for selected grid
            UpdateAllTimeBestPanel();
        }

        /// <summary>
        /// Update selected grid size from dropdowns
        /// </summary>
        private void UpdateSelectedGridSize()
        {
            selectedRows = int.Parse(rowsDropdown.options[rowsDropdown.value].text);
            selectedColumns = int.Parse(columnsDropdown.options[columnsDropdown.value].text);
            
            int totalCards = selectedRows * selectedColumns;
            Debug.Log($"Grid selected: {selectedRows}x{selectedColumns} = {totalCards} cards");
        }

        #endregion

        #region UI Updates

        /// <summary>
        /// Updates the All Time Best panel with saved data for selected grid
        /// </summary>
        private void UpdateAllTimeBestPanel()
        {
            // Get game data for currently selected grid size
            GameData currentGridData = saveLoadManager.GetGameData(selectedRows, selectedColumns);

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

            // Optional: Show which grid we're displaying stats for
            Debug.Log($"Displaying stats for {selectedRows}x{selectedColumns} grid");
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
        }

        /// <summary>
        /// Handle Play button click 
        /// </summary>
        private void OnPlayButtonClicked()
        {
            Debug.Log($"Play button clicked - Starting game with {selectedRows}x{selectedColumns} grid");
            
            // Store selected grid size in PlayerPrefs to pass to game scene
            PlayerPrefs.SetInt("SelectedRows", selectedRows);
            PlayerPrefs.SetInt("SelectedColumns", selectedColumns);
            PlayerPrefs.Save();
            
            // Load game scene
            SceneManager.LoadScene(1);
        }

        /// <summary>
        /// Handle Delete Save button click - Delete data for selected grid only
        /// </summary>
        private void OnDeleteSaveButtonClicked()
        {
            Debug.Log($"Delete Save button clicked for {selectedRows}x{selectedColumns} grid");
            
            // Delete save data for currently selected grid
            saveLoadManager.DeleteGridData(selectedRows, selectedColumns);
            
            // Refresh UI to show cleared stats
            UpdateAllTimeBestPanel();
            
            Debug.Log($"Save data deleted for {selectedRows}x{selectedColumns} grid!");
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
                Debug.LogError("Need at least 2 CardData objects for demo cards!");
                return;
            }

            // Initialize cards with demo data
            // Cards 1 & 4 will start face down (showing back)
            // Cards 2 & 3 will start face up (showing front)
            
            card1.Initialize(demoCardData[0], cardBackSprite, 0);
            card2.Initialize(demoCardData[1 % demoCardData.Length], cardBackSprite, 1);
            card3.Initialize(demoCardData[0], cardBackSprite, 2);
            card4.Initialize(demoCardData[1 % demoCardData.Length], cardBackSprite, 3);

            // Set initial states - cards 2 & 3 start flipped to front
            card1.SetState(CardState.FaceDown);
            card2.SetState(CardState.FaceUp);
            card3.SetState(CardState.FaceUp);
            card4.SetState(CardState.FaceDown);

            // Manually set sprites to match initial state
            StartCoroutine(InitialFlipSetup());
        }

        /// <summary>
        /// Setup initial card states without animation
        /// </summary>
        private IEnumerator InitialFlipSetup()
        {
            yield return null; // Wait one frame
            
            // Flip cards 2 & 3 to front immediately
            card2.FlipToFront();
            card3.FlipToFront();
        }

        /// <summary>
        /// Continuous card flip animation routine
        /// Cross pattern: Cards 1 & 4 flip together, Cards 2 & 3 flip together
        /// </summary>
        private IEnumerator ContinuousCardFlipRoutine()
        {
            // Wait a bit before starting animation
            yield return new WaitForSeconds(0.5f);

            while (true)
            {
                isAnimating = true;

                // === FLIP CYCLE 1: Flip cards 1 & 4 to front, cards 2 & 3 to back ===
                
                // Play sound for first pair
                PlayFlipSound();
                
                card1.FlipToFront();
                card4.FlipToFront();
                
                // Small delay between pairs for visual effect
                yield return new WaitForSeconds(0.15f);
                
                // Play sound for second pair
                PlayFlipSound();
                
                card2.FlipToBack();
                card3.FlipToBack();

                // Wait for interval
                yield return new WaitForSeconds(flipInterval);

                // === FLIP CYCLE 2: Flip cards 1 & 4 to back, cards 2 & 3 to front ===
                
                // Play sound for first pair
                PlayFlipSound();
                
                card1.FlipToBack();
                card4.FlipToBack();
                
                // Small delay between pairs
                yield return new WaitForSeconds(0.15f);
                
                // Play sound for second pair
                PlayFlipSound();
                
                card2.FlipToFront();
                card3.FlipToFront();

                // Wait for interval before repeating
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
        }

        #endregion
    }
}