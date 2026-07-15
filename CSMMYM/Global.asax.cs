using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using QuestPDF.Infrastructure;


namespace CSMMYM
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            QuestPDF.Settings.License = LicenseType.Community;   // ← ESTA LÍNEA ES OBLIGATORIA

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }
    }
}
