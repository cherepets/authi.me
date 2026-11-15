using AotServiceProvider;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Authi.Common.Services
{
    [Service]
    public interface IBinarySerializer
    {
        byte[]? Serialize<T>(T? value);
        T? Deserialize<T>(byte[]? bytes) where T : class;
        T? DeserializeValue<T>(byte[]? bytes) where T : struct;
    }

    public class BinarySerializer : IBinarySerializer
    {
        public byte[]? Serialize<T>(T? value)
        {
            if (value is null)
            {
                return null;
            }

            return value switch
            {
                string str => Encoding.UTF8.GetBytes(str),
                bool b => [b ? (byte)1 : (byte)0],
                byte b => [b],
                sbyte sb => [(byte)sb],
                short s => BitConverter.GetBytes(s),
                ushort us => BitConverter.GetBytes(us),
                int i => BitConverter.GetBytes(i),
                uint ui => BitConverter.GetBytes(ui),
                long l => BitConverter.GetBytes(l),
                ulong ul => BitConverter.GetBytes(ul),
                float f => BitConverter.GetBytes(f),
                double d => BitConverter.GetBytes(d),
                char c => BitConverter.GetBytes(c),
                Guid g => g.ToByteArray(),
                bool[] arr => arr.Select(b => b ? (byte)1 : (byte)0).ToArray(),
                byte[] arr => arr,
                short[] arr => MemoryMarshal.AsBytes<short>(arr).ToArray(),
                ushort[] arr => MemoryMarshal.AsBytes<ushort>(arr).ToArray(),
                int[] arr => MemoryMarshal.AsBytes<int>(arr).ToArray(),
                uint[] arr => MemoryMarshal.AsBytes<uint>(arr).ToArray(),
                long[] arr => MemoryMarshal.AsBytes<long>(arr).ToArray(),
                ulong[] arr => MemoryMarshal.AsBytes<ulong>(arr).ToArray(),
                float[] arr => MemoryMarshal.AsBytes<float>(arr).ToArray(),
                double[] arr => MemoryMarshal.AsBytes<double>(arr).ToArray(),
                char[] arr => Encoding.UTF8.GetBytes(arr),
                Guid[] arr => MemoryMarshal.AsBytes<Guid>(arr).ToArray(),
                _ => throw new NotSupportedException($"Unsupported type: {typeof(T)}")
            };
        }

        public T? Deserialize<T>(byte[]? bytes) where T : class
        {
            if (bytes == null)
            {
                return null;
            }

            object converted = typeof(T) switch
            {
                var t when t == typeof(string) => Encoding.UTF8.GetString(bytes),
                var t when t == typeof(bool[]) => bytes.Select(b => b != 0).ToArray(),
                var t when t == typeof(byte[]) => bytes.ToArray(),
                var t when t == typeof(short[]) => MemoryMarshal.Cast<byte, short>(bytes).ToArray(),
                var t when t == typeof(ushort[]) => MemoryMarshal.Cast<byte, ushort>(bytes).ToArray(),
                var t when t == typeof(int[]) => MemoryMarshal.Cast<byte, int>(bytes).ToArray(),
                var t when t == typeof(uint[]) => MemoryMarshal.Cast<byte, uint>(bytes).ToArray(),
                var t when t == typeof(long[]) => MemoryMarshal.Cast<byte, long>(bytes).ToArray(),
                var t when t == typeof(ulong[]) => MemoryMarshal.Cast<byte, ulong>(bytes).ToArray(),
                var t when t == typeof(float[]) => MemoryMarshal.Cast<byte, float>(bytes).ToArray(),
                var t when t == typeof(double[]) => MemoryMarshal.Cast<byte, double>(bytes).ToArray(),
                var t when t == typeof(char[]) => Encoding.UTF8.GetChars(bytes),
                var t when t == typeof(Guid[]) => MemoryMarshal.Cast<byte, Guid>(bytes).ToArray(),
                _ => throw new NotSupportedException($"Unsupported type: {typeof(T)}")
            };

            return (T?)converted;
        }

        public T? DeserializeValue<T>(byte[]? bytes) where T : struct
        {
            if (bytes == null)
            {
                return null;
            }

            object converted = typeof(T) switch
            {
                var t when t == typeof(bool) => bytes[0] != 0,
                var t when t == typeof(byte) => bytes[0],
                var t when t == typeof(sbyte) => (sbyte)bytes[0],
                var t when t == typeof(short) => BitConverter.ToInt16(bytes),
                var t when t == typeof(ushort) => BitConverter.ToUInt16(bytes),
                var t when t == typeof(int) => BitConverter.ToInt32(bytes),
                var t when t == typeof(uint) => BitConverter.ToUInt32(bytes),
                var t when t == typeof(long) => BitConverter.ToInt64(bytes),
                var t when t == typeof(ulong) => BitConverter.ToUInt64(bytes),
                var t when t == typeof(float) => BitConverter.ToSingle(bytes),
                var t when t == typeof(double) => BitConverter.ToDouble(bytes),
                var t when t == typeof(char) => BitConverter.ToChar(bytes),
                var t when t == typeof(Guid) => new Guid(bytes),
                _ => throw new NotSupportedException($"Unsupported type: {typeof(T)}")
            };

            return (T?)converted;
        }
    }
}
