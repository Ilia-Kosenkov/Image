using System;

namespace Image
{
    public interface IImage<T> : IEquatable<IImage<T>>, ISubImage<T>, IViewable<T>, IImage
        where T : unmanaged, IComparable<T>
    {
        T this[int i, int j] { get; }
        IImage<T> Copy();
        IImage<T> Transpose();

        IImage<TOther> CastTo<TOther>() 
            where TOther : unmanaged, IComparable<TOther>;

        IImage<TOther> CastTo<TOther>(Func<T, TOther> caster) 
            where TOther : unmanaged, IComparable<TOther>;

        IImage<T> Clamp(T low, T high);
        IImage<T> Scale(T low, T high);
        IImage<T> AddScalar(T item);
        IImage<T> MultiplyBy(T item);
        IImage<T> DivideBy(T item);
        IImage<T> Add(IImage<T> other);
        IImage<T> Subtract(IImage<T> other);
    }
}