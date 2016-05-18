using System;
using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerseusApi.Utils;

namespace PerseusApi.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string[] baseNames;
            string[] files;
            var x = 12;

            PerseusUtils.GetAvailableAnnots(out baseNames, out files);

            Assert.Fail();
        }
    }
}
