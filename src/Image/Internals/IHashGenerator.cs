using System;

namespace Image.Internals
{
    internal interface IHashGenerator
    {
        uint Compute<T>(ReadOnlySpan<T> data) where T : unmanaged;
    }
}