namespace SL.Systems
{
   public interface IAnalyticsService { void Track(string evt, object data = null); }

   // No-op stub so the project compiles without Analytics packages.
   public sealed class AnalyticsStub : IAnalyticsService
   {
      public void Track(string evt, object data = null) { /* no-op */ }
   }
}
