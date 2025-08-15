using System;
using System.Collections.Generic;

namespace SL.Core
{
    /// <summary>Minimal type-safe event bus. Subscribe/Unsubscribe and Publish by type T.</summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            var t = typeof(T);
            if (!_handlers.TryGetValue(t, out var list))
            {
                list = new List<Delegate>();
                _handlers[t] = list;
            }
            if (!list.Contains(handler)) list.Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            var t = typeof(T);
            if (_handlers.TryGetValue(t, out var list))
            {
                list.Remove(handler);
                if (list.Count == 0) _handlers.Remove(t);
            }
        }

        public static void Publish<T>(T evt)
        {
            var t = typeof(T);
            if (!_handlers.TryGetValue(t, out var list)) return;
            // Copy to avoid mutation during iteration
            var snapshot = list.ToArray();
            foreach (var d in snapshot)
            {
                if (d is Action<T> a) a.Invoke(evt);
            }
        }
    }

    // Common events
    public readonly struct AppStateChanged
    {
        public readonly AppState From;
        public readonly AppState To;
        public AppStateChanged(AppState from, AppState to) { From = from; To = to; }
        public override string ToString() => $"AppStateChanged {From} -> {To}";
    }
}
