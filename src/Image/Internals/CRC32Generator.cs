#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ImageCore.Internals
{
    // ReSharper disable once InconsistentNaming
    internal sealed class CRC32Generator : IHashGenerator
    {
        private static IHashGenerator Instance { get; }
        private readonly uint[] _table;


        [MethodImpl(MethodImplOptions.Synchronized)]
        static CRC32Generator()
        {
            Instance = new CRC32Generator();
        }

        private CRC32Generator()
        {
            _table = new uint[256];
            FillTable();
        }

        // From https://github.com/NTDLS/NSWFL/blob/4d74039697a10a722319d88b7b450622c49ef629/NSWFL_CRC32.Cpp#L51
        private void FillTable()
        {
            const uint key = 0x04C11DB7;

            for (var i = 0; i < _table.Length; i++)
            {
                var value = Reflect((uint) i, 8) << 24;
                for (var j = 0; j < 8; j++)
                    value = (value << 1) ^ ((value & (1 << 31)) != 0 ? key : 0);

                _table[i] = Reflect(value, 32);
            }
        }

        private uint InternalCompute(uint start, ReadOnlySpan<byte> data)
        {
            var result = start;
            foreach (var t in data)
                result = _table[(result ^ t) & 0xFF] ^ (result >> 8);

            return result;
        }
        
        public uint Compute<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            var byteView = MemoryMarshal.Cast<T, byte>(data);
            var hash = InternalCompute(0xFFFFFFFF, byteView);
            return unchecked(~hash * 31 + (uint)typeof(T).GetHashCode());
        }


        // From https://github.com/NTDLS/NSWFL/blob/4d74039697a10a722319d88b7b450622c49ef629/NSWFL_CRC32.Cpp#L79
        private static uint Reflect(uint value, byte size)
        {
            var result = 0u;

            for (var i = 1; i < size + 1; i++)
            {
                if ((value & 1) != 0)
                    result |= 1u << (size - i);
                value >>= 1;
            }

            return result;
        }

        public static uint ComputeHash<T>(ReadOnlySpan<T> data)
            where T : unmanaged
            => Instance.Compute(data);
    }
}
