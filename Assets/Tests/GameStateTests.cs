using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SL.Core;
using SL.Systems;
using UnityEngine;

public class GameStateTests
{
    private sealed class FakeLoader : ISceneLoader
    {
        public readonly List<string> Loaded = new();
        public Task LoadSingleAsync(string sceneName)
        {
            Loaded.Add(sceneName);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeServices : IGameServices
    {
        public bool Initialized { get; private set; }
        public Task InitializeAsync()
        {
            Initialized = true;
            return Task.CompletedTask;
        }
    }

    [SetUp]
    public void SetUp()
    {
        // Make sure a fresh Game exists per test
        if (GameObject.Find("[Game]") == null)
        {
            _ = Game.Instance; // forces creation
        }
    }

    [TearDown]
    public void TearDown()
    {
#if UNITY_EDITOR
        // Clean up singleton to avoid cross-test leakage
        var go = GameObject.Find("[Game]");
        if (go != null) Object.DestroyImmediate(go);
#endif
    }

    [Test]
    public async Task BootAsync_TransitionsThroughExpectedStates_AndLoadsHome()
    {
        var loader = new FakeLoader();
        var services = new FakeServices();

        // Subscription capture
        var sequence = new List<AppState>();
        void OnState(AppStateChanged e) => sequence.Add(e.To);
        EventBus.Subscribe<AppStateChanged>(OnState);

        try
        {
            Game.Instance.Configure(loader, services);
            Assert.AreEqual(AppState.None, Game.Instance.State);

            await Game.Instance.BootAsync("HomeBase");

            // Verify services initialized and scene "loaded"
            Assert.IsTrue(services.Initialized);
            CollectionAssert.Contains(loader.Loaded, "HomeBase");

            // Verify terminal state
            Assert.AreEqual(AppState.Running, Game.Instance.State);

            // Expected ordered states after None: Booting, Initializing, LoadingHome, Running
            CollectionAssert.AreEqual(
                new[] { AppState.Booting, AppState.Initializing, AppState.LoadingHome, AppState.Running },
                sequence.ToArray()
            );
        }
        finally
        {
            EventBus.Unsubscribe<AppStateChanged>(OnState);
        }
    }
}
