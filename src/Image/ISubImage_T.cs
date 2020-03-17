#nullable  enable
using System;
using static Internal.UnsafeNumerics.MathOps;

namespace ImageCore
{
    public interface ISubImage<T> : ISubImage 
        where T : unmanaged, IComparable<T>
    {
        T this[long i] { get; }
        new T Max();
        new T Min();

        T Percentile(T lvl);
        new T Median();

        new T Var();
        new T Average();


        #region ISubImage

        double ISubImage.Min() => DangerousCast<T, double>(Min());
        double ISubImage.Max() => DangerousCast<T, double>(Max());
        double ISubImage.Percentile(double lvl) => DangerousCast<T, double>(Percentile(DangerousCast<double, T>(lvl)));
        double ISubImage.Median() => DangerousCast<T, double>(Median());
        double ISubImage.Var() => DangerousCast<T, double>(Var());
        double ISubImage.Average() => DangerousCast<T, double>(Average());

        #endregion
    }
}