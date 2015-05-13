using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Keylol.FontGarage.Table;

namespace Keylol.FontGarage
{
    public class OpenTypeFont
    {
        public string SfntVersion { get; set; }
        public List<IOpenTypeFontTable> Tables { get; set; }

        public T Get<T>()
        {
            return Tables.OfType<T>().Single();
        }

        public OpenTypeFont(string sfntVersion) : this()
        {
            SfntVersion = sfntVersion;
        }

        public OpenTypeFont()
        {
            Tables = new List<IOpenTypeFontTable>();
        }
    }
}