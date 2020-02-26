using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using ImageCore;
using NUnit.Framework;
using Internal.UnsafeNumerics;

namespace Tests
{
    [TestFixture]
    public class Tests
    {
        private Random _r;

        [SetUp]
        public void SetUp() => _r = new Random();

        [Test]
        public void TestInit()
        {
            var x = new[] {1.0, 123141, 12333, -149719};
            var bytes = new byte[x.Length * sizeof(double)];
            Buffer.BlockCopy(x, 0, bytes, 0, bytes.Length);

            var imageFromBytes = new Image<double>(bytes, 1, x.Length);
            var imageFromDoubles = new Image<double>(x, 1, x.Length);

            Assert.IsTrue(imageFromBytes.Equals(imageFromDoubles));
        }


        [Test]
        public void TestMinMax()
        {
            var arr = new int[4_000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = _r.Next();


            var img = new Image<int>(arr, 80, 50);

            Assert.AreEqual(arr.Min(), img.Min());
            Assert.AreEqual(arr.Max(), img.Max());
            Assert.AreEqual(arr.Min(), ((ISubImage) img).Min(), 1e-16);
            Assert.AreEqual(arr.Max(), ((ISubImage)img).Max(), 1e-16);

        }

        [Test]
        public void TestClamp()
        {
            var arr = new int[40_000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = _r.Next();


            var img = new Image<int>(arr, 200, 200);

            var min = img.Min();
            var max = img.Max();

            var newMin = (int)(min + 0.1 * Math.Abs(min));
            var newMax = (int)(max - 0.1 * Math.Abs(max));

            var newIm = img.Clamp(newMin, newMax);

            Assert.False(img.Equals(newIm));
            Assert.AreEqual(newMin, newIm.Min());
            Assert.AreEqual(newMax, newIm.Max());
        }

        [Test]
        public void TestScale()
        {
            var arr = new double[40_000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = _r.Next(-100_000, 100_000);


            var img = new Image<double>(arr, 200, 200);

            var newIm = img.Scale(0, 1_000);

            Assert.False(img.Equals(newIm));
            Assert.AreEqual(0, newIm.Min());
            Assert.AreEqual(1_000, newIm.Max());
        }


        [Test]
        public void TestTranspose()
        {
            var arr = new double[40_000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = _r.Next(-100_000, 100_000);


            var img = new Image<double>(arr, 200, 200);

            var newIm = img.Transpose();

            Assert.False(img.Equals(newIm));
            Assert.AreEqual(img[10, 20], newIm[20, 10]);
            Assert.AreNotEqual(img[10, 20], newIm[10, 20]);
        }

        [Test]
        public void TestMath()
        {
            var arr = new double[40_000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = _r.Next(-100_000, 100_000);


            var img = new Image<double>(arr, 200, 200);
            
            Assume.That(img.Equals(Image<double>.Zero(img.Height, img.Width)), Is.False);

            var newIm = ((IImage) img).Subtract(img);

            Assert.True(newIm.Equals(Image<double>.Zero(newIm.Height, newIm.Width)));

            var newIm2 = newIm.Add(img);

            Assert.True(newIm2.Equals(img));

        }

        [Test]
        public void TestSerialize()
        {
            var img = Image.Create<double>(x =>
            {
                for (var i = 0; i < 40_000; i++)
                    x[i] = _r.Next(-100_000, 100_000);
            }, 100, 400);

            using var mem = new MemoryStream();
            var f = new BinaryFormatter();
            f.Serialize(mem, img);
            mem.Position = 0;
            var img2 = f.Deserialize(mem) as Image<double>;
            Assert.True(img.Equals(img2));
        }

        [Test]
        public void TestIl()
        {
            Assert.AreEqual(373, MathOps.DangerousAdd(123, 250));
            Assert.AreEqual(373.0, MathOps.DangerousAdd(123.0, 250.0));

            Assert.AreEqual(-1, MathOps.DangerousCompare(1.00, 200.0));
            Assert.AreEqual(1, MathOps.DangerousCompare(1230, 250));
            Assert.AreEqual(0, MathOps.DangerousCompare(1.00, 1.0));
            Assert.AreEqual(0, MathOps.DangerousCompare(200u, 200u));

            Assert.True(MathOps.DangerousGreaterEquals(200, 100));
            Assert.False(MathOps.DangerousLessEquals(200, 100));

            Assert.AreEqual(123, MathOps.DangerousCast<double, int>(123));
        }

        [Test]
        public void TestImageShape()
        {
            var arr = new double[40_000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = _r.Next(-100_000, 100_000);


            var img = new Image<double>(arr, 100, 400);

            Assert.AreEqual(100, img.Height);
            Assert.AreEqual(400, img.Width);
            Assert.AreEqual(arr.Length, img.Size);

            for(var i = 0; i < img.Height; i++)
            for (var j = 0; j < img.Width; j++)
                Assert.IsTrue(img[i, j].AlmostEqual(img[i * img.Width + j]) &&
                              img[i, j].AlmostEqual(arr[i * img.Width + j]));
        }

        [Test]
        public void TestCloning()
        {
            var img = Image.Create<double>(x =>
            {
                for (var i = 0; i < 40_000; i++)
                    x[i] = _r.Next(-100_000, 100_000);
            }, 100, 400);

            Assert.IsTrue(img.Equals(img.Clone()));
            Assert.AreEqual(img.GetHashCode(), img.Clone().GetHashCode());

        }

        [Test]
        public void TestEquals()
        {
            var img = Image.Create<double>(x =>
            {
                for (var i = 0; i < 40_000; i++)
                    x[i] = _r.Next(-100_000, 100_000);
            }, 100, 400);

            var other = img.Copy();

            Assert.IsTrue(img.Equals(other));
            Assert.IsTrue(img.Equals((IImage) other));
            Assert.IsTrue(img.Equals((object) other));
            Assert.IsTrue(img.BitwiseEquals(other));

            Assert.IsFalse(img.Equals((object) null));
            Assert.IsFalse(img.Equals((IImage)null));
            Assert.IsFalse(img.Equals(null));
            Assert.IsFalse(img.BitwiseEquals(null));

        }
        [Test]
        public void TestFullSlice()
        {
            var img = Image.Create<double>(x =>
            {
                for (var i = 0; i < 40_000; i++)
                    x[i] = _r.Next(-100_000, 100_000);
            }, 100, 400);

            var slice = img.Slice(x => true);

            Assert.AreEqual(img.Size, slice.Size);
            Assert.AreEqual(img.Max(), slice.Max());
            Assert.AreEqual(img.Min(), slice.Min());
            Assert.AreEqual(img.Percentile(0.66), slice.Percentile(0.66));
            Assert.AreEqual(img.Average(), slice.Average());
            Assert.AreEqual(img.Var(), slice.Var());

        }

        [Test]
        public void TestSlice()
        {
            var img = Image.Create<long>(x =>
            {
                for (var i = 0; i < 4_000; i++)
                    x[i] = _r.Next(-1_000, 1_000);
            }, 100, 40);

            var max = img.Max();
            var min = img.Min();

            var slice = img.Slice(x => x == max);
            
            Assert.AreEqual(img.Max(), slice.Max());
            Assert.AreEqual(slice.Max(), slice.Min());
            Assert.AreEqual(max, slice[0]);

            slice = img.Slice(x => x == min);
            Assert.AreEqual(img.Min(), slice.Min());
            Assert.AreEqual(slice.Max(), slice.Min());
            Assert.AreEqual(min, slice[0]);

            slice = img.Slice(x => x == min || x == max);
            Assert.AreEqual(img.Min(), slice.Min());
            Assert.AreEqual(img.Max(), slice.Max());
            Assert.AreEqual(((ISubImage)img).Min(), ((ISubImage)slice).Min());
            Assert.AreEqual(((ISubImage)img).Max(), ((ISubImage)slice).Max());

            slice = img.Slice((i, j, _) => (i + j) > 0);

            Assert.AreEqual(img.Average(), slice.Average(), 5);
            Assert.AreEqual(
                img.Var(), 
                slice.Var(), 5e2);

        }

        [TestCase(0.25)]
        [TestCase(0.50)]
        [TestCase(0.75)]
        [Test]
        public void TestPercentile(double p)
        {
            var arr = new int[40_000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = _r.Next(-100_000, 100_000);


            var img = new Image<int>(arr, 100, 400);
            Assert.AreEqual(((ISubImage)img).Min(), ((ISubImage)img).Percentile(0));

            Assert.AreEqual(((ISubImage)img).Max(), ((ISubImage)img).Percentile(100));
            var med1 = img.Percentile((int)(100 * p));

            var med2 = arr.OrderBy(x => x).Skip((int) (p * img.Size) - 1).First();
            Assert.AreEqual(med2, med1);
            Assert.AreEqual(((ISubImage)img).Median(), ((ISubImage) img).Median());
        }

        [Test]
        public void TestCastTo()
        {
            var img = Image.Create<double>(x =>
            {
                for (var i = 0; i < 4_000; i++)
                    x[i] = _r.Next(-1_000, 1_000) + _r.NextDouble();
            }, 100, 40);

            Assert.That(() => img.CastTo<int>(), Throws.Nothing);
        }

        [Test]
        public void TestThrows()
        {
            Assert.That(() => Image.Create<double>(null, 1, 1), Throws.ArgumentNullException);
            Assert.That(() => Image.Create<char>((_) => { }, 1, 1), Throws.InstanceOf<NotSupportedException>());
            Assert.That(() => Image.Create<int>((_) => { }, -1, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => Image.Create<int>((_) => { }, 1, -1), Throws.InstanceOf<ArgumentOutOfRangeException>());

            Assert.That(() => new Image<char>(ReadOnlySpan<char>.Empty, 1, 1), Throws.InstanceOf<NotSupportedException>());
            Assert.That(() => new Image<char>(ReadOnlySpan<byte>.Empty, 1, 1), Throws.InstanceOf<NotSupportedException>());

            Assert.That(() => new Image<int>(ReadOnlySpan<int>.Empty, -1, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => new Image<int>(ReadOnlySpan<int>.Empty,  1, -1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => new Image<int>(ReadOnlySpan<int>.Empty, 1, 1), Throws.ArgumentException);


            Assert.That(() => new Image<int>(ReadOnlySpan<byte>.Empty, -1, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => new Image<int>(ReadOnlySpan<byte>.Empty, 1, -1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => new Image<int>(ReadOnlySpan<byte>.Empty, 1, 1), Throws.ArgumentException);

        }

        [Test]
        public void TestDangerousAccess()
        {
            var arr = new int[40_000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = _r.Next(-100_000, 100_000);

            var img = new Image<int>(arr, 400, 100);

            for (var i = 0; i < arr.Length; i++) 
                Assert.AreEqual(img[i], img.DangerousGet(i));
        }
    }
 
}
