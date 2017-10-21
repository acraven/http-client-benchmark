namespace WebApp.Infrastructure
{
   using System.Threading;
   using Burble.Abstractions;
   using Burble.Events;
   using log4net;
   using Newtonsoft.Json;

   public class Log4NetHttpEventCallback : IHttpClientEventCallback
   {
      private readonly ILog _logger;

      private int _requests;
      private int _responses;
      private int _timeouts;
      private int _exceptions;
      private int _retries;

      public Log4NetHttpEventCallback()
      {
         _logger = LogManager.GetLogger(typeof(Log4NetHttpEventCallback));
      }

      public int Requests => _requests;

      public int Reponses => _responses;

      public int Timeouts => _timeouts;

      public int Exceptions => _exceptions;

      public int Retries => _retries;

      public void Invoke(HttpClientRequestInitiated @event)
      {
         Interlocked.Increment(ref _requests);
         _logger.Info(JsonConvert.SerializeObject(@event));
      }

      public void Invoke(HttpClientResponseReceived @event)
      {
         Interlocked.Increment(ref _responses);
         _logger.Info(JsonConvert.SerializeObject(@event));
      }
      
      public void Invoke(HttpClientTimedOut @event)
      {
         Interlocked.Increment(ref _timeouts);
         _logger.Info(JsonConvert.SerializeObject(@event));
      }

      public void Invoke(HttpClientExceptionThrown @event)
      {
         Interlocked.Increment(ref _exceptions);
         _logger.Info(JsonConvert.SerializeObject(@event));
      }

      public void Invoke(HttpClientRetryAttempt @event)
      {
         Interlocked.Increment(ref _retries);
         _logger.Info(JsonConvert.SerializeObject(@event));
      }

      public void Invoke(IHttpClientEvent @event)
      {
         Invoke((dynamic)@event);
      }
   }
}