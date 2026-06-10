using UnityEngine;

namespace Permafrost.World
{
    /// <summary>
    /// Main handler for day/night cycle behaviour.
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float dayLength = 360;
        [Tooltip("The physical time of day in seconds. Setting this effectively sets the time of day at the start of the game.")]
        [SerializeField] private float time = 0;
        [Tooltip("Offsets the start of the day this many seconds later. 120 effectively sets dawn as the start of a new day.")]
        [SerializeField] private float offsetToDayStart = 120;
        [Tooltip("Multiplies the speed of day-night transitions. 1 = fading constantly, higher values mean more and more abrupt transitions.")]
        [Range(1f, 100f)]
        [SerializeField] private float dayNightTransitionFactor = 2;
        [Tooltip("How many days pass between full moon and new moon and vice versa.")]
        [SerializeField] private int weekLength = 4;
        [SerializeField] private float newMoonBrightness = 0f;
        [SerializeField] private float fullMoonBrightness = 0.15f;

        private float dayMax;
        private float dayMin;

        [Header("Rendering")]
        [SerializeField] private Color fogColorDay;

        [Header("Component References")]
        [SerializeField] private GameMaster gameMaster;
        [SerializeField] private Light globalLight;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled;

        /// <summary>
        /// The current light value. This is a float between 0 and 1.
        /// </summary>
        public float LightValue { get; private set; }
        public float TimeOfDay { get => time; }

        #region Unity Methods
        // Honestly barely needed
        private void Awake()
        {
            dayMin = fullMoonBrightness;
            dayMax = 1;
        }

        // yup yup
        void Update()
        {
            if (gameMaster.GamePaused) return;

            CheckStartNewDay();
            UpdateDaytime();
            UpdateRender();
        }
        #endregion

        #region Updates
        /// <summary>
        /// Checks if we need to start a new day and recalculates the min and max brightness for that day.
        /// </summary>
        private void CheckStartNewDay()
        {
            if (time < dayLength) return;

            // reset day
            time %= dayLength;
            gameMaster.NewDay();

            // this so we can have full moon to new moon transitions throughout the week
            int dayInWeek = gameMaster.DayNumber % (weekLength * 2);
            float ratio = (fullMoonBrightness - newMoonBrightness) / weekLength;
            dayMax = 1;
            if (dayInWeek < weekLength) dayMin = fullMoonBrightness - (dayInWeek * ratio);
            else dayMin = newMoonBrightness + ((dayInWeek - weekLength) * ratio);

            if (debugEnabled)
            {
                Debug.Log($"[DayNightCycle] Starting day {gameMaster.DayNumber}, max brightness: {dayMax}, min brightness: {dayMin}, day in week: {dayInWeek}");
            }
        }

        /// <summary>
        /// Just updates the time and light value. literally, that's the only two lines of code here. 
        /// I guess the light value calculation is pretty large but
        /// </summary>
        private void UpdateDaytime()
        {
            time += Time.deltaTime;

            LightValue = Mathf.Clamp(
                Mathf.Sin(Mathf.PI * 2 * (time + offsetToDayStart) / dayLength) * dayNightTransitionFactor + dayMin,
                dayMin,
                dayMax
            );
            
            if (debugEnabled)
            {
                Debug.Log($"[DayNightCycle] light value: {LightValue}");
            }
        }

        /// <summary>
        /// Updates all the rendering related aspects.
        /// </summary>
        private void UpdateRender()
        {
            float rotation = (time + offsetToDayStart) / dayLength * 360;
            globalLight.transform.rotation = Quaternion.Euler(rotation, 0, 0);
            RenderSettings.ambientIntensity = LightValue;
            RenderSettings.fogColor = new Color(fogColorDay.r * LightValue, fogColorDay.g * LightValue, fogColorDay.b * LightValue);
            globalLight.intensity = LightValue + 0.01f;

            if (debugEnabled)
            {
                Debug.Log($"[DayNightCycle] rotation: {rotation}");
            }
        }
        #endregion
    }
}