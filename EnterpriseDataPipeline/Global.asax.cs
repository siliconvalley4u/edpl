using DynamicMVC.UI.DynamicMVC;
using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using DynamicMVC.Data;
using DynamicMVC.Data.Interfaces;
using DynamicMVC.Shared;
using DynamicMVC.Shared.Interfaces;
using DynamicMVC.Shared.Models;
using DynamicMVC.UI.DynamicMVC.Interfaces;
using UnityConfig = EnterpriseDataPipeline.App_Start.UnityConfig;



namespace EnterpriseDataPipeline
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            if (typeof(MvcApplication).Assembly.ManifestModule.Name.ToUpper() == "DYNAMICMVC.DLL")
            {
                throw new Exception("Your UI assembly cannot be named DynamicMVC.  This conflicts with dynamicmvc.dll");
            }
            //var applicationMetadata = new DynamicMVC.Business.Models.ApplicationMetadata(typeof(MvcApplication).Assembly,
            //    typeof(MvcApplication).Assembly, typeof(MvcApplication).Assembly,
            //    () => new DynamicMVC.Data.DynamicRepository(new EnterpriseDataPipeline.Models.ApplicationDbContext()));

            //DynamicMVC.Managers.DynamicMVCManager.ParseApplicationMetadata(applicationMetadata);
            //DynamicMVCContext.DynamicMvcManager.RegisterDynamicMvc(applicationMetadata);


            //DynamicMVC.Managers.DynamicMVCManager.SetDynamicRoutes(RouteTable.Routes);
            //DynamicMVCContext.DynamicMvcManager.SetDynamicRoutes(RouteTable.Routes);

            //load mvc container and use it as dynamic mvc container
            var mvcContainer = UnityConfig.GetConfiguredContainer();
            DynamicMVC.Shared.Container.EagerLoadedContainer = mvcContainer;
            DynamicMVC.Shared.UnityConfig.RegisterTypes(mvcContainer);

            DynamicMVCUnityConfig.RegisterTypes(Container.GetConfiguredContainer());
            UnityConfig.RegisterTypes(Container.GetConfiguredContainer());
            ICreateDbContextManager createDbContextManager = new CreateDbContextManager(() => new EnterpriseDataPipeline.Models.ApplicationDbContext());
            Container.RegisterInstance(createDbContextManager);
            var applicationMetadataProvider = new ApplicationMetadataProvider(typeof(MvcApplication).Assembly, typeof(MvcApplication).Assembly, typeof(MvcApplication).Assembly);
            Container.RegisterInstance<IApplicationMetadataProvider>(applicationMetadataProvider);
            DynamicMVCContext.DynamicMvcManager = Container.Resolve<IDynamicMvcManager>();
            Container.RegisterInstance(DynamicMVCContext.DynamicMvcManager);

            DynamicMVCContext.DynamicMvcManager.RegisterDynamicMvc();
            DynamicMVCContext.DynamicMvcManager.SetDynamicRoutes(RouteTable.Routes);

        }
    }
}
