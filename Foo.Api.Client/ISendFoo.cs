namespace Foo.Api.Client
{
   using System.Threading.Tasks;

   public interface ISendFoo
   {
      Task SendAsync(string message);
   }
}