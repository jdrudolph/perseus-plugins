using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Norm{
	public class Subtract : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public BitmapSource DisplayImage { get { return null; } }
		public string HelpDescription { get { return "The specified quantity calculated on each row/column is subtracted from each value."; } }
		public string HelpOutput { get { return "Normalized expression matrix."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Subtract"; } }
		public string Heading { get { return "Normalization"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -6; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters) {
			return int.MaxValue;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
				SingleChoiceWithSubParams access = param.GetSingleChoiceWithSubParams("Matrix access");
			bool rows = access.Value == 0;
			int groupInd;
			if (rows){
				groupInd = access.GetSubParameters().GetSingleChoiceParam("Grouping").Value - 1;
			} else{
				groupInd = -1;
			}
			int what = param.GetSingleChoiceParam("Subtract what").Value;
			if (groupInd < 0){
				SubtractValues(rows, GetFunc(what), mdata, processInfo.NumThreads);
			} else{
				string[][] catRow = mdata.GetCategoryRowAt(groupInd);
				foreach (string[] t in catRow){
					if (t.Length > 1){
						processInfo.ErrString = "The groups are overlapping.";
						return;
					}
				}
				SubtractGroups(mdata, catRow, GetFunc(what));
			}
		}

		private static void SubtractGroups(IMatrixData mdata, IList<string[]> catRow, Func<double[], double> func){
			string[] groupVals = ArrayUtils.UniqueValuesPreserveOrder(catRow);
			foreach (int[] inds in groupVals.Select(groupVal => ZScore.GetIndices(catRow, groupVal))){
				SubtractGroup(mdata, inds, func);
			}
		}

		private static void SubtractGroup(IMatrixData data, IList<int> inds, Func<double[], double> func){
			for (int i = 0; i < data.RowCount; i++){
				double[] vals = new double[inds.Count];
				for (int j = 0; j < inds.Count; j++){
					double q = data[i, inds[j]];
					vals[j] = q;
				}
				double mean = func(vals);
				foreach (int t in inds){
					data[i, t] = (float) ((data[i, t] - mean));
				}
			}
		}

		private static Func<double[], double> GetFunc(int what){
			switch (what){
				case 0:
					return ArrayUtils.Mean;
				case 1:
					return ArrayUtils.Median;
				case 2:
					return ArrayUtils.MostFrequentValue;
				case 3:
					return ArrayUtils.TukeyBiweight;
				default:
					throw new Exception("Never get here.");
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			return
				new Parameters(new Parameter[]{
					new SingleChoiceWithSubParams("Matrix access"){
						Values = new[]{"Rows", "Columns"}, ParamNameWidth = 136, TotalWidth = 731,
						SubParams =
							new[]{
								new Parameters(new SingleChoiceParam("Grouping")
								{Values = ArrayUtils.Concat(new[]{"<No grouping>"}, mdata.CategoryRowNames)}),
								new Parameters()
							},
						Help = "Specifies if the subtraction is performed on the rows or the columns of the matrix."
					},
					new SingleChoiceParam("Subtract what"){
						Values = new[]{
							"Mean", "Median", "Most frequent value", "Tukey's biweight"
						},
						Value = 1
					}
				});
		}

		public static void SubtractValues(bool rows, Func<double[], double> summarize, IMatrixData data, int nthreads){
			if (rows){
				new ThreadDistributor(nthreads, data.RowCount, i => Calc1(i, summarize, data)).Start();
			} else{
				new ThreadDistributor(nthreads, data.ExpressionColumnCount, j => Calc2(j, summarize, data)).Start();
			}
		}

		private static void Calc1(int i, Func<double[], double> summarize, IMatrixData data){
			List<double> vals = new List<double>();
			for (int j = 0; j < data.ExpressionColumnCount; j++){
				double q = data[i, j];
				if (!double.IsNaN(q) && !double.IsInfinity(q)){
					vals.Add(q);
				}
			}
			double med = summarize(vals.ToArray());
			for (int j = 0; j < data.ExpressionColumnCount; j++){
				data[i, j] -= (float) med;
			}
		}

		private static void Calc2(int j, Func<double[], double> summarize, IMatrixData data){
			List<double> vals = new List<double>();
			for (int i = 0; i < data.RowCount; i++){
				double q = data[i, j];
				if (!double.IsNaN(q) && !double.IsInfinity(q)){
					vals.Add(q);
				}
			}
			double med = summarize(vals.ToArray());
			for (int i = 0; i < data.RowCount; i++){
				data[i, j] -= (float) med;
			}
		}
	}
}