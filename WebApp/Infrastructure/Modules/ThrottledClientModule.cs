namespace WebApp.Infrastructure.Modules
{
   using System.Net.Http;
   using Autofac;
   using Burble;
   using Burble.Abstractions;
   using Burble.Throttling;
   using log4net;
   using Newtonsoft.Json;
   using WebApp.Services;

   public class ThrottledClientModule : Module
   {
      protected override void Load(ContainerBuilder builder)
      {
         builder.RegisterType<ThrottledHelloWorld>()
            .Named<IHelloWorld>("ThrottledHelloWorld")
            .WithParameter("httpClient", context => context.ResolveNamed<IHttpClient>("ThrottledHttpClientWrapper"));

         builder.Register(CreateThrottledHttpClientWrapper)
            .Named<IHttpClient>("ThrottledHttpClientWrapper");

         builder.Register(CreateThrottledHttpClient)
            .Named<HttpClient>("ThrottledHttpClient")
            .SingleInstance();

         builder.Register(CreateThrottledHelloWorldSync)
            .Named<IThrottleSync>("ThrottledHelloWorldSync")
            .SingleInstance();
      }

      private static IHttpClient CreateThrottledHttpClientWrapper(IComponentContext context)
      {
         var baseHttpClient = context.ResolveNamed<HttpClient>("ThrottledHttpClient");
         var throttleSync = context.ResolveNamed<IThrottleSync>("ThrottledHelloWorldSync");

         var logger = LogManager.GetLogger("Throttled");

         var httpClient = baseHttpClient
            .AddLogging(
               initiated => Log(logger, initiated),
               received => Log(logger, received),
               timeout => Log(logger, timeout),
               exception => Log(logger, exception))
            .AddThrottling(throttleSync);
         return httpClient;
      }

      private static HttpClient CreateThrottledHttpClient(IComponentContext context)
      {
         var config = context.Resolve<Config>();

         return new HttpClient { BaseAddress = config.HelloWorldUrl, Timeout = config.HelloWorldTimeout };
      }

      private static IThrottleSync CreateThrottledHelloWorldSync(IComponentContext context)
      {
         var config = context.Resolve<Config>();

         return new SemaphoneThrottleSync(config.HelloWorldConcurrency);
      }

      private static void Log<TEvent>(ILog logger, TEvent @event)
      {
         logger.Info(JsonConvert.SerializeObject(@event));
      }
   }
}