namespace WebApp.Services
{
   using System.Net.Http;
   using System.Threading.Tasks;

   public class FailingHelloWorld : IHelloWorld
   {
      private readonly HttpClient _httpClient;

      public FailingHelloWorld(HttpClient httpClient)
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