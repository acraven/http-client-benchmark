namespace WebApp.Services
{
   using System.Threading.Tasks;
   using Burble;
   using Burble.Abstractions;

   public class ThrottledHelloWorld : IHelloWorld
   {
      private readonly IHttpClient _httpClient;

      public ThrottledHelloWorld(IHttpClient httpClient)
      {
         _httpClient = httpClient;
      }

      public async Task Get()
      {
         // The problem was noticed when posting a blob to an endpoint, but can be equally demonstrated
         // by calling a get endpoint of an existing noddy service.
         var response = await _httpClient.GetAsync("/");

         if (response.IsSuccessStatusCode)
         {
            await response.Content.ReadAsStringAsync();
         }
      }
   }
}