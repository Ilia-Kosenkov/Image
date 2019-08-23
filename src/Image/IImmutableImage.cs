using System;

namespace Image
{
    public interface IImmutableImage<T> : IEquatable<OldImage>, ICloneable where T : unmanaged
    {
        int Height { get; }
        int Width { get; }
        T this[int i, int j] { get; }

        T Max();
        T Min();

        double Percentile(T lvl);

        IImmutableImage<T> Copy();
        IImmutableImage<T> Transpose();
        IImmutableImage<TOther> CastTo<TOther>() where TOther : unmanaged;
        IImmutableImage<TOther> CastTo<TOther>(Func<T, TOther> caster) where TOther : unmanaged;

        IImmutableImage<T> Clamp(T low, T high);
        IImmutableImage<T> Scale(T low, T high);

    }
}
