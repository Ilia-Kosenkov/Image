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
    }

    public interface IImmutableImage<T> : 
        IEquatable<IImmutableImage<T>>, IImmutableImage 
        where T : unmanaged, IComparable<T>
    {
        T this[int i, int j] { get; }

        T Max();
        T Min();

        double Percentile(T lvl);

        IImmutableImage<T> Copy();
        IImmutableImage<T> Transpose();
        IImmutableImage<TOther> CastTo<TOther>() 
            where TOther : unmanaged, IComparable<TOther>;
        IImmutableImage<TOther> CastTo<TOther>(Func<T, TOther> caster) 
            where TOther : unmanaged, IComparable<TOther>;

        IImmutableImage<T> Clamp(T low, T high);
        IImmutableImage<T> Scale(T low, T high);

    }
}
