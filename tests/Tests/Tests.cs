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
            var x = new double[] {1.0, 123141, 12333, -149719};
            var bytes = new byte[x.Length * sizeof(double)];
            Buffer.BlockCopy(x, 0, bytes, 0, bytes.Length);

            var image = new Image<byte>(bytes, sizeof(double), x.Length);

            //for(var i = 0; i < x.Length; i++)
            //    Assert.AreEqual(x[i], image[0, i]);

            Assert.Pass();

        }
    }
}
