using UnityEngine;

namespace Permafrost
{
    public class GameMaster : MonoBehaviour
    {
        [Header("Global Settings")]
        [SerializeField] private string savePath;

        public string SaveDataFolder { get => savePath; }

        public bool GamePaused { get; private set; }

        public int DayNumber { get; private set; }
        public void NewDay() { DayNumber++; }

    }
}
