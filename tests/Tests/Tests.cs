using System;
using Image;
using NUnit.Framework;

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
    }
}
