namespace WebApp.Infrastructure.Modules
{
   using System;
   using System.Diagnostics;
   using System.Net.Http;
   using System.Threading;
   using System.Threading.Tasks;
   using Autofac;
   using Burble;
   using Burble.Abstractions;
   using Burble.Events;
   using Burble.Retrying;
   using Foo.Api.Client;

   public class InternalFooClientModule : Module
   {
      protected override void Load(ContainerBuilder builder)
      {
         Register(builder, CreateHttpClientWithInstrumentationOnly, "InternalInstrumentation");
         Register(builder, CreateHttpClientWithInstrumentationAndRetry, "InternalInstrumentationRetry");
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
         var eventCallback = context.Resolve<IHttpClientEventCallback>();

         var loggingHandler = new InstrumentationHandler(config.FooUrl, eventCallback);
         var httpClient = HttpClientFactory.Create(loggingHandler);
         httpClient.BaseAddress = config.FooUrl;
         httpClient.Timeout = config.FooTimeout;

         return new HttpClientAdapter(httpClient);
      }

      private static IHttpClient CreateHttpClientWithInstrumentationAndRetry(IComponentContext context)
      {
         var config = context.Resolve<Config>();
         var eventCallback = context.Resolve<IHttpClientEventCallback>();

         var loggingHandler = new InstrumentationHandler(config.FooUrl, eventCallback);
         var retryingHandler = new RetryingHandler(config.FooUrl, config.FooTimeout, new DefaultRetryPredicate(3), new ExponentialRetryDelay(200), eventCallback);

         var httpClient = HttpClientFactory.Create(retryingHandler, loggingHandler);
         httpClient.BaseAddress = config.FooUrl;
         httpClient.Timeout = TimeSpan.FromSeconds(10);

         //public int AggregateTimeoutMs
         //} (_maxRetryAttempts + 1) * _timeout + _retryDelay.AggregateDelayMs(_maxRetryAttempts);


         return new HttpClientAdapter(httpClient);
      }

      private class InstrumentationHandler : DelegatingHandler
      {
         private readonly IHttpClientEventCallback _callback;
         private readonly Uri _baseAddress;

         public InstrumentationHandler(Uri baseAddress, IHttpClientEventCallback callback)
         {
            _callback = callback;
            _baseAddress = baseAddress;
         }

         protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
         {
            var stopwatch = Stopwatch.StartNew();

            try
            {
               _callback.Invoke(HttpClientRequestInitiated.Create(request, _baseAddress));
               var response = await base.SendAsync(request, cancellationToken);
               _callback.Invoke(HttpClientResponseReceived.Create(response, _baseAddress, stopwatch.ElapsedMilliseconds));
               return response;
            }
            catch (TaskCanceledException)
            {
               _callback.Invoke(HttpClientTimedOut.Create(request, _baseAddress, stopwatch.ElapsedMilliseconds));
               throw;
            }
            catch (Exception e)
            {
               _callback.Invoke(HttpClientExceptionThrown.Create(request, _baseAddress, stopwatch.ElapsedMilliseconds, e));
               throw;
            }
         }
      }

      public class RetryingHandler : DelegatingHandler
      {
         private readonly Uri _baseAddress;
         private readonly TimeSpan _timeout;
         private readonly IRetryPredicate _retryPredicate;
         private readonly IRetryDelay _retryDelay;
         private readonly IHttpClientEventCallback _callback;

         public RetryingHandler(
            Uri baseAddress,
            TimeSpan timeout,
            IRetryPredicate retryPredicate,
            IRetryDelay retryDelay,
            IHttpClientEventCallback callback)
         {
            _baseAddress = baseAddress;
            _timeout = timeout;
            _retryPredicate = retryPredicate;
            _retryDelay = retryDelay;
            _callback = callback;
         }

         protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
         {
            var retryAttempts = 0;

            while (true)
            {
               // Create a new cancellation token just for the downstream call
               var cts = CancellationTokenSource.CreateLinkedTokenSource(
                  new CancellationTokenSource(_timeout).Token,
                  cancellationToken);

               HttpResponseMessage response = null;
               Exception exception = null;

               try
               {
                  response = await base.SendAsync(request, cts.Token);
               }
               catch (Exception e)
               {
                  exception = e;
               }

               retryAttempts++;

               if (!_retryPredicate.ShouldRetry(retryAttempts, response))
               {
                  if (exception != null)
                  {
                     throw exception;
                  }

                  return response;
               }

               var delayMs = _retryDelay.DelayMs(retryAttempts);
               await Task.Delay(delayMs, CancellationToken.None);

               _callback.Invoke(HttpClientRetryAttempt.Create(request, _baseAddress, retryAttempts));
            }
         }
      }
   }
}