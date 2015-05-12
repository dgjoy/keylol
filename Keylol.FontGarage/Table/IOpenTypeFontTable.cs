using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.FontGarage.Table
{
    public interface IOpenTypeFontTable
    {
        string Tag { get; }

        void Serialize(BinaryWriter writer);
    }
}
