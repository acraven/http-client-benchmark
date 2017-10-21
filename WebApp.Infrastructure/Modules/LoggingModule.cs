namespace WebApp.Infrastructure.Modules
{
   using Autofac;
   using Burble.Abstractions;

   public class LoggingModule : Module
   {
      protected override void Load(ContainerBuilder builder)
      {
         var eventCallback = new Log4NetHttpEventCallback();

         builder.RegisterInstance(eventCallback)
            .As<IHttpClientEventCallback>()
            .SingleInstance();
      }
   }
}