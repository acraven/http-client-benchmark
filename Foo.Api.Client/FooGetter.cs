namespace Foo.Api.Client
{
   using System.Threading.Tasks;
   using Burble;
   using Burble.Abstractions;
   using Newtonsoft.Json;
   using Newtonsoft.Json.Linq;

   public class FooGetter : IGetFoo
   {
      private readonly IHttpClient _httpClient;

      public FooGetter(IHttpClient httpClient)
      {
         _httpClient = httpClient;
      }

      public async Task<FooResult> GetAsync()
      {
         var response = await _httpClient.GetAsync("/foo");

         if (response.IsSuccessStatusCode)
         {
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            return new FooResult { IsComplete = true, Message = json["name"].Value<string>() };
         }

         return new FooResult { IsComplete = false };
      }
   }
}