using System;
using System.Web.Optimization;
using AngularTemplates.Bundling;
using AngularTemplates.Compile;
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

            var vendorJsBundle1 = new CustomScriptBundle("~/bundles/vendor-1");
            vendorJsBundle1.Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/moment.js",
                "~/Scripts/moment-local_zh-cn.js",
                "~/Scripts/quill/quill.js",
                "~/Scripts/quill/quill-click-expand.js",
                "~/Scripts/quill/quill-float-toolbar.js");
            bundles.Add(vendorJsBundle1);

            var vendorJsBundle2 = new CustomScriptBundle("~/bundles/vendor-2");
            vendorJsBundle2.Include(
                "~/Scripts/angular.js",
                "~/Scripts/i18n/angular-locale_zh.js",
                "~/Scripts/angular-route.js",
                //"~/Scripts/angular-animate.js",
                "~/Scripts/angular-http-batch.js",
                "~/Scripts/angular-modal-service.js",
                "~/Scripts/angular-moment.js",
                "~/Scripts/ngStorage.js"
                );
            bundles.Add(vendorJsBundle2);

            var appJsBundle = new CustomScriptBundle("~/bundles/app");
            appJsBundle.IncludeDirectory("~/Scripts/app", "*.js", true);
            appJsBundle.Orderer = nullOrderer;
            bundles.Add(appJsBundle);

            var cssBundle = new CustomStyleBundle("~/bundles/css");
            cssBundle.Include(
                "~/Content/normalize.css",
                "~/Scripts/quill/quill.snow.css",
                "~/Content/site.css");
            cssBundle.Orderer = nullOrderer;
            bundles.Add(cssBundle);

            var templateBundle = new TemplateBundle("~/bundles/templates", new TemplateCompilerOptions
            {
                ModuleName = "KeylolApp"
            });
            templateBundle.IncludeDirectory("~/Templates", "*.html", true);
            bundles.Add(templateBundle);
        }
    }
}