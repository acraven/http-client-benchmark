namespace Foo.Api.Client
{
   using System.Threading.Tasks;

   public class FooResult
   {
      public bool IsComplete { get; set; }

      public string Message { get; set; }
   }

   public interface IGetFoo
   {
      Task<FooResult> GetAsync();
   }
}
