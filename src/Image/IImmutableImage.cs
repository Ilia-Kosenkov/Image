using System;

namespace Image
{
    public interface IImmutableImage : ICloneable, IEquatable<IImmutableImage>
    {
        int Height { get; }
        int Width { get; }
        ReadOnlySpan<byte> GetByteView();

        double Min();
        double Max();
        double Percentile(double lvl);
        IImmutableImage Clamp(double low, double high);
    }

    public interface IImmutableImage<T> : 
        IEquatable<IImmutableImage<T>>, IImmutableImage 
        where T : unmanaged, IComparable<T>
    {
        T this[int i, int j] { get; }

        new T Max();
        new T Min();

        T Percentile(T lvl);

        ReadOnlySpan<T> GetView();

        IImmutableImage<T> Copy();
        IImmutableImage<T> Transpose();
        IImmutableImage<TOther> CastTo<TOther>() 
            where TOther : unmanaged, IComparable<TOther>;
        IImmutableImage<TOther> CastTo<TOther>(Func<T, TOther> caster) 
            where TOther : unmanaged, IComparable<TOther>;

        IImmutableImage<T> Clamp(T low, T high);
        IImmutableImage<T> Scale(T low, T high);

        IImmutableImage<T> AddScalar(T item);
        IImmutableImage<T> MultiplyBy(T item);
        IImmutableImage<T> DivideBy(T item);
    }
}
