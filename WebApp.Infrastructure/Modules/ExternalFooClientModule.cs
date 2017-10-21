namespace WebApp.Infrastructure.Modules
{
   using System;
   using System.Net.Http;
   using Autofac;
   using Burble;
   using Burble.Abstractions;
   using Burble.Retrying;
   using Burble.Throttling;
   using Foo.Api.Client;

   public class ExternalFooClientModule : Module
   {
      protected override void Load(ContainerBuilder builder)
      {
         Register(builder, CreateHttpClientWithInstrumentationOnly, "ExternalInstrumentation");
         Register(builder, CreateHttpClientWithInstrumentationAndRetry, "ExternalInstrumentationRetry");
         Register(builder, CreateHttpClientWithThrottling, "ExternalInstrumentationRetryThrottling");
      }

      private static void Register(ContainerBuilder builder, Func<IComponentContext, IHttpClient> httpClientFactory, string name)
      {
         builder.RegisterType<FooGetter>()
            .Named<IGetFoo>(name)
            .WithParameter("httpClient", context => context.ResolveNamed<IHttpClient>(name));

         builder.RegisterType<FooSender>()
            .Named<ISendFoo>(name)
            .WithParameter("httpClient", context => context.ResolveNamed<IHttpClient>(name));

         builder.Register(httpClientFactory)
            .Named<IHttpClient>(name)
            .SingleInstance();
      }

      private static IHttpClient CreateHttpClientWithInstrumentationOnly(IComponentContext context)
      {
         var config = context.Resolve<Config>();

         var httpClient = new HttpClient { BaseAddress = config.FooUrl, Timeout = config.FooTimeout };

         var eventCallback = context.Resolve<IHttpClientEventCallback>();

         return new HttpClientAdapter(httpClient)
            .AddInstrumenting(eventCallback);
      }

      private static IHttpClient CreateHttpClientWithInstrumentationAndRetry(IComponentContext context)
      {
         var config = context.Resolve<Config>();

         var httpClient = new HttpClient { BaseAddress = config.FooUrl, Timeout = config.FooTimeout };

         var eventCallback = context.Resolve<IHttpClientEventCallback>();

         return new HttpClientAdapter(httpClient)
            .AddInstrumenting(eventCallback)
            .AddRetrying(new DefaultRetryPredicate(3), new ExponentialRetryDelay(200), eventCallback);
      }

      private static IHttpClient CreateHttpClientWithThrottling(IComponentContext context)
      {
         var config = context.Resolve<Config>();

         var httpClient = new HttpClient { BaseAddress = config.FooUrl, Timeout = config.FooTimeout };
         var throttleSync = new SemaphoneThrottleSync(config.FooConcurrency);

         var eventCallback = context.Resolve<IHttpClientEventCallback>();

         return new HttpClientAdapter(httpClient)
            .AddInstrumenting(eventCallback)
            .AddRetrying(new DefaultRetryPredicate(3), new ExponentialRetryDelay(200), eventCallback)
            .AddThrottling(throttleSync);
      }
   }
}