using System.Collections.Generic;
using System.Drawing;
using BaseLibS.Num;
using BaseLibS.Param;
using Calc;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;
using PerseusPluginLib.Utils;

namespace PerseusPluginLib.Filter{
	public class FilterNumericalColumn : IMatrixProcessing{
		public bool HasButton => false;
		public Bitmap DisplayImage => null;
		public string HelpOutput => "The filtered matrix.";
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string Name => "Filter rows based on numerical/main column";
		public string Heading => "Filter rows";
		public bool IsActive => true;
		public float DisplayRank => 1;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;
		public string Url
			=> "http://coxdocs.org/doku.php?id=perseus:user:activities:MatrixProcessing:Filterrows:FilterNumericalColumn";
		public string Description
			=>
				"Only those rows are kept that have a value in the numerical column fulfilling the " +
				"equation or inequality relation.";

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			string[] selection = ArrayUtils.Concat(mdata.NumericColumnNames, mdata.ColumnNames);
			return
				new Parameters(ArrayUtils.Concat(PerseusUtils.GetNumFilterParams(selection),
					PerseusPluginUtils.GetFilterModeParam(true)));
		}

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			string[] realVariableNames;
			int[] colInds = GetColInds(param, out realVariableNames);
			if (colInds == null || colInds.Length == 0){
				processInfo.ErrString = "Please specify at least one column.";
				return;
			}
			Relation[] relations = PerseusUtils.GetRelations(param, realVariableNames);
			foreach (Relation relation in relations){
				if (relation == null){
					processInfo.ErrString = "Could not parse relations";
					return;
				}
			}
			double[][] cols = new double[colInds.Length][];
			for (int i = 0; i < cols.Length; i++){
				cols[i] = colInds[i] < mdata.NumericColumnCount
					? mdata.NumericColumns[colInds[i]]
					: ArrayUtils.ToDoubles(mdata.Values.GetColumn(colInds[i] - mdata.NumericColumnCount));
			}
			bool and = param.GetParam<int>("Combine through").Value == 0;
			List<int> valids = new List<int>();
			for (int i = 0; i < cols[0].Length; i++){
				Dictionary<int, double> vars = new Dictionary<int, double>();
				for (int j = 0; j < cols.Length; j++){
					vars.Add(j, cols[j][i]);
				}
				bool[] results = new bool[relations.Length];
				for (int j = 0; j < relations.Length; j++){
					results[j] = relations[j].NumEvaluateDouble(vars);
				}
				bool valid = and ? ArrayUtils.And(results) : ArrayUtils.Or(results);
				if (valid){
					valids.Add(i);
				}
			}
			PerseusPluginUtils.FilterRows(mdata, param, valids.ToArray());
		}

		private static int[] GetColInds(Parameters parameters, out string[] realVariableNames){
			ParameterWithSubParams<int> sp = parameters.GetParamWithSubParams<int>("Number of columns");
			int ncols = sp.Value + 1;
			int[] result = new int[ncols];
			realVariableNames = new string[ncols];
			Parameters param = sp.GetSubParameters();
			for (int j = 0; j < ncols; j++){
				realVariableNames[j] = PerseusUtils.GetVariableName(j);
				result[j] = param.GetParam<int>(realVariableNames[j]).Value;
			}
			return result;
		}
	}
}