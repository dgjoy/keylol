using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zlib;

namespace Keylol.FontGarage
{
    public static class FontFormatConverter
    {
        /// <summary>
        /// Convert sfnt file to woff file.
        /// </summary>
        /// <param name="reader">BinaryReader to read the sfnt data stream.</param>
        /// <param name="writer">BinaryWriter to write the woff data.</param>
        /// <param name="compression">Set to true to compress data.</param>
        public static void SfntToWoff(BinaryReader reader, BinaryWriter writer, bool compression = false)
        {
            reader.BaseStream.Position = 0;
            writer.BaseStream.Position = 0;

            // Header
            DataTypeConverter.WriteULong(writer, 0x774F4646); // signature
            writer.Write(reader.ReadBytes(DataTypeLength.Fixed)); // flavor
            writer.BaseStream.Position += DataTypeLength.ULong; // length
            var numberOfTables = DataTypeConverter.ReadUShort(reader);
            DataTypeConverter.WriteUShort(writer, numberOfTables); // numTables
            DataTypeConverter.WriteUShort(writer, 0); // reserved
            DataTypeConverter.WriteULong(writer, (uint) reader.BaseStream.Length); // totalSfntSize
            DataTypeConverter.WriteUShort(writer, 1); // majorVision
            DataTypeConverter.WriteUShort(writer, 0); // minorVision
            DataTypeConverter.WriteULong(writer, 0); // metaOffset
            DataTypeConverter.WriteULong(writer, 0); // metaLength
            DataTypeConverter.WriteULong(writer, 0); // metaOriLength
            DataTypeConverter.WriteULong(writer, 0); // privOffset
            DataTypeConverter.WriteULong(writer, 0); // privLength

            // Read original table directory
            reader.BaseStream.Position += 3*DataTypeLength.UShort;
            var tags = new uint[numberOfTables];
            var offsets = new uint[numberOfTables];
            var compLengths = new uint[numberOfTables];
            var origLengths = new uint[numberOfTables];
            var origChecksum = new uint[numberOfTables];
            var origOffsets = new uint[numberOfTables];
            var startOffsetOfTableDirectory = writer.BaseStream.Position;
            for (var i = 0; i < numberOfTables; i++)
            {
                tags[i] = DataTypeConverter.ReadULong(reader);
                origChecksum[i] = DataTypeConverter.ReadULong(reader);
                origOffsets[i] = DataTypeConverter.ReadULong(reader);
                origLengths[i] = DataTypeConverter.ReadULong(reader);
            }

            // Table data
            writer.BaseStream.Position += numberOfTables*5*DataTypeLength.ULong;
            for (var i = 0; i < numberOfTables; i++)
            {
                offsets[i] = (uint) writer.BaseStream.Position;

                reader.BaseStream.Position = origOffsets[i];
                var tableData = reader.ReadBytes((int) origLengths[i]);
                if (compression)
                {
                    using (var compressedStream = new MemoryStream())
                    {
                        var zOutputStream = new ZOutputStream(compressedStream, zlibConst.Z_DEFAULT_COMPRESSION);
                        zOutputStream.Write(tableData, 0, (int) origLengths[i]);
                        zOutputStream.finish();

                        if (compressedStream.Length >= origLengths[i])
                            writer.Write(tableData);
                        else
                            compressedStream.WriteTo(writer.BaseStream);
                    }
                }
                else
                {
                    writer.Write(tableData);
                }

                compLengths[i] = (uint) (writer.BaseStream.Position - offsets[i]);

                // 4byte padding
                if (writer.BaseStream.Position % 4 != 0)
                {
                    var zeroCount = 4 - writer.BaseStream.Position % 4;
                    for (var j = 0; j < zeroCount; j++)
                        writer.Write((byte) 0);
                }
            }

            // Write table directory
            writer.BaseStream.Position = startOffsetOfTableDirectory;
            for (var i = 0; i < numberOfTables; i++)
            {
                DataTypeConverter.WriteULong(writer, tags[i]);
                DataTypeConverter.WriteULong(writer, offsets[i]);
                DataTypeConverter.WriteULong(writer, compLengths[i]);
                DataTypeConverter.WriteULong(writer, origLengths[i]);
                DataTypeConverter.WriteULong(writer, origChecksum[i]);
            }

            // Write length in header
            writer.BaseStream.Position = 2*DataTypeLength.ULong;
            DataTypeConverter.WriteULong(writer, (uint) writer.BaseStream.Length);
        }
    }
}
