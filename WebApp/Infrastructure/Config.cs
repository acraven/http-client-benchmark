namespace WebApp.Infrastructure
{
   using System;

   public class Config
   {
      public Uri HelloWorldUrl { get; set; }

      public TimeSpan HelloWorldTimeout { get; set; }

      public int HelloWorldConcurrency { get; set; }
   }
}