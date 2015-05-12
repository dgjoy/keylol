using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.FontGarage.Table.Cmap
{
    public class Environment
    {
        public ushort PlatformId { get; set; }
        public ushort EncodingId { get; set; }
    }

    public abstract class CmapSubtable
    {
        public List<Environment> Environments { get; set; }
        public Dictionary<uint, uint> CharGlyphIdMap { get; set; }
        public abstract ushort Format { get; }

        public abstract void Serialize(BinaryWriter writer);

        protected CmapSubtable()
        {
            Environments = new List<Environment>();
            CharGlyphIdMap = new Dictionary<uint, uint>();
        }
    }
}
