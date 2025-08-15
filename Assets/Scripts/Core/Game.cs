using System;
using System.Threading.Tasks;
using SL.Systems;
using UnityEngine;

namespace SL.Core
{
    /// <summary>
    /// Game singleton and core state machine.
    /// </summary>
    public sealed class Game : MonoBehaviour
    {
        private static Game _instance;
        public static Game Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[Game]");
                    _instance = go.AddComponent<Game>();
                }
                return _instance;
            }
        }

        public AppState State { get; private set; } = AppState.None;

        // Dependencies (can be replaced in tests)
        public ISceneLoader SceneLoader { get; private set; } = new SceneLoader();
        public IGameServices Services { get; private set; } = new DefaultGameServices();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>Override dependencies (useful for tests/DI).</summary>
        public void Configure(ISceneLoader sceneLoader, IGameServices services)
        {
            if (sceneLoader != null) SceneLoader = sceneLoader;
            if (services != null) Services = services;
        }

        /// <summary>Entry point called by Boot scene.</summary>
        public async void Boot(string homeScene = "HomeBase")
        {
            await BootAsync(homeScene);
        }

        internal async Task BootAsync(string homeScene)
        {
            SetState(AppState.Booting);

            // Initialize services (addressables, analytics/iap stubs, save system, etc.)
            SetState(AppState.Initializing);
            try
            {
                await Services.InitializeAsync();
            }
            catch (Exception)
            {
                SetState(AppState.Error);
                throw;
            }

            // Load the home scene
            SetState(AppState.LoadingHome);
            await SceneLoader.LoadSingleAsync(homeScene);

            // Running
            SetState(AppState.Running);
        }

        private void SetState(AppState next)
        {
            var prev = State;
            State = next;
            EventBus.Publish(new AppStateChanged(prev, next));
            // Optional: Debug.Log($"[Game] {prev} -> {next}");
        }
    }

    /// <summary>Service aggregator to keep Game lean.</summary>
    public interface IGameServices
    {
        Task InitializeAsync();
    }

    /// <summary>Default no-op async initializers for core services.</summary>
    public sealed class DefaultGameServices : IGameServices
    {
        public async Task InitializeAsync()
        {
            // Simulate fast async init boundary so tests and real init share the same path.
            await Task.Yield();

            // Example: register stubs, set up save paths, etc.
            SL.Systems.ServiceLocator.Register<IIAPService>(new SL.Systems.IAPStub());
            SL.Systems.ServiceLocator.Register<IAnalyticsService>(new SL.Systems.AnalyticsStub());
        }
    }
}
