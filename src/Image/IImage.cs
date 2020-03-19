using System;
using System.Collections;
using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace ImageCore
{
    public interface IImage : ISubImage, IViewable, ICloneable, IEquatable<IImage>, ISerializable, IEnumerable
    {
        int Height { get; }
        int Width { get; }

        IImage Transpose();

        IImage Rotate(RotationDegree degree);
        IImage Flip(FlipDirection direction);

        IImage Clamp(double low, double high);

        IImage Add(IImage other);
        IImage Subtract(IImage other);

        ISubImage Slice(IImmutableList<(int I, int J)> pixels);
        ISubImage Slice(Func<double, bool> selector);
        ISubImage Slice(Func<int, int, double, bool> selector);
        ISubImage Slice(Range horizontal, Range vertical);
    }
}