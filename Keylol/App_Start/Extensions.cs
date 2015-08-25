using System.Web.Mvc;

namespace Keylol
{
    public static class Extensions
    {
        public static int ByteLength(this string str)
        {
            var s = 0;
            for (var i = str.Length - 1; i >= 0; i--)
            {
                var code = (int) str[i];
                if (code <= 0xff) s++;
                else if (code > 0xff && code <= 0xffff) s += 2;
                if (code >= 0xDC00 && code <= 0xDFFF)
                {
                    i--;
                    s++;
                }
            }
            return s;
        }
    }
}