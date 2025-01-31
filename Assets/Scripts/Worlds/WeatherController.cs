using UnityEngine;

using UZSG.Data;
using UZSG.Systems;

namespace UZSG.Worlds
{
    public class WeatherController : MonoBehaviour
    {
        public World World { get; private set; }

        WeatherData defaultWeatherPreset;
        WeatherData currentWeatherPreset;
        
        float _timer;

        /// <summary>
        /// Runtime particle system for the current weather.
        /// </summary>
        [SerializeField] ParticleSystem weatherParticleSystem;

        void Awake()
        {
            World = GetComponentInParent<World>();
        }

        internal void Initialize()
        {
            defaultWeatherPreset = Resources.Load<WeatherData>("Data/Weather/clear");
            SetWeather(defaultWeatherPreset, int.MaxValue);

            Game.Tick.OnSecond += OnSecond;
        }


        #region Event callbacks

        void OnSecond(SecondInfo info)
        {
            
        }

        internal void Tick(float deltaTime)
        {
            _timer -= deltaTime;

            if (_timer < 0f)
            {
                EndWeather();
            }
        }

        #endregion


        #region Public

        /// <summary>
        /// Set the weather given its id and duration.
        /// </summary>
        public void SetWeather(string id, int durationSeconds)
        {
            var weatherData = Resources.Load<WeatherData>($"Data/Weather/{id}");
            if (weatherData == null)
            {
                Game.Console.LogInfo($"No such weather '{id}' exists!");
                return;
            }

            SetWeather(weatherData, durationSeconds); /// indefinite
        }

        /// <summary>
        /// Ends the current weather.
        /// </summary>
        public void EndWeather()
        {
            SetWeather(defaultWeatherPreset, int.MaxValue);
        }
        
        #endregion


        void SetWeather(WeatherData preset, int durationSeconds)
        {
            if (preset == null) return;

            _timer = durationSeconds;

            currentWeatherPreset = preset;
            World.Time.WeatherPreset = preset;
            InitializeParticleSystem(preset);
            if (preset.Skybox != null)
            {
                RenderSettings.skybox = preset.Skybox;
            }
        }

        void InitializeParticleSystem(WeatherData preset)
        {
            if (preset.WeatherParticlesPrefab == null) /// no weather particles
            {
                Destroy(weatherParticleSystem.gameObject);
            }; 

            var go = Instantiate(preset.WeatherParticlesPrefab);
            go.name = $"Weather Particle System ({preset.DisplayName})";
            weatherParticleSystem = go.GetComponent<ParticleSystem>();
            
            var localPlayer = World.GetLocalPlayer();
            if (localPlayer == null)
            {
                Destroy(weatherParticleSystem.gameObject);
                return;
            }

            weatherParticleSystem.transform.SetParent(localPlayer.transform);
        
            var offset = new Vector3(0, 16, 0);
            weatherParticleSystem.transform.position = Camera.main.transform.position + offset;
            weatherParticleSystem.Play();
        }
    }
}

