namespace WebApp.Controllers
{
   using System.Threading.Tasks;
   using System.Web.Mvc;
   using WebApp.Services;

   public class HelloWorldController : Controller
   {
      private readonly IHelloWorld _failingHelloWorld;
      private readonly IHelloWorld _throttledHelloWorld;

      public HelloWorldController(
         IHelloWorld failingHelloWorld,
         IHelloWorld throttledHelloWorld)
      {
         _failingHelloWorld = failingHelloWorld;
         _throttledHelloWorld = throttledHelloWorld;
      }

      [Route("")]
      public ActionResult Index()
      {
         return View();
      }

      [Route("failing")]
      public async Task<ActionResult> Failing()
      {
         // Purposely no await
         _failingHelloWorld.Get();

         return View("Index");
      }

      [Route("throttled")]
      public async Task<ActionResult> Throttled()
      {
         // Purposely no await
         _throttledHelloWorld.Get();

         return View("Index");
      }
   }
}