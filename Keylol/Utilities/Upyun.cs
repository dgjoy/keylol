using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Keylol.Utilities
{
    public static class Upyun
    {
        public static string ExtractFileName(string uri)
        {
            var match =
                Regex.Match(uri,
                    @"^(?:(?:(?:http:|https:)?\/\/keylol\.b0\.upaiyun\.com\/)|(?:keylol:\/\/))?([a-z0-9\.]+?)(?:!.*)?$",
                    RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

        public static string CustomVersionUrl(string fileName, string version = null)
        {
            var url = "//keylol.b0.upaiyun.com/" + fileName;
            if (!string.IsNullOrEmpty(version))
                url += "!" + version;
            return url;
        }
    }
}