using System;

namespace Image
{
    public interface ISubImage<T> : ISubImage 
        where T : unmanaged, IComparable<T>
    {
        T this[long i] { get; }
        new T Max();
        new T Min();

        T Percentile(T lvl);
    }
}