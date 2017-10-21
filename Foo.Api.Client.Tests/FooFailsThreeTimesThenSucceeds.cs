namespace Foo.Api.Client.Tests
{
   using System;
   using System.Net;
   using System.Threading;
   using Hornbill;

   public class FooFailsThreeTimesThenSucceeds : FakeService
   {
      public FooFailsThreeTimesThenSucceeds()
      {
         AddResponse(
             "/head",
             Method.HEAD,
             Response.WithDelegate(r =>
             {
                var t = DateTime.UtcNow;
                Console.WriteLine(t.ToString("HH:mm:ss.fff") + " " + r.Path);

                return Response.WithBody(200, "{}");
             }));

         AddResponse(
             "/init",
             Method.GET,
             Response.WithDelegate(r =>
             {
                var t = DateTime.UtcNow;
                Console.WriteLine(t.ToString("HH:mm:ss.fff") + " " + r.Path);

                return Response.WithBody(200, "{}");
             }));

         var attempt = 0;
         AddResponse(
            "/foo",
            Method.GET,
            Response.WithDelegate(r =>
            {
               var t = DateTime.UtcNow;
               Console.WriteLine(t.ToString("HH:mm:ss.fff") + " " + r.Path);

               if (++attempt <= 3)
               {
                  return Response.WithBody((int)HttpStatusCode.InternalServerError, "");
               }

               return Response.WithBody(200, "{\"name\":\"bar\"}");
            }));
      }
   }
}