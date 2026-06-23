#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Permafrost.World
{
    /// <summary>
    /// Used for debug menu options to advance the in-game clock.
    /// </summary>
    public class DebugClock : MonoBehaviour
    {
        /// <summary>
        /// Advances the clock one hour of time relative to the clock's day length.
        /// </summary>
        [MenuItem("Clock/Advance One Hour")]
        public static void AdvanceClock1Hour()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Must be used in play mode!");
                return;
            }

            DayNightCycle clock = GameObject.Find("Directional Light").GetComponent<DayNightCycle>();
            clock.AddDayTime(clock.DayLength / 24f);
        }

        /// <summary>
        /// Advances the clock six hours of time relative to the clock's day length.
        /// </summary>
        [MenuItem("Clock/Advance Six Hours")]
        public static void AdvanceClock6Hours()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Must be used in play mode!");
                return;
            }

            DayNightCycle clock = GameObject.Find("Directional Light").GetComponent<DayNightCycle>();
            clock.AddDayTime(clock.DayLength / 4f);
        }

        /// <summary>
        /// Advances the clock twelve hours of time relative to the clock's day length.
        /// </summary>
        [MenuItem("Clock/Advance Twelve Hours")]
        public static void AdvanceClock12Hours()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Must be used in play mode!");
                return;
            }

            DayNightCycle clock = GameObject.Find("Directional Light").GetComponent<DayNightCycle>();
            clock.AddDayTime(clock.DayLength / 2f);
        }

        /// <summary>
        /// Advances the clock 24 hours of time relative to the clock's day length, or just one day advanced.
        /// </summary>
        [MenuItem("Clock/Advance One Day")]
        public static void AdvanceClock1Day()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Must be used in play mode!");
                return;
            }

            DayNightCycle clock = GameObject.Find("Directional Light").GetComponent<DayNightCycle>();
            clock.AddDayTime(clock.DayLength);
        }
    }
}
#endif
// SLOC not counted