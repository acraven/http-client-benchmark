namespace WebApp
{
   using System;
   using System.Web.Mvc;
   using Autofac;
   using Autofac.Integration.Mvc;
   using Foo.Api.Client;
   using WebApp.Controllers;
   using WebApp.Infrastructure;
   using WebApp.Infrastructure.Modules;
   using WebApp.Services;

   public static class AutofacConfig
   {
      public static void RegisterDependencies()
      {
         var builder = new ContainerBuilder();

         builder.RegisterControllers(typeof(MvcApplication).Assembly);
         builder.RegisterModule<AutofacWebTypesModule>();

         var config = new Config
         {
            FooUrl = new Uri("http://192.168.99.100:8088"),
            //FooUrl = new Uri("http://www.google.co.uk/"),
            FooTimeout = TimeSpan.FromMilliseconds(500),
            FooConcurrency = 3
         };
         builder.RegisterInstance(config);

         builder.RegisterType<HelloWorldController>()
            .WithParameter("internalHttpClientOnly", context => context.ResolveNamed<ISendFoo>("InternalInstrumentation"))
            .WithParameter("externalInstrumentationRetryThrottling", context => context.ResolveNamed<ISendFoo>("ExternalInstrumentationRetryThrottling"));

         builder.RegisterModule<LoggingModule>();
         builder.RegisterModule<InternalFooClientModule>();
         builder.RegisterModule<ExternalFooClientModule>();

         var container = builder.Build();
         DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
      }
   }
}