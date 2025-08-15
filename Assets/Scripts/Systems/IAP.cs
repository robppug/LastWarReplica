namespace SL.Systems
{
   public interface IIAPService { void Init(); }

   // No-op stub so the project compiles without a real IAP implementation.
   public sealed class IAPStub : IIAPService
   {
      public void Init() { /* no-op */ }
   }
}
