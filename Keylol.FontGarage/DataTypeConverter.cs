using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Keylol.FontGarage
{
    internal static class DataTypeLength
    {
        public const int UShort = 2;
        public const int Short = 2;
        public const int F2Dot14 = 2;
        public const int Fword = 2;
        public const int UFword = 2;
        public const int ULong = 4;
        public const int Fixed = 4;
        public const int LongDateTime = 8;
    }

    internal static class DataTypeConverter
    {
        private static readonly SmartBitConverter BitConverter = new SmartBitConverter(Endian.BigEndian);

        #region Read

        public static ushort ReadUShort(BinaryReader reader)
        {
            return BitConverter.ToUInt16(reader.ReadBytes(DataTypeLength.UShort), 0);
        }

        public static short ReadShort(BinaryReader reader)
        {
            return BitConverter.ToInt16(reader.ReadBytes(DataTypeLength.Short), 0);
        }

        public static uint ReadULong(BinaryReader reader)
        {
            return BitConverter.ToUInt32(reader.ReadBytes(DataTypeLength.ULong), 0);
        }

        public static string ReadFixed(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(DataTypeLength.Fixed);
            var text = Encoding.ASCII.GetString(bytes);
            if (text == "OTTO" || text == "true" || text == "typ1")
                return text;
            return string.Format("{0}.{1:X2}{2:X2}", BitConverter.ToUInt16(bytes, 0), bytes[2], bytes[3]);
        }

        public static string ReadTag(BinaryReader reader)
        {
            return Encoding.ASCII.GetString(reader.ReadBytes(DataTypeLength.ULong)).TrimEnd();
        }

        public static DateTime ReadLongDateTime(BinaryReader reader)
        {
            var seconds = BitConverter.ToInt64(reader.ReadBytes(DataTypeLength.LongDateTime), 0);
            return new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds);
        }

        public static short ReadFword(BinaryReader reader)
        {
            return ReadShort(reader);
        }

        public static ushort ReadUFword(BinaryReader reader)
        {
            return ReadUShort(reader);
        }

        #endregion

        #region Write

        public static void WriteUShort(BinaryWriter writer, ushort number)
        {
            writer.Write(BitConverter.GetBytes(number));
        }

        public static void WriteShort(BinaryWriter writer, short number)
        {
            writer.Write(BitConverter.GetBytes(number));
        }

        public static void WriteULong(BinaryWriter writer, uint number)
        {
            writer.Write(BitConverter.GetBytes(number));
        }

        public static void WriteFixed(BinaryWriter writer, string value)
        {
            if (value.Contains('.'))
            {
                var parts = value.Split('.');
                writer.Write(BitConverter.GetBytes(ushort.Parse(parts[0])));
                writer.Write(Convert.ToByte(parts[1].Substring(0, 2), 16));
                writer.Write(Convert.ToByte(parts[1].Substring(2, 2), 16));
            }
            else
                WriteTag(writer, value);
        }

        public static void WriteTag(BinaryWriter writer, string tag)
        {
            if (tag.Length > 4)
                throw new ArgumentOutOfRangeException("tag", "Tag too long.");
            if (tag.Length < 4)
                tag = tag.PadRight(4, ' ');
            writer.Write(Encoding.ASCII.GetBytes(tag));
        }

        public static void WriteLongDateTime(BinaryWriter writer, DateTime time)
        {
            writer.Write(
                BitConverter.GetBytes((long) (time - new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds));
        }

        public static void WriteFword(BinaryWriter writer, short fword)
        {
            WriteShort(writer, fword);
        }

        public static void WriteUFword(BinaryWriter writer, ushort uFword)
        {
            WriteUShort(writer, uFword);
        }

        #endregion
    }
}