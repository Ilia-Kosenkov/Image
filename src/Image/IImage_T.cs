﻿using System;
using System.Collections.Generic;

namespace Image
{
    public interface IImage<T> : IEquatable<IImage<T>>, ISubImage<T>, IViewable<T>, IEnumerable<T>, IImage
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

        new ISubImage<T> Slice(ICollection<(int I, int J)> indexes);
        ISubImage<T> Slice(Func<T, bool> selector);
        ISubImage<T> Slice(Func<int, int, T, bool> selector);
    }
}