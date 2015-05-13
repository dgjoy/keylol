using System;
using System.Linq;

namespace Keylol.FontGarage
{
    public enum Endian
    {
        BigEndian,
        LittleEndian
    }

    public class SmartBitConverter
    {
        public Endian ExternalEndian { get; set; }

        public static Endian InternalEndian
        {
            get { return BitConverter.IsLittleEndian ? Endian.LittleEndian : Endian.BigEndian; }
        }

        public SmartBitConverter(Endian externalEndian)
        {
            ExternalEndian = externalEndian;
        }

        // Converts a byte into an array of bytes with length one.
        public byte[] GetBytes(bool value)
        {
            return BitConverter.GetBytes(value);
        }

        // Converts a char into an array of bytes with length two.
        public byte[] GetBytes(char value)
        {
            return InternalEndian == ExternalEndian
                ? BitConverter.GetBytes(value)
                : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        // Converts a short into an array of bytes with length
        // two.
        public byte[] GetBytes(short value)
        {
            return InternalEndian == ExternalEndian
                ? BitConverter.GetBytes(value)
                : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        // Converts an int into an array of bytes with length 
        // four.
        public byte[] GetBytes(int value)
        {
            return InternalEndian == ExternalEndian
                ? BitConverter.GetBytes(value)
                : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        // Converts a long into an array of bytes with length 
        // eight.
        public byte[] GetBytes(long value)
        {
            return InternalEndian == ExternalEndian
                ? BitConverter.GetBytes(value)
                : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        // Converts an ushort into an array of bytes with
        // length two.
        public byte[] GetBytes(ushort value)
        {
            return InternalEndian == ExternalEndian
                ? BitConverter.GetBytes(value)
                : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        // Converts an uint into an array of bytes with
        // length four.
        public byte[] GetBytes(uint value)
        {
            return InternalEndian == ExternalEndian
                ? BitConverter.GetBytes(value)
                : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        // Converts an unsigned long into an array of bytes with
        // length eight.
        public byte[] GetBytes(ulong value)
        {
            return InternalEndian == ExternalEndian
                ? BitConverter.GetBytes(value)
                : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        // Converts a float into an array of bytes with length 
        // four.
        public byte[] GetBytes(float value)
        {
            return InternalEndian == ExternalEndian
                ? BitConverter.GetBytes(value)
                : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        // Converts a double into an array of bytes with length 
        // eight.
        public byte[] GetBytes(double value)
        {
            return InternalEndian == ExternalEndian
                ? BitConverter.GetBytes(value)
                : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        // Converts an array of bytes into a char.  
        public char ToChar(byte[] value, int startIndex)
        {
            var c = BitConverter.ToChar(value, startIndex);
            return InternalEndian == ExternalEndian
                ? c
                : BitConverter.ToChar(BitConverter.GetBytes(c).Reverse().ToArray(), 0);
        }

        // Converts an array of bytes into a short.  
        public short ToInt16(byte[] value, int startIndex)
        {
            var i = BitConverter.ToInt16(value, startIndex);
            return InternalEndian == ExternalEndian
                ? i
                : BitConverter.ToInt16(BitConverter.GetBytes(i).Reverse().ToArray(), 0);
        }

        // Converts an array of bytes into an int.  
        public int ToInt32(byte[] value, int startIndex)
        {
            var i = BitConverter.ToInt32(value, startIndex);
            return InternalEndian == ExternalEndian
                ? i
                : BitConverter.ToInt32(BitConverter.GetBytes(i).Reverse().ToArray(), 0);
        }

        // Converts an array of bytes into a long.  
        public long ToInt64(byte[] value, int startIndex)
        {
            var i = BitConverter.ToInt64(value, startIndex);
            return InternalEndian == ExternalEndian
                ? i
                : BitConverter.ToInt64(BitConverter.GetBytes(i).Reverse().ToArray(), 0);
        }


        // Converts an array of bytes into an ushort.
        // 
        public ushort ToUInt16(byte[] value, int startIndex)
        {
            var i = BitConverter.ToUInt16(value, startIndex);
            return InternalEndian == ExternalEndian
                ? i
                : BitConverter.ToUInt16(BitConverter.GetBytes(i).Reverse().ToArray(), 0);
        }

        // Converts an array of bytes into an uint.
        // 
        public uint ToUInt32(byte[] value, int startIndex)
        {
            var i = BitConverter.ToUInt32(value, startIndex);
            return InternalEndian == ExternalEndian
                ? i
                : (i << 24 | (i & 0x0000FF00) << 8 | (i & 0x00FF0000) >> 8 | i >> 24);
        }

        // Converts an array of bytes into an unsigned long.
        // 
        public ulong ToUInt64(byte[] value, int startIndex)
        {
            var i = BitConverter.ToUInt64(value, startIndex);
            return InternalEndian == ExternalEndian
                ? i
                : BitConverter.ToUInt64(BitConverter.GetBytes(i).Reverse().ToArray(), 0);
        }

        // Converts an array of bytes into a float.  
        public float ToSingle(byte[] value, int startIndex)
        {
            var s = BitConverter.ToSingle(value, startIndex);
            return InternalEndian == ExternalEndian
                ? s
                : BitConverter.ToSingle(BitConverter.GetBytes(s).Reverse().ToArray(), 0);
        }

        // Converts an array of bytes into a double.  
        public double ToDouble(byte[] value, int startIndex)
        {
            var d = BitConverter.ToDouble(value, startIndex);
            return InternalEndian == ExternalEndian
                ? d
                : BitConverter.ToDouble(BitConverter.GetBytes(d).Reverse().ToArray(), 0);
        }

        // Converts an array of bytes into a String.  
        public String ToString(byte[] value, int startIndex, int length)
        {
            return BitConverter.ToString(value, startIndex, length);
        }

        // Converts an array of bytes into a String.  
        public String ToString(byte[] value)
        {
            return BitConverter.ToString(value);
        }

        // Converts an array of bytes into a String.  
        public String ToString(byte[] value, int startIndex)
        {
            return BitConverter.ToString(value, startIndex);
        }

        /*==================================ToBoolean===================================
        **Action:  Convert an array of bytes to a boolean value.  We treat this array 
        **         as if the first 4 bytes were an Int4 an operate on this value.
        **Returns: True if the Int4 value of the first 4 bytes is non-zero.
        **Arguments: value -- The byte array
        **           startIndex -- The position within the array.
        **Exceptions: See ToInt4.
        ==============================================================================*/
        // Converts an array of bytes into a boolean.  
        public bool ToBoolean(byte[] value, int startIndex)
        {
            return BitConverter.ToBoolean(value, startIndex);
        }

        public long DoubleToInt64Bits(double value)
        {
            return BitConverter.DoubleToInt64Bits(value);
        }

        public double Int64BitsToDouble(long value)
        {
            return BitConverter.Int64BitsToDouble(value);
        }
    }
}