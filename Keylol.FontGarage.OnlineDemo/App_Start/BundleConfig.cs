using System.Web;
using System.Web.Optimization;

namespace Keylol.FontGarage.OnlineDemo
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.validate.js",
                        "~/Scripts/jquery.validate.unobtrusive.js",
                        "~/Scripts/jquery.autogrow-textarea.js",
                        "~/Scripts/jquery.fontface.js",
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/spin.js",
                        "~/Scripts/ladda.js",
                        "~/Scripts/site.js"));

            bundles.Add(new StyleBundle("~/bundles/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/ladda-themeless.css",
                      "~/Content/site.css"));
        }
    }
}
