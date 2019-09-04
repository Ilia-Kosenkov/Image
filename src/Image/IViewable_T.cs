using System;

namespace ImageCore
{
    public interface IViewable<T> where T : unmanaged, IComparable<T>
    {
        ReadOnlySpan<T> GetView();
    }
}