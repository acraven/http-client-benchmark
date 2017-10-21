namespace Foo.Api.Client
{
   using System.Net.Http;
   using System.Threading.Tasks;
   using Burble.Abstractions;

   public class FooSender : ISendFoo
   {
      private readonly IHttpClient _httpClient;

      public FooSender(IHttpClient httpClient)
      {
         _httpClient = httpClient;
      }

      public async Task SendAsync(string message)
      {
         var request = new HttpRequestMessage(HttpMethod.Post, "/foo");

         var response = await _httpClient.SendAsync(request);

         response.EnsureSuccessStatusCode();
      }
   }
}