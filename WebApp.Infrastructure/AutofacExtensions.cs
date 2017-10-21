namespace WebApp.Infrastructure
{
   using System;
   using Autofac;
   using Autofac.Builder;

   public static class AutofacExtensions
   {
      public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithParameter<TLimit, TReflectionActivatorData, TStyle>(
         this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, string name,
         Func<IComponentContext, object> valueProvider) where TReflectionActivatorData : ReflectionActivatorData
      {
         return registration.WithParameter((p, context) => p.Name == name, (p, context) => valueProvider(context));
      }
   }
}