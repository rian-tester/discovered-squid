using System;

namespace FirstRound
{
    /// <summary>
    /// Data structure for saving game progress per grid size
    /// Serializable for JSON or PlayerPrefs
    /// </summary>
    [Serializable]
    public class GameData
    {
        // Grid identification
        public int rows;
        public int columns;
        public string gridKey; // "4x4", "2x2", etc.
        
        // Best stats (all-time records)
        public int highScore;
        public int bestCombo;
        public float highestEfficiency;
        
        // Last game stats
        public int lastScore;
        public int lastTurns;
        public int lastMatches;
        public float lastEfficiency;
        public float lastGameTime;
        
        // Metadata
        public int totalGamesPlayed;
        public string lastPlayedDate;
        
        /// <summary>
        /// Constructor with grid size
        /// </summary>
        public GameData(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;
            this.gridKey = $"{rows}x{columns}";
            
            // Initialize with default values
            highScore = 0;
            bestCombo = 0;
            highestEfficiency = 0f;
            
            lastScore = 0;
            lastTurns = 0;
            lastMatches = 0;
            lastEfficiency = 0f;
            lastGameTime = 0f;
            
            totalGamesPlayed = 0;
            lastPlayedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        
        /// <summary>
        /// Updates game data with new game results
        /// </summary>
        public void UpdateWithGameResults(int score, int combo, float efficiency, 
                                          int turns, int matches, float gameTime)
        {
            // Update last game stats
            lastScore = score;
            lastTurns = turns;
            lastMatches = matches;
            lastEfficiency = efficiency;
            lastGameTime = gameTime;
            
            // Update best stats if new records
            if (score > highScore)
            {
                highScore = score;
            }
            
            if (combo > bestCombo)
            {
                bestCombo = combo;
            }
            
            if (efficiency > highestEfficiency)
            {
                highestEfficiency = efficiency;
            }
            
            // Update metadata
            totalGamesPlayed++;
            lastPlayedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        
        /// <summary>
        /// Checks if current game achieved new records
        /// </summary>
        public bool IsNewHighScore(int score) => score > highScore;
        
        public bool IsNewBestCombo(int combo) => combo > bestCombo;
        
        public bool IsNewHighestEfficiency(float efficiency) => efficiency > highestEfficiency;
    }
}