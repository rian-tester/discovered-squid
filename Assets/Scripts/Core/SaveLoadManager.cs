using UnityEngine;
using System.Collections.Generic;

namespace FirstRound
{
    /// <summary>
    /// Manages saving and loading game data using PlayerPrefs
    /// Supports multiple grid sizes with separate save data
    /// </summary>
    public class SaveLoadManager : MonoBehaviour
    {
        private const string SAVE_KEY_PREFIX = "CardGame_";
        private const string ALL_GRIDS_KEY = "CardGame_AllGrids";
        
        // Cache for loaded data
        private Dictionary<string, GameData> loadedGameData = new Dictionary<string, GameData>();
        
        #region Initialization
        
        /// <summary>
        /// Initializes save/load manager
        /// </summary>
        public void Initialize()
        {
            LoadAllGridData();
            Debug.Log("SaveLoadManager initialized");
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
                Debug.LogError("Cannot save null GameData!");
                return;
            }
            
            string key = SAVE_KEY_PREFIX + gameData.gridKey;
            string json = JsonUtility.ToJson(gameData);
            
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
            
            // Update cache
            loadedGameData[gameData.gridKey] = gameData;
            
            // Track this grid in list of all grids
            AddToGridList(gameData.gridKey);
            
            Debug.Log($"Saved game data for {gameData.gridKey}: High Score = {gameData.highScore}");
        }
        
        /// <summary>
        /// Saves game results and updates records
        /// </summary>
        public void SaveGameResults(int rows, int columns, int score, int combo, 
                                   float efficiency, int turns, int matches, float gameTime)
        {
            // Get or create game data for this grid
            GameData gameData = GetGameData(rows, columns);
            
            // Update with new results
            gameData.UpdateWithGameResults(score, combo, efficiency, turns, matches, gameTime);
            
            // Save updated data
            SaveGameData(gameData);
            
            // Log new records
            if (gameData.IsNewHighScore(score))
            {
                Debug.Log($"NEW HIGH SCORE for {gameData.gridKey}: {score}!");
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
                return loadedGameData[gridKey];
            }
            
            // Try to load from PlayerPrefs
            string key = SAVE_KEY_PREFIX + gridKey;
            
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                GameData gameData = JsonUtility.FromJson<GameData>(json);
                loadedGameData[gridKey] = gameData;
                return gameData;
            }
            
            // Create new data if not found
            GameData newData = new GameData(rows, columns);
            loadedGameData[gridKey] = newData;
            return newData;
        }
        
        /// <summary>
        /// Loads all saved grid data
        /// </summary>
        private void LoadAllGridData()
        {
            loadedGameData.Clear();
            
            // Get list of all saved grids
            List<string> gridKeys = GetAllGridKeys();
            
            foreach (string gridKey in gridKeys)
            {
                string key = SAVE_KEY_PREFIX + gridKey;
                if (PlayerPrefs.HasKey(key))
                {
                    string json = PlayerPrefs.GetString(key);
                    GameData gameData = JsonUtility.FromJson<GameData>(json);
                    loadedGameData[gridKey] = gameData;
                }
            }
            
            Debug.Log($"Loaded {loadedGameData.Count} grid save files");
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
            }
        }
        
        /// <summary>
        /// Gets list of all grid keys that have been played
        /// </summary>
        private List<string> GetAllGridKeys()
        {
            if (!PlayerPrefs.HasKey(ALL_GRIDS_KEY))
            {
                return new List<string>();
            }
            
            string json = PlayerPrefs.GetString(ALL_GRIDS_KEY);
            GridKeyList gridKeyList = JsonUtility.FromJson<GridKeyList>(json);
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
            }
            
            // Remove from cache
            if (loadedGameData.ContainsKey(gridKey))
            {
                loadedGameData.Remove(gridKey);
            }
            
            Debug.Log($"Deleted save data for {gridKey}");
        }
        
        /// <summary>
        /// Deletes ALL save data (for reset button in main menu)
        /// </summary>
        public void DeleteAllData()
        {
            // Get all grid keys
            List<string> gridKeys = GetAllGridKeys();
            
            // Delete each grid's data
            foreach (string gridKey in gridKeys)
            {
                string key = SAVE_KEY_PREFIX + gridKey;
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                }
            }
            
            // Delete grid list
            if (PlayerPrefs.HasKey(ALL_GRIDS_KEY))
            {
                PlayerPrefs.DeleteKey(ALL_GRIDS_KEY);
            }
            
            PlayerPrefs.Save();
            
            // Clear cache
            loadedGameData.Clear();
            
            Debug.Log("ALL save data deleted!");
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
            return PlayerPrefs.HasKey(key);
        }
        
        /// <summary>
        /// Gets all saved game data (for main menu display)
        /// </summary>
        public Dictionary<string, GameData> GetAllGameData()
        {
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