using System;
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
                "~/Scripts/angular-animate.js",
                "~/Scripts/angular-modal-service.js");
            bundles.Add(vendorJsBundle);

            var appJsBundle = new CustomScriptBundle("~/bundles/angular-app");
            appJsBundle.IncludeDirectory("~/Scripts/app", "*.js", true);
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