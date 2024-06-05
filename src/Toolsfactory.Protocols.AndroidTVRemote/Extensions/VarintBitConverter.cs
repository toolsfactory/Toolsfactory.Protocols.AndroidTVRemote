using System;
using System.Linq;
namespace System
{

    /// <summary>
    /// Helpers for Google Protocol Buffers Varint encoding/decoding.
    /// </summary>
    public static class  VarIntExtensions
    {
        #region VarInt Encoding helpers
        /// <summary>
        /// Returns the specified byte value as varint encoded array of bytes.   
        /// </summary>
        /// <param name="value">Byte value</param>
        /// <returns>Varint array of bytes.</returns>
        public static byte[] ToVarIntBytes(this byte value)
        {
            return ToVarIntBytes((ulong) value);
        }

        /// <summary>
        /// Returns the specified 16-bit signed value as varint encoded array of bytes.   
        /// </summary>
        /// <param name="value">16-bit signed value</param>
        /// <returns>Varint array of bytes.</returns>
        public static byte[] ToVarIntBytes(this short value)
        {
            var zigzag = EncodeZigZag(value, 16);
            return ToVarIntBytes((ulong) zigzag);
        }

        /// <summary>
        /// Returns the specified 16-bit unsigned value as varint encoded array of bytes.   
        /// </summary>
        /// <param name="value">16-bit unsigned value</param>
        /// <returns>Varint array of bytes.</returns>
        public static byte[] ToVarIntBytes(this ushort value)
        {
            return ToVarIntBytes((ulong) value);
        }

        /// <summary>
        /// Returns the specified 32-bit signed value as varint encoded array of bytes.   
        /// </summary>
        /// <param name="value">32-bit signed value</param>
        /// <returns>Varint array of bytes.</returns>
        public static byte[] ToVarIntBytes(this int value)
        {
            var zigzag = EncodeZigZag(value, 32);
            return ToVarIntBytes((ulong) zigzag);
        }

        /// <summary>
        /// Returns the specified 32-bit unsigned value as varint encoded array of bytes.   
        /// </summary>
        /// <param name="value">32-bit unsigned value</param>
        /// <returns>Varint array of bytes.</returns>
        public static byte[] ToVarIntBytes(this uint value)
        {
            return ToVarIntBytes((ulong) value);
        }

        /// <summary>
        /// Returns the specified 64-bit signed value as varint encoded array of bytes.   
        /// </summary>
        /// <param name="value">64-bit signed value</param>
        /// <returns>Varint array of bytes.</returns>
        public static byte[] ToVarIntBytes(this long value)
        {
            var zigzag = EncodeZigZag(value, 64);
            return ToVarIntBytes((ulong) zigzag);
        }

        /// <summary>
        /// Returns the specified 64-bit unsigned value as varint encoded array of bytes.   
        /// </summary>
        /// <param name="value">64-bit unsigned value</param>
        /// <returns>Varint array of bytes.</returns>
        public static byte[] ToVarIntBytes(this ulong value)
        {
            var buffer = new byte[10];
            var pos = 0;
            do
            {
                var byteVal = value & 0x7f;
                value >>= 7;

                if (value != 0)
                {
                    byteVal |= 0x80;
                }

                buffer[pos++] = (byte) byteVal;

            } while (value != 0);

            var result = new byte[pos];
            Buffer.BlockCopy(buffer, 0, result, 0, pos);

            return result;
        }
        #endregion

        #region VarInt Decoding helpers
        /// <summary>
        /// Returns byte value from varint encoded array of bytes.
        /// </summary>
        /// <param name="bytes">Varint encoded array of bytes.</param>
        /// <returns>Byte value</returns>
        public static byte ToByte(this byte[] bytes)
        {
            return (byte) InternalConvertToUInt64(bytes, 8);
        }

        /// <summary>
        /// Returns 16-bit signed value from varint encoded array of bytes.
        /// </summary>
        /// <param name="bytes">Varint encoded array of bytes.</param>
        /// <returns>16-bit signed value</returns>
        public static short ToInt16(this byte[] bytes)
        {
            var zigzag = InternalConvertToUInt64(bytes, 16);
            return (short) DecodeZigZag(zigzag);
        }

        /// <summary>
        /// Returns 16-bit usigned value from varint encoded array of bytes.
        /// </summary>
        /// <param name="bytes">Varint encoded array of bytes.</param>
        /// <returns>16-bit usigned value</returns>
        public static ushort ToUInt16(this byte[] bytes)
        {
            return (ushort) InternalConvertToUInt64(bytes, 16);
        }

        /// <summary>
        /// Returns 32-bit signed value from varint encoded array of bytes.
        /// </summary>
        /// <param name="bytes">Varint encoded array of bytes.</param>
        /// <returns>32-bit signed value</returns>
        public static int ToInt32(this byte[] bytes)
        {
            var zigzag = InternalConvertToUInt64(bytes, 32);
            return (int) DecodeZigZag(zigzag);
        }

        /// <summary>
        /// Returns 32-bit unsigned value from varint encoded array of bytes.
        /// </summary>
        /// <param name="bytes">Varint encoded array of bytes.</param>
        /// <returns>32-bit unsigned value</returns>
        public static uint ToUInt32(this byte[] bytes)
        {
            return (uint) InternalConvertToUInt64(bytes, 32);
        }

        /// <summary>
        /// Returns 64-bit signed value from varint encoded array of bytes.
        /// </summary>
        /// <param name="bytes">Varint encoded array of bytes.</param>
        /// <returns>64-bit signed value</returns>
        public static long ToInt64(this byte[] bytes)
        {
            var zigzag = InternalConvertToUInt64(bytes, 64);
            return DecodeZigZag(zigzag);
        }

        /// <summary>
        /// Returns 64-bit unsigned value from varint encoded array of bytes.
        /// </summary>
        /// <param name="bytes">Varint encoded array of bytes.</param>
        /// <returns>64-bit unsigned value</returns>
        public static ulong ToUInt64(this byte[] bytes)
        {
            return InternalConvertToUInt64(bytes, 64);
        }

        public static async Task<ulong> ReadVarIntAsync(this Stream stream, CancellationToken token)
        {
            int shift = 0;
            ulong result = 0;

            for (int i = 0; i < 10; i++)
            {
                byte b = await stream.ReadByteAsync(token);
                result |= (ulong) (b & 0x7f) << shift;

                if ((b & 0x80) != 0x80)
                    return result;

                shift += 7;
            }
            throw new ArgumentException("Decoding varint from stream failed.");
        }

        private static ulong InternalConvertToUInt64(byte[] bytes, int cntBits)
        {
            int shift = 0;
            ulong result = 0;

            foreach (ulong byteValue in bytes)
            {
                result |= (byteValue & 0x7f) << shift;

                if (shift > cntBits)
                    throw new ArgumentOutOfRangeException("bytes", "Byte array too large.");

                if ((byteValue & 0x80) != 0x80)
                    return result;

                shift += 7;
            }
            throw new ArgumentException("Decoding varint from byte array failed.");
        }
        #endregion

        #region Internal ZigZag Encoding/Decoding helpers
        private static long EncodeZigZag(long value, int bitLength)
        {
            return value << 1 ^ value >> bitLength - 1;
        }

        private static long DecodeZigZag(ulong value)
        {
            if ((value & 0x1) == 0x1)
            {
                return -1 * ((long) (value >> 1) + 1);
            }

            return (long) (value >> 1);
        }
        #endregion

    }
}