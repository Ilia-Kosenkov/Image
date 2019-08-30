using System;
using System.Buffers.Text;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Image;
using NUnit.Framework;
using Internal.Numerics;
using MathNet.Numerics.Distributions;

namespace Tests
{
    [TestFixture]
    public class Tests
    {
        private Random R;

        [SetUp]
        public void SetUp() => R = new Random();

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
                arr[i] = R.Next();


            var img = new Image<int>(arr, 80, 50);

            Assert.AreEqual(arr.Min(), img.Min());
            Assert.AreEqual(arr.Max(), img.Max());
            Assert.AreEqual(arr.Min(), ((IImmutableImage) img).Min(), 1e-16);
            Assert.AreEqual(arr.Max(), ((IImmutableImage)img).Max(), 1e-16);

        }

        [Test]
        public void TestClamp()
        {
            var arr = new int[40_000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = R.Next();


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
                arr[i] = R.Next(-100_000, 100_000);


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
                arr[i] = R.Next(-100_000, 100_000);


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
                arr[i] = R.Next(-100_000, 100_000);


            var img = new Image<double>(arr, 200, 200);

            Assume.That(img.Equals(Image<double>.Zero(img.Height, img.Width)), Is.False);

            var newIm = img.Subtract(img);

            Assert.True(newIm.Equals(Image<double>.Zero(newIm.Height, newIm.Width)));

            var newIm2 = newIm.Add(img);

            Assert.True(newIm2.Equals(img));

        }

        [Test]
        public void TestSerialize()
        {
            var arr = new double[40_000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = R.Next(-100_000, 100_000);


            var img = new Image<double>(arr, 200, 200);

            using var mem = new MemoryStream();
            var f = new BinaryFormatter();
            f.Serialize(mem, img);
            mem.Position = 0;
            var img2 = f.Deserialize(mem) as Image<double>;
            Assert.True(img.Equals(img2));
        }

        [Test]
        public void TestIL()
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
    }
}
