using System;

namespace ImageCore
{
    public interface IViewable<T> : IViewable
        where T : unmanaged, IComparable<T>
    {
        ReadOnlySpan<T> GetView();
    }
}