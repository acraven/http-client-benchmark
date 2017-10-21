namespace Foo.Api.Client.Tests
{
   using System;
   using System.Threading;
   using Burble.Abstractions;
   using Burble.Events;

   public class StubHttpEventCallback : IHttpClientEventCallback
   {
      private int _requests;
      private int _responses;
      private int _timeouts;
      private int _exceptions;
      private int _retries;

      public int Requests => _requests;

      public int Reponses => _responses;

      public int Timeouts => _timeouts;

      public int Exceptions => _exceptions;

      public int Retries => _retries;

      public void Invoke(HttpClientRequestInitiated @event)
      {
         Interlocked.Increment(ref _requests);
      }

      public void Invoke(HttpClientResponseReceived @event)
      {
         Interlocked.Increment(ref _responses);
      }

      public void Invoke(HttpClientTimedOut @event)
      {
         Interlocked.Increment(ref _timeouts);
      }

      public void Invoke(HttpClientExceptionThrown @event)
      {
         Interlocked.Increment(ref _exceptions);
      }

      public void Invoke(HttpClientRetryAttempt @event)
      {
         Interlocked.Increment(ref _retries);
      }

      public void Invoke(IHttpClientEvent @event)
      {
         var t = DateTime.UtcNow;
         Console.WriteLine(t.ToString("HH:mm:ss.fff") + " " + @event.GetType().Name);
         Invoke((dynamic)@event);
      }
   }
}