using System;

namespace Image
{
    public interface IViewable
    {
        ReadOnlySpan<byte> GetByteView();
        bool BitwiseEquals(IImage other);
    }
}