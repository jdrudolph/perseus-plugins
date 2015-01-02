using System;
using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLibS.Param;
using BaseLibS.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Utils;

namespace PerseusPluginLib.Filter{
	public class FilterValidValuesColumns : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string Name { get { return "Filter columns based on valid values"; } }
		public string Heading { get { return "Filter columns"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 3; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }

		public string Url{
			get{
				return
					"http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Filtercolumns:FilterValidValuesColumns";
			}
		}

		public int GetMaxThreads(Parameters parameters) { return 1; }

		public string Description{
			get{
				return
					"Rows/columns of the expression matrix are filtered to contain at least the specified numbers of entries that are " +
						"valid in the specified way.";
			}
		}

		public string HelpOutput{
			get{
				return
					"The matrix of expression values is constrained to contain only these rows/columns that fulfill the requirement.";
			}
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			const bool rows = false;
			int minValids = param.GetParam<int>("Min. number of values").Value;
			ParameterWithSubParams<int> modeParam = param.GetParamWithSubParams<int>("Mode");
			int modeInd = modeParam.Value;
			if (modeInd != 0 && mdata.CategoryRowNames.Count == 0){
				processInfo.ErrString = "No grouping is defined.";
				return;
			}
			if (modeInd != 0){
				processInfo.ErrString = "Group-wise filtering can only be appled to rows.";
				return;
			}
			ParameterWithSubParams<int> x = param.GetParamWithSubParams<int>("Values should be");
			Parameters subParams = x.GetSubParameters();
			int shouldBeIndex = x.Value;
			FilteringMode filterMode;
			double threshold = double.NaN;
			double threshold2 = double.NaN;
			switch (shouldBeIndex){
				case 0:
					filterMode = FilteringMode.Valid;
					break;
				case 1:
					filterMode = FilteringMode.GreaterThan;
					threshold = subParams.GetParam<double>("Minimum").Value;
					break;
				case 2:
					filterMode = FilteringMode.GreaterEqualThan;
					threshold = subParams.GetParam<double>("Minimum").Value;
					break;
				case 3:
					filterMode = FilteringMode.LessThan;
					threshold = subParams.GetParam<double>("Maximum").Value;
					break;
				case 4:
					filterMode = FilteringMode.LessEqualThan;
					threshold = subParams.GetParam<double>("Maximum").Value;
					break;
				case 5:
					filterMode = FilteringMode.Between;
					threshold = subParams.GetParam<double>("Minimum").Value;
					threshold2 = subParams.GetParam<double>("Maximum").Value;
					break;
				case 6:
					filterMode = FilteringMode.Outside;
					threshold = subParams.GetParam<double>("Minimum").Value;
					threshold2 = subParams.GetParam<double>("Maximum").Value;
					break;
				default:
					throw new Exception("Should not happen.");
			}
			if (modeInd != 0){
				int gind = modeParam.GetSubParameters().GetParam<int>("Grouping").Value;
				string[][] groupCol = mdata.GetCategoryRowAt(gind);
				NonzeroFilterGroup(minValids, mdata, param, modeInd == 2, threshold, threshold2, filterMode, groupCol);
			} else{
				NonzeroFilter1(rows, minValids, mdata, param, threshold, threshold2, filterMode);
			}
		}

		private static void NonzeroFilterGroup(int minValids, IMatrixData mdata, Parameters param, bool oneGroup,
			double threshold, double threshold2, FilteringMode filterMode, IList<string[]> groupCol){
			List<int> valids = new List<int>();
			string[] groupVals = ArrayUtils.UniqueValuesPreserveOrder(groupCol);
			Array.Sort(groupVals);
			int[][] groupInds = CalcGroupInds(groupVals, groupCol);
			for (int i = 0; i < mdata.RowCount; i++){
				int[] counts = new int[groupVals.Length];
				for (int j = 0; j < mdata.ColumnCount; j++){
					if (Valid(mdata.Values[i, j], threshold, threshold2, filterMode)){
						for (int k = 0; k < groupInds[j].Length; k++){
							if (groupInds[j][k] >= 0){
								counts[groupInds[j][k]]++;
							}
						}
					}
				}
				if ((oneGroup ? ArrayUtils.Max(counts) : ArrayUtils.Min(counts)) >= minValids){
					valids.Add(i);
				}
			}
			PerseusPluginUtils.FilterRows(mdata, param, valids.ToArray());
		}

		internal static int[][] CalcGroupInds(string[] groupVals, IList<string[]> groupCol){
			int[][] result = new int[groupCol.Count][];
			for (int i = 0; i < result.Length; i++){
				result[i] = new int[groupCol[i].Length];
				for (int j = 0; j < result[i].Length; j++){
					result[i][j] = Array.BinarySearch(groupVals, groupCol[i][j]);
				}
			}
			return result;
		}

		private static void NonzeroFilter1(bool rows, int minValids, IMatrixData mdata, Parameters param, double threshold,
			double threshold2, FilteringMode filterMode){
			if (rows){
				List<int> valids = new List<int>();
				for (int i = 0; i < mdata.RowCount; i++){
					int count = 0;
					for (int j = 0; j < mdata.ColumnCount; j++){
						if (Valid(mdata.Values[i, j], threshold, threshold2, filterMode)){
							count++;
						}
					}
					if (count >= minValids){
						valids.Add(i);
					}
				}
				PerseusPluginUtils.FilterRows(mdata, param, valids.ToArray());
			} else{
				List<int> valids = new List<int>();
				for (int j = 0; j < mdata.ColumnCount; j++){
					int count = 0;
					for (int i = 0; i < mdata.RowCount; i++){
						if (Valid(mdata.Values[i, j], threshold, threshold2, filterMode)){
							count++;
						}
					}
					if (count >= minValids){
						valids.Add(j);
					}
				}
				PerseusPluginUtils.FilterColumns(mdata, param, valids.ToArray());
			}
		}

		private static bool Valid(double data, double threshold, double threshold2, FilteringMode filterMode){
			switch (filterMode){
				case FilteringMode.Valid:
					return !double.IsNaN(data) && !double.IsNaN(data);
				case FilteringMode.GreaterThan:
					return data > threshold;
				case FilteringMode.GreaterEqualThan:
					return data >= threshold;
				case FilteringMode.LessThan:
					return data < threshold;
				case FilteringMode.LessEqualThan:
					return data <= threshold;
				case FilteringMode.Between:
					return data >= threshold && data <= threshold2;
				case FilteringMode.Outside:
					return data < threshold || data > threshold2;
			}
			throw new Exception("Never get here.");
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			Parameters[] subParams = new Parameters[1];
			subParams[0] = new Parameters(new Parameter[0]);
			return
				new Parameters(new Parameter[]{
					new IntParam("Min. number of values", 3){
						Help = "If a row/column has less than the specified number of valid values it will be discarded in the output."
					},
					new SingleChoiceWithSubParams("Mode"){Values = new[]{"In total"}, SubParams = subParams},
					new SingleChoiceWithSubParams("Values should be"){
						Values = new[]{"Valid", "Greater than", "Greater or equal", "Less than", "Less or equal", "Between", "Outside"},
						SubParams =
							new[]{
								new Parameters(new Parameter[0]),
								new Parameters(new Parameter[]
								{new DoubleParam("Minimum", 0){Help = "Value defining which entry is counted as a valid value."}}),
								new Parameters(new Parameter[]
								{new DoubleParam("Minimum", 0){Help = "Value defining which entry is counted as a valid value."}}),
								new Parameters(new Parameter[]
								{new DoubleParam("Maximum", 0){Help = "Value defining which entry is counted as a valid value."}}),
								new Parameters(new Parameter[]
								{new DoubleParam("Maximum", 0){Help = "Value defining which entry is counted as a valid value."}}),
								new Parameters(new Parameter[]{
									new DoubleParam("Minimum", 0){Help = "Value defining which entry is counted as a valid value."},
									new DoubleParam("Maximum", 0){Help = "Value defining which entry is counted as a valid value."}
								}),
								new Parameters(new Parameter[]{
									new DoubleParam("Minimum", 0){Help = "Value defining which entry is counted as a valid value."},
									new DoubleParam("Maximum", 0){Help = "Value defining which entry is counted as a valid value."}
								})
							}
					},
					PerseusPluginUtils.GetFilterModeParam(true)
				});
		}
	}
}