using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.VirtualMachine.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Test
{
    [TestClass]
    public class FreeListTest
    {

        [TestMethod]
        public void TestMerge()
        {
            var list = new FreeList();
            list.Add((IntPtr)0, 8);
            list.Add((IntPtr)8, 8);
            list.Add((IntPtr)400, 8);

            list.Merge();

            Assert.AreEqual(2, list.Count());

            Assert.AreEqual((IntPtr)0, list[0].Position);
            Assert.AreEqual(16, list[0].Size);

            Assert.AreEqual((IntPtr)400, list[1].Position);
            Assert.AreEqual(8, list[1].Size);
        }
    }
}
