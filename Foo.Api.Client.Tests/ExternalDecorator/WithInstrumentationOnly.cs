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

   public class WithInstrumentationOnly
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
            var testSubject = container.ResolveNamed<IGetFoo>("ExternalInstrumentation");

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
      public void should_return_iscomplete_of_false()
      {
         Assert.That(_result.IsComplete, Is.False);
      }

      [Test]
      public void should_return_message_of_null()
      {
         Assert.That(_result.Message, Is.Null);
      }

      [Test]
      public void logs_one_request_initiated_event()
      {
         Assert.That(_callback.Requests, Is.EqualTo(1));
      }

      [Test]
      public void logs_one_response_received_event()
      {
         Assert.That(_callback.Reponses, Is.EqualTo(1));
      }
   }
}
