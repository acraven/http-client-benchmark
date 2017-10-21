namespace WebApp.Controllers
{
   using System.Web.Mvc;
   using Foo.Api.Client;

   public class HelloWorldController : Controller
   {
      private readonly ISendFoo _internalHttpClientOnly;
      private readonly ISendFoo _externalInstrumentationRetryThrottling;

      public HelloWorldController(
         ISendFoo internalHttpClientOnly,
         ISendFoo externalInstrumentationRetryThrottling)
      {
         _internalHttpClientOnly = internalHttpClientOnly;
         _externalInstrumentationRetryThrottling = externalInstrumentationRetryThrottling;
      }

      [Route("")]
      public ActionResult Index()
      {
         return View("Stats");
      }

      [Route("failing")]
      public ActionResult Failing(string message)
      {
         // Purposely no await
         _internalHttpClientOnly.SendAsync(message);

         return View("Index");
      }

      [Route("throttled")]
      public ActionResult Throttled(string message)
      {
         // Purposely no await
         _externalInstrumentationRetryThrottling.SendAsync(message);

         return View("Index");
      }
   }
}