using UnityEngine;

namespace Permafrost
{
    /// <summary>
    /// The Game Master; generally used for any global information that everything would need to know.
    /// Examples include the save data path, whether the game is currently paused, what day it is, etc.
    /// </summary>
    public class GameMaster : MonoBehaviour
    {
        [Header("Global Settings")]
        [SerializeField] private string savePath;

        /// <summary>
        /// The path to the game's save data folder.
        /// </summary>
        public string SaveDataFolder { get => savePath; }

        /// <summary>
        /// True if the game is currently paused, false otherwise.
        /// </summary>
        public bool GamePaused { get; private set; }

        /// <summary>
        /// The current in-game day number.
        /// </summary>
        public int DayNumber { get; private set; }
        /// <summary>
        /// Increments the day number.
        /// </summary>
        public void NewDay() { DayNumber++; }

    }
}
// 6 SLOC