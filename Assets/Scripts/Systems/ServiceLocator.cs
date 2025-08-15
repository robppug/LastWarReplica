using System;
using System.Collections.Generic;

namespace SL.Systems
{
   public static class ServiceLocator
   {
      private static readonly Dictionary<Type, object> _services = new();

      public static void Register<T>(T svc) where T : class => _services[typeof(T)] = svc;
      public static T Resolve<T>() where T : class => _services.TryGetValue(typeof(T), out var o) ? (T)o : null;
      public static void Clear() => _services.Clear();
   }
}
