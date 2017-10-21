namespace Foo.Api.Client.Tests.ExternalDecorator
{
   using System;
   using System.Threading.Tasks;
   using Autofac;
   using Autofac.Core;
   using Burble.Abstractions;
   using NUnit.Framework;
   using WebApp.Infrastructure;
   using WebApp.Infrastructure.Modules;

   public class WithInstrumentationAndRetry
   {
      private FooResult _result;
      private StubHttpEventCallback _callback;

      [OneTimeSetUp]
      public async Task BeforeAllTests()
      {
         _callback = new StubHttpEventCallback();

         using (var fooService = new FooFailsThreeTimesThenSucceeds())
         {
            fooService.Start();

            var config = new Config { FooUrl = fooService.Uri, FooTimeout = TimeSpan.FromSeconds(2) };

            var container = BuildContainer<ExternalFooClientModule>(config, _callback);
            var testSubject = container.ResolveNamed<IGetFoo>("ExternalInstrumentationRetry");

            _result = await testSubject.GetAsync();
         }
      }

      private static IContainer BuildContainer<TModule>(Config config, IHttpClientEventCallback callback) where TModule : IModule, new()
      {
         var containerBuilder = new ContainerBuilder();
         containerBuilder.RegisterModule<TModule>();
         containerBuilder.RegisterInstance(config);
         containerBuilder.RegisterInstance(callback);

         return containerBuilder.Build();
      }

      [Test]
      public void should_return_iscomplete_of_true()
      {
         Assert.That(_result.IsComplete, Is.True);
      }

      [Test]
      public void should_return_message_of_bar()
      {
         Assert.That(_result.Message, Is.EqualTo("bar"));
      }

      [Test]
      public void logs_four_request_initiated_events()
      {
         Assert.That(_callback.Requests, Is.EqualTo(4));
      }

      [Test]
      public void logs_four_response_received_events()
      {
         Assert.That(_callback.Reponses, Is.EqualTo(4));
      }

      [Test]
      public void logs_three_retry_events()
      {
         Assert.That(_callback.Retries, Is.EqualTo(3));
      }
   }
}
