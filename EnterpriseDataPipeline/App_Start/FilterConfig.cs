using System.Web;
using System.Web.Mvc;

namespace EnterpriseDataPipeline
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());

            // use the [AllowAnonymous] attribute to open up individual Actions/Controllers
            //filters.Add(new System.Web.Mvc.AuthorizeAttribute());
            //filters.Add(new RequireHttpsAttribute());
        }
    }
}
