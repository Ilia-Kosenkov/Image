using System;
using Image;
using NUnit.Framework;
using Image.Internal;

namespace Tests
{
    [TestFixture]
    public class Tests
    {
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
        public void TestIL()
        {
            var xx = Numerics.DangerousAdd(123, 250);
            var yy = Numerics.DangerousAdd(123.0, 250.0);

            var comp1 = Numerics.Compare(1.00, 200.0);
            var comp2 = Numerics.Compare(1230, 250);
            var comp3 = Numerics.Compare(1.00, 1.0);
            var comp4 = Numerics.Compare(200u, 200u);



        }
    }
}
