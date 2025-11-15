using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace FirstRound
{
    /// <summary>
    /// Manages saving and loading game data using PlayerPrefs
    /// Supports multiple grid sizes with separate save data
    /// AUTO-RELOADS when Menu scene is loaded to show latest data
    /// </summary>
    public class SaveLoadManager : MonoBehaviour
    {
        private const string SAVE_KEY_PREFIX = "CardGame_";
        private const string ALL_GRIDS_KEY = "CardGame_AllGrids";
        
        // Cache for loaded data
        private Dictionary<string, GameData> loadedGameData = new Dictionary<string, GameData>();
        
        #region Initialization
        
        private void Awake()
        {
            // Subscribe to scene loaded event for auto-reload
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        /// <summary>
        /// Called automatically when ANY scene loads
        /// Reloads data when returning to menu to show latest scores
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[SaveLoadManager] Scene loaded: {scene.name}");
            
            // NOTE: Change "Menu" or "Main" to match YOUR menu scene name exactly!
            if (scene.name == "Menu" || scene.name == "Main")
            {
                Debug.Log("[SaveLoadManager] Menu scene detected - Force reloading save data");
                Initialize();
            }
        }
        
        /// <summary>
        /// Initializes save/load manager
        /// FORCES fresh reload from PlayerPrefs
        /// </summary>
        public void Initialize()
        {
            Debug.Log("[SaveLoadManager] Initialize() called - Clearing cache and reloading");
            
            // CRITICAL: Clear cache to force reload from PlayerPrefs
            loadedGameData.Clear();
            
            LoadAllGridData();
            Debug.Log($"[SaveLoadManager] Initialized with {loadedGameData.Count} grid save files");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from scene loaded event
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        #endregion
        
        #region Save Operations
        
        /// <summary>
        /// Saves game data for specific grid size
        /// </summary>
        public void SaveGameData(GameData gameData)
        {
            if (gameData == null)
            {
                Debug.LogError("[SaveLoadManager] Cannot save null GameData!");
                return;
            }
            
            string key = SAVE_KEY_PREFIX + gameData.gridKey;
            string json = JsonUtility.ToJson(gameData);
            
            Debug.Log($"[SaveLoadManager] Saving to PlayerPrefs - Key: {key}");
            
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
            
            // Update cache
            loadedGameData[gameData.gridKey] = gameData;
            
            // Track this grid in list of all grids
            AddToGridList(gameData.gridKey);
            
            Debug.Log($"[SaveLoadManager] ✓ Saved game data for {gameData.gridKey}: High Score = {gameData.highScore}");
        }
        
        /// <summary>
        /// Saves game results and updates records
        /// </summary>
        public void SaveGameResults(int rows, int columns, int score, int combo, 
                                   float efficiency, int turns, int matches, float gameTime)
        {
            Debug.Log($"[SaveLoadManager] SaveGameResults called for {rows}x{columns}");
            Debug.Log($"[SaveLoadManager] Score: {score}, Combo: {combo}, Efficiency: {efficiency:F1}%");
            
            // Get or create game data for this grid
            GameData gameData = GetGameData(rows, columns);
            
            Debug.Log($"[SaveLoadManager] Current high score: {gameData.highScore}");
            
            // Update with new results
            gameData.UpdateWithGameResults(score, combo, efficiency, turns, matches, gameTime);
            
            Debug.Log($"[SaveLoadManager] Updated high score: {gameData.highScore}");
            
            // Save updated data
            SaveGameData(gameData);
            
            // Log new records
            if (gameData.IsNewHighScore(score))
            {
                Debug.Log($"[SaveLoadManager] 🏆 NEW HIGH SCORE for {gameData.gridKey}: {score}!");
            }
            if (gameData.IsNewBestCombo(combo))
            {
                Debug.Log($"[SaveLoadManager] 🏆 NEW BEST COMBO for {gameData.gridKey}: {combo}!");
            }
            if (gameData.IsNewHighestEfficiency(efficiency))
            {
                Debug.Log($"[SaveLoadManager] 🏆 NEW BEST EFFICIENCY for {gameData.gridKey}: {efficiency:F1}%!");
            }
        }
        
        #endregion
        
        #region Load Operations
        
        /// <summary>
        /// Loads game data for specific grid size
        /// </summary>
        public GameData GetGameData(int rows, int columns)
        {
            string gridKey = $"{rows}x{columns}";
            
            // Check cache first
            if (loadedGameData.ContainsKey(gridKey))
            {
                Debug.Log($"[SaveLoadManager] Using cached data for {gridKey}");
                return loadedGameData[gridKey];
            }
            
            // Try to load from PlayerPrefs
            string key = SAVE_KEY_PREFIX + gridKey;
            
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                Debug.Log($"[SaveLoadManager] Loading from PlayerPrefs - Key: {key}");
                
                GameData gameData = JsonUtility.FromJson<GameData>(json);
                loadedGameData[gridKey] = gameData;
                
                Debug.Log($"[SaveLoadManager] ✓ Loaded existing data for {gridKey} - High Score: {gameData.highScore}");
                return gameData;
            }
            
            // Create new data if not found
            Debug.Log($"[SaveLoadManager] No save data found for {gridKey} - Creating new");
            GameData newData = new GameData(rows, columns);
            loadedGameData[gridKey] = newData;
            return newData;
        }
        
        /// <summary>
        /// Loads all saved grid data
        /// </summary>
        private void LoadAllGridData()
        {
            Debug.Log("[SaveLoadManager] LoadAllGridData() called");
            loadedGameData.Clear();
            
            // Get list of all saved grids
            List<string> gridKeys = GetAllGridKeys();
            Debug.Log($"[SaveLoadManager] Found {gridKeys.Count} grid keys to load");
            
            foreach (string gridKey in gridKeys)
            {
                string key = SAVE_KEY_PREFIX + gridKey;
                if (PlayerPrefs.HasKey(key))
                {
                    string json = PlayerPrefs.GetString(key);
                    GameData gameData = JsonUtility.FromJson<GameData>(json);
                    loadedGameData[gridKey] = gameData;
                    Debug.Log($"[SaveLoadManager] Loaded {gridKey}: High Score = {gameData.highScore}");
                }
            }
            
            Debug.Log($"[SaveLoadManager] ✓ Loaded {loadedGameData.Count} grid save files");
        }
        
        #endregion
        
        #region Grid Tracking
        
        /// <summary>
        /// Adds grid key to list of all grids
        /// </summary>
        private void AddToGridList(string gridKey)
        {
            List<string> gridKeys = GetAllGridKeys();
            
            if (!gridKeys.Contains(gridKey))
            {
                gridKeys.Add(gridKey);
                SaveAllGridKeys(gridKeys);
                Debug.Log($"[SaveLoadManager] Added {gridKey} to grid list");
            }
        }
        
        /// <summary>
        /// Gets list of all grid keys that have been played
        /// </summary>
        private List<string> GetAllGridKeys()
        {
            if (!PlayerPrefs.HasKey(ALL_GRIDS_KEY))
            {
                Debug.Log("[SaveLoadManager] No grid list found in PlayerPrefs");
                return new List<string>();
            }
            
            string json = PlayerPrefs.GetString(ALL_GRIDS_KEY);
            GridKeyList gridKeyList = JsonUtility.FromJson<GridKeyList>(json);
            Debug.Log($"[SaveLoadManager] Grid list contains: {string.Join(", ", gridKeyList.keys)}");
            return gridKeyList.keys;
        }
        
        /// <summary>
        /// Saves list of all grid keys
        /// </summary>
        private void SaveAllGridKeys(List<string> gridKeys)
        {
            GridKeyList gridKeyList = new GridKeyList { keys = gridKeys };
            string json = JsonUtility.ToJson(gridKeyList);
            PlayerPrefs.SetString(ALL_GRIDS_KEY, json);
            PlayerPrefs.Save();
            Debug.Log($"[SaveLoadManager] Saved grid list: {string.Join(", ", gridKeys)}");
        }
        
        #endregion
        
        #region Delete/Reset Operations
        
        /// <summary>
        /// Deletes save data for specific grid size
        /// </summary>
        public void DeleteGridData(int rows, int columns)
        {
            string gridKey = $"{rows}x{columns}";
            string key = SAVE_KEY_PREFIX + gridKey;
            
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
                Debug.Log($"[SaveLoadManager] Deleted PlayerPrefs key: {key}");
            }
            
            // Remove from cache
            if (loadedGameData.ContainsKey(gridKey))
            {
                loadedGameData.Remove(gridKey);
                Debug.Log($"[SaveLoadManager] Removed {gridKey} from cache");
            }
            
            Debug.Log($"[SaveLoadManager] ✓ Deleted save data for {gridKey}");
        }
        
        /// <summary>
        /// Deletes ALL save data (for reset button in main menu)
        /// </summary>
        public void DeleteAllData()
        {
            Debug.Log("[SaveLoadManager] DeleteAllData() called");
            
            // Get all grid keys
            List<string> gridKeys = GetAllGridKeys();
            
            // Delete each grid's data
            foreach (string gridKey in gridKeys)
            {
                string key = SAVE_KEY_PREFIX + gridKey;
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                    Debug.Log($"[SaveLoadManager] Deleted {key}");
                }
            }
            
            // Delete grid list
            if (PlayerPrefs.HasKey(ALL_GRIDS_KEY))
            {
                PlayerPrefs.DeleteKey(ALL_GRIDS_KEY);
                Debug.Log($"[SaveLoadManager] Deleted grid list");
            }
            
            PlayerPrefs.Save();
            
            // Clear cache
            loadedGameData.Clear();
            
            Debug.Log("[SaveLoadManager] ✓ ALL save data deleted!");
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// Checks if save data exists for grid size
        /// </summary>
        public bool HasSaveData(int rows, int columns)
        {
            string gridKey = $"{rows}x{columns}";
            string key = SAVE_KEY_PREFIX + gridKey;
            bool exists = PlayerPrefs.HasKey(key);
            Debug.Log($"[SaveLoadManager] HasSaveData({gridKey}): {exists}");
            return exists;
        }
        
        /// <summary>
        /// Gets all saved game data (for main menu display)
        /// </summary>
        public Dictionary<string, GameData> GetAllGameData()
        {
            Debug.Log($"[SaveLoadManager] GetAllGameData() - Returning {loadedGameData.Count} entries");
            return new Dictionary<string, GameData>(loadedGameData);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Helper class for storing list of grid keys in JSON
    /// </summary>
    [System.Serializable]
    internal class GridKeyList
    {
        public List<string> keys = new List<string>();
    }
}