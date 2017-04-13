using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestDiceRoller
{
    [TestClass]
    public class TestAST
    {
        private Action<byte[]> GetRNG(IEnumerator<uint> values)
        {
            void GetRandomBytes(byte[] arr)
            {
                values.MoveNext();
                BitConverter.GetBytes(values.Current).CopyTo(arr, 0);
            }

            return GetRandomBytes;
        }

        private IEnumerator<uint> Roll9()
        {
            while (true)
            {
                yield return 9;
            }
        }

        [TestMethod]
        public void TestBasicRoll()
        {

        }
    }
}
