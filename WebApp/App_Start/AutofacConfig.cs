namespace WebApp
{
   using System;
   using System.Web.Mvc;
   using Autofac;
   using Autofac.Integration.Mvc;
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
            HelloWorldUrl = new Uri("http://192.168.99.100:8088"),
            HelloWorldTimeout = TimeSpan.FromMilliseconds(750),
            HelloWorldConcurrency = 3
         };
         builder.RegisterInstance(config);

         builder.RegisterType<HelloWorldController>()
            .WithParameter("failingHelloWorld", context => context.ResolveNamed<IHelloWorld>("FailingHelloWorld"))
            .WithParameter("throttledHelloWorld", context => context.ResolveNamed<IHelloWorld>("ThrottledHelloWorld"));

         builder.RegisterModule<FailingClientModule>();
         builder.RegisterModule<ThrottledClientModule>();

         var container = builder.Build();
         DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
      }
   }
}