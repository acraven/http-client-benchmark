namespace WebApp
{
   using System.Web.Mvc;
   using System.Web.Routing;

   public class MvcApplication : System.Web.HttpApplication
   {
      protected void Application_Start()
      {
         log4net.Config.XmlConfigurator.Configure();

         AreaRegistration.RegisterAllAreas();
         RouteConfig.RegisterRoutes(RouteTable.Routes);
         AutofacConfig.RegisterDependencies();
      }
   }
}
