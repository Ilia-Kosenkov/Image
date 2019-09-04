using System;

namespace ImageCore.Internals
{
    internal interface IHashGenerator
    {
        uint Compute<T>(ReadOnlySpan<T> data) where T : unmanaged;
    }
}