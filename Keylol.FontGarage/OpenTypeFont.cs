using System.Collections.Generic;
using System.Linq;
using Keylol.FontGarage.Table;

namespace Keylol.FontGarage
{
    public class OpenTypeFont
    {
        public OpenTypeFont(string sfntVersion) : this()
        {
            SfntVersion = sfntVersion;
        }

        public OpenTypeFont()
        {
            Tables = new List<IOpenTypeFontTable>();
        }

        public string SfntVersion { get; set; }
        public List<IOpenTypeFontTable> Tables { get; set; }

        /// <summary>
        ///     Get a OpenType table of a specified type. You cannot use this to get BinaryDataTable.
        /// </summary>
        /// <typeparam name="T">Table type.</typeparam>
        /// <returns>The specified OpenType font table.</returns>
        public T Get<T>()
        {
            return Tables.OfType<T>().Single();
        }
    }
}