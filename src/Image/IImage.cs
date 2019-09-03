using System;
using System.Runtime.Serialization;

namespace Image
{
    public interface IImage : ISubImage, IViewable, ICloneable, IEquatable<IImage>, ISerializable
    {
        int Height { get; }
        int Width { get; }

        IImage Clamp(double low, double high);

        IImage Add(IImage other);
        IImage Subtract(IImage other);

    }
}