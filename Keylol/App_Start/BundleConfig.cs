using System.Web.Optimization;
using BundleTransformer.Core.Bundles;
using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.Resolvers;

namespace Keylol
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleResolver.Current = new CustomBundleResolver();
            
            var nullOrderer = new NullOrderer();

            var vendorJsBundle = new CustomScriptBundle("~/bundles/angular");
            vendorJsBundle.Include(
                "~/Scripts/angular.js",
                "~/Scripts/angular-route.js",
                "~/Scripts/angular-animate.js");
            bundles.Add(vendorJsBundle);

            var appJsBundle = new CustomScriptBundle("~/bundles/angular-app");
            appJsBundle.Include(
                "~/Scripts/app/keylol-app.js",
                "~/Scripts/app/services/page-title-service.js",
                "~/Scripts/app/controllers/root-controller.js",
                "~/Scripts/app/controllers/home-controller.js",
                "~/Scripts/app/controllers/test-controller.js",
                "~/Scripts/app/controllers/section/main-navigation-controller.js",
                "~/Scripts/app/controllers/section/point-recommendation-controller.js",
                "~/Scripts/app/controllers/section/reading-recommendation-controller.js",
                "~/Scripts/app/controllers/section/timeline-controller.js");
            appJsBundle.Orderer = nullOrderer;
            bundles.Add(appJsBundle);

            var cssBundle = new CustomStyleBundle("~/Content/css");
            cssBundle.Include(
                "~/Content/normalize.css",
                "~/Content/site.css");
            cssBundle.Orderer = nullOrderer;
            bundles.Add(cssBundle);
        }
    }
}