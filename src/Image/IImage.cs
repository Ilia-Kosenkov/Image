using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ImageCore
{
    public interface IImage : ISubImage, IViewable, ICloneable, IEquatable<IImage>, ISerializable, IEnumerable
    {
        int Height { get; }
        int Width { get; }

        IImage Clamp(double low, double high);

        IImage Add(IImage other);
        IImage Subtract(IImage other);

        ISubImage Slice(ICollection<(int I, int J)> pixels);
        ISubImage Slice(Func<double, bool> selector);
        ISubImage Slice(Func<int, int, double, bool> selector);

        ISubImage Slice(Range horizontal, Range vertical);
    }
}