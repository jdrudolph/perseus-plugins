using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerseusApi.Document;
using PerseusApi.Matrix;
using PerseusPluginLib.Rearrange;

namespace PerseusPluginLibTest.Rearrange
{
    [TestClass]
    public class RenameColumnsRegexpTest
    {
        [TestMethod]
        public void TestWikiExample()
        {
            var renamer = new RenameColumnsRegexp();
            var matrix = new Moq.Mock<IMatrixData>();
            var colnames = new List<string>() {"column 1", "column 2", "column 3"};
            matrix.Setup(m => m.ColumnCount).Returns(colnames.Count);
            matrix.Setup(m => m.ColumnNames).Returns(colnames);
            string err = "";
            var param = renamer.GetParameters(matrix.Object, ref err);
            param.GetParam<string>("Regular expression").Value = "column (.*)";

            IMatrixData[] supplTables = null;
            IDocumentData[] documents = null;
            renamer.ProcessData(matrix.Object, param, ref supplTables, ref documents, null);

            CollectionAssert.AreEqual(new List<string> {"1", "2", "3"}, colnames);
        }
    }
}