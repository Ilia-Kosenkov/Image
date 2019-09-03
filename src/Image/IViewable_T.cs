using System;

namespace Image
{
    public interface IViewable<T> where T : unmanaged, IComparable<T>
    {
        ReadOnlySpan<T> GetView();
    }
}