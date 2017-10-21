namespace WebApp.Infrastructure
{
   using System;

   public class Config
   {
      public Uri FooUrl { get; set; }

      public TimeSpan FooTimeout { get; set; }

      public int FooConcurrency { get; set; }
   }
}