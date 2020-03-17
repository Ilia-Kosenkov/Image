#nullable enable

using System;
using System.Collections.Generic;

using static Internal.UnsafeNumerics.MathOps;

namespace ImageCore
{
    public interface IImage<T> : IEquatable<IImage<T>>, ISubImage<T>, IViewable<T>, IEnumerable<T>, IImage
        where T : unmanaged, IComparable<T>, IEquatable<T>
    {
        T this[int i, int j] { get; }
        T this[Index i, Index j] { get; }

        ref readonly T DangerousGet(long pos);

        IImage<T> Copy();
        new IImage<T> Transpose();
        new IImage<T> Rotate(RotationDegree degree);
        new IImage<T> Flip(FlipDirection direction);

        IImage<TOther> CastTo<TOther>() 
            where TOther : unmanaged, IComparable<TOther>, IEquatable<TOther>;

        IImage<TOther> CastTo<TOther>(Func<T, TOther> caster) 
            where TOther : unmanaged, IComparable<TOther>, IEquatable<TOther>;

        IImage<T> Clamp(T low, T high);
        IImage<T> Scale(T low, T high);
        IImage<T> AddScalar(T item);
        IImage<T> MultiplyBy(T item);
        IImage<T> DivideBy(T item);
        IImage<T> Add(IImage<T> other);
        IImage<T> Subtract(IImage<T> other);

        new ISubImage<T> Slice(ICollection<(int I, int J)> indexes);
        ISubImage<T> Slice(Func<T, bool> selector);
        ISubImage<T> Slice(Func<int, int, T, bool> selector);
        new ISubImage<T> Slice(Range horizontal, Range vertical);

        #region IImage

        IImage IImage.Transpose() => Transpose();
        IImage IImage.Rotate(RotationDegree degree) => Rotate(degree);
        IImage IImage.Flip(FlipDirection direction) => Flip(direction);
        IImage IImage.Clamp(double low, double high) => Clamp(DangerousCast<double, T>(low), DangerousCast<double, T>(high));

        IImage IImage.Add(IImage other)
            => other is IImage<T> img
                ? Add(img)
                : throw new ArgumentException(nameof(other));

        IImage IImage.Subtract(IImage other)
            => other is IImage<T> img
                ? Subtract(img)
                : throw new ArgumentException(nameof(other));

        ISubImage IImage.Slice(Range horizontal, Range vertical) => Slice(horizontal, vertical);

        ISubImage IImage.Slice(ICollection<(int I, int J)> pixels)
            => Slice(pixels);
        ISubImage IImage.Slice(Func<double, bool> selector)
        {
            bool Func(T x) => selector(DangerousCast<T, double>(x));
            return Slice(Func);
        }
        ISubImage IImage.Slice(Func<int, int, double, bool> selector)
        {
            bool Func(int i, int j, T x) => selector(i, j, DangerousCast<T, double>(x));
            return Slice(Func);
        }

     

        #endregion
    }
}