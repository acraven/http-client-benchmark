using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Foo.Api.Client.Tests
{
   using System;
   using Burble.Abstractions;
   using Burble.Events;

   public class StubHttpEventCallback : IHttpClientEventCallback
   {
      private readonly ConcurrentQueue<HttpClientRequestInitiated> _requests = new ConcurrentQueue<HttpClientRequestInitiated>();
      private readonly ConcurrentQueue<HttpClientResponseReceived> _responses = new ConcurrentQueue<HttpClientResponseReceived>();
      private readonly ConcurrentQueue<HttpClientTimedOut> _timeouts = new ConcurrentQueue<HttpClientTimedOut>();
      private readonly ConcurrentQueue<HttpClientExceptionThrown> _exceptions = new ConcurrentQueue<HttpClientExceptionThrown>();
      private readonly ConcurrentQueue<HttpClientRetryAttempt> _retries = new ConcurrentQueue<HttpClientRetryAttempt>();

      public IEnumerable<HttpClientRequestInitiated> Requests => _requests;

      public IEnumerable<HttpClientResponseReceived> Responses => _responses;

      public IEnumerable<HttpClientTimedOut> Timeouts => _timeouts;

      public IEnumerable<HttpClientExceptionThrown> Exceptions => _exceptions;

      public IEnumerable<HttpClientRetryAttempt> Retries => _retries;

      public void Invoke(HttpClientRequestInitiated @event)
      {
         _requests.Enqueue(@event);
      }

      public void Invoke(HttpClientResponseReceived @event)
      {
         _responses.Enqueue(@event);
      }

      public void Invoke(HttpClientTimedOut @event)
      {
         _timeouts.Enqueue(@event);
      }

      public void Invoke(HttpClientExceptionThrown @event)
      {
         _exceptions.Enqueue(@event);
      }

      public void Invoke(HttpClientRetryAttempt @event)
      {
         _retries.Enqueue(@event);
      }

      public void Invoke(IHttpClientEvent @event)
      {
         var t = DateTime.UtcNow;
         Console.WriteLine(t.ToString("HH:mm:ss.fff") + " " + @event.GetType().Name);
         Invoke((dynamic)@event);
      }
   }
}