using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib;
using PerseusPluginLib.Rearrange;
using BaseLib.Param;
using PerseusLib.Data.Matrix;

namespace PerseusPluginLibTest.Rearrange
{
    /// <summary>
    /// Testing the ProcessTextColumns class requires, at a minimum, a regular expression 
    /// and MatrixData for it to act on. The private method TestRegex encapsulates nost
    /// of the mechanics, so that the test methods only have to specify the regex, the 
    /// input data, and the expected output.
    /// </summary>
    [TestClass]
    public class ProcessTextColumnsTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            string name = null;
            List<string> expressionColumnNames = null;
            float[,] expressionValues = null;
            List<string> categoryColumnNames = null;
            List<string[][]> categoryColumns = null;
            List<string> numericColumnNames = null;
            List<double[]> numericColumns = null;
            List<string> multiNumericColumnNames = null;
            List<double[][]> multiNumericColumns = null;
            IMatrixData[] supplTables = null;
            IDocumentData[] documents = null;
            ProcessInfo processInfo = null;

            List<string> stringColumnNames = new List<string>
                {
                    "Name of Column 1",
                    "Name of Column 2",
                    "Name of Column 3"
                };
            List<string[]> stringColumnsInit = new List<string[]>
                {
                    new string[] { "col1, row1", "col1, row2" },
                    new string[] { "col2, row1", "col2, row2" },
                    new string[] { "col3; row1", "col3, row2" }
                };
            List<string[]> stringColumnsExpect = new List<string[]>
                {
                    new string[] { "col1, row1", "col1, row2" },
                    new string[] { "col2, row1", "col2, row2" },
                    new string[] { "col3", "col3, row2" }
                };

            Parameters param = new Parameters(new Parameter[]
                {
                    new MultiChoiceParam("Columns", Enumerable.Range(0, stringColumnNames.Count).ToArray())
                        {Values = stringColumnNames},
                    new StringParam("Regular expression", "^([^;]+)"),
                    new BoolParam("Keep original columns", false)
                });

            IMatrixData mdata = new MatrixData();
            mdata.Clear();
            mdata.SetData(name,
                          mdata.ExpressionColumnNames, mdata.ExpressionValues,
                          stringColumnNames, stringColumnsInit,
                          mdata.CategoryColumnNames, new List<string[][]>(),
                          mdata.NumericColumnNames, mdata.NumericColumns,
                          mdata.MultiNumericColumnNames, mdata.MultiNumericColumns);

            var ptc = new ProcessTextColumns();
            ptc.ProcessData(mdata, param, ref supplTables,
                            ref documents, processInfo);

            Boolean ignoreCase = false;
            for (int colInd = 0; colInd < stringColumnsInit.Count; colInd++)
            {
                for (int rowInd = 0; rowInd < stringColumnsInit[colInd].Length; rowInd++)
                {
                    String errMsg = "For column " + colInd + " and row " + rowInd + ", result was '" +
                                    mdata.StringColumns[colInd][rowInd] + "', but expected was '" +
                                    stringColumnsExpect[colInd][rowInd] + "'.";
                    Assert.AreEqual(mdata.StringColumns[colInd][rowInd], stringColumnsExpect[colInd][rowInd],
                        ignoreCase, errMsg);
                }
            }

        }
    }
}