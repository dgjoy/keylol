using System.Web.Optimization;

namespace Keylol
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                "~/Scripts/angular.js",
                "~/Scripts/angular-route.js",
                "~/Scripts/angular-animate.js"));

            bundles.Add(new ScriptBundle("~/bundles/angular-app").Include(
                "~/Scripts/app/keylol-app.js",
                "~/Scripts/app/services/page-title-service.js",
                "~/Scripts/app/controllers/home-controller.js",
                "~/Scripts/app/controllers/test-controller.js",
                "~/Scripts/app/controllers/section/main-navigation-controller.js",
                "~/Scripts/app/controllers/section/point-recommendation-controller.js",
                "~/Scripts/app/controllers/section/reading-recommendation-controller.js",
                "~/Scripts/app/controllers/section/timeline-controller.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/normalize.css",
                "~/Content/site.css"));
        }
    }
}