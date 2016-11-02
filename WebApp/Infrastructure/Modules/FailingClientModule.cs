namespace WebApp.Infrastructure.Modules
{
   using System;
   using System.Diagnostics;
   using System.Net.Http;
   using System.Threading;
   using System.Threading.Tasks;
   using Autofac;
   using Burble.Events;
   using log4net;
   using Newtonsoft.Json;
   using WebApp.Services;

   public class FailingClientModule : Module
   {
      protected override void Load(ContainerBuilder builder)
      {
         builder.RegisterType<FailingHelloWorld>()
            .Named<IHelloWorld>("FailingHelloWorld")
            .WithParameter("httpClient", context => context.ResolveNamed<HttpClient>("FailingHttpClient"));

         builder.Register(CreateFailingHttpClient)
            .Named<HttpClient>("FailingHttpClient")
            .SingleInstance();
      }

      private static HttpClient CreateFailingHttpClient(IComponentContext context)
      {
         var config = context.Resolve<Config>();
         var logger = LogManager.GetLogger("Failing");

         var loggingHandler = new LoggingHandler(
               initiated => Log(logger, initiated),
               received => Log(logger, received),
               timeout => Log(logger, timeout),
               exception => Log(logger, exception));

         var httpClient = HttpClientFactory.Create(loggingHandler);
         httpClient.BaseAddress = config.HelloWorldUrl;
         httpClient.Timeout = config.HelloWorldTimeout;

         return httpClient;
      }

      private static void Log<TEvent>(ILog logger, TEvent @event)
      {
         logger.Info(JsonConvert.SerializeObject(@event));
      }

      private class LoggingHandler : DelegatingHandler
      {
         private readonly Action<HttpClientRequestInitiated> _onInitiated;
         private readonly Action<HttpClientResponseReceived> _onReceived;
         private readonly Action<HttpClientTimedOut> _onTimeout;
         private readonly Action<HttpClientExceptionThrown> _onException;

         public LoggingHandler(
            Action<HttpClientRequestInitiated> onInitiated,
            Action<HttpClientResponseReceived> onReceived,
            Action<HttpClientTimedOut> onTimeout,
            Action<HttpClientExceptionThrown> onException)
         {
            _onInitiated = onInitiated;
            _onReceived = onReceived;
            _onTimeout = onTimeout;
            _onException = onException;
         }

         protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
         {
            var stopwatch = Stopwatch.StartNew();

            try
            {
               _onInitiated(HttpClientRequestInitiated.Create(request));
               var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
               _onReceived(HttpClientResponseReceived.Create(response, stopwatch.ElapsedMilliseconds));
               return response;
            }
            catch (TaskCanceledException)
            {
               _onTimeout(HttpClientTimedOut.Create(request, stopwatch.ElapsedMilliseconds));
               throw;
            }
            catch (Exception e)
            {
               _onException(HttpClientExceptionThrown.Create(request, stopwatch.ElapsedMilliseconds, e));
               throw;
            }
         }
      }
   }
}