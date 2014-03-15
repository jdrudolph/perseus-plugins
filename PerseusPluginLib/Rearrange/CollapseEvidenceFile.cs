using System;
using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Rearrange{
	public class CollapseEvidenceFile : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string Description { get { return ""; } }
		public string Name { get { return "Collapse MaxQuant evidence file"; } }
		public string Heading { get { return "Rearrange"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 123; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public string HelpOutput { get { return ""; } }

		public int GetMaxThreads(Parameters parameters){
			return int.MaxValue;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			int chargeInd = 0;
			int experimentInd = 0;
			for (int i = 0; i < mdata.CategoryColumnCount; i++){
				if (mdata.CategoryColumnNames[i].ToLower().Equals("charge")){
					chargeInd = i;
				}
				if (mdata.CategoryColumnNames[i].ToLower().Equals("experiment")){
					experimentInd = i;
				}
			}
			int sequenceInd = 0;
			for (int i = 0; i < mdata.StringColumnCount; i++){
				if (mdata.StringColumnNames[i].ToLower().Equals("sequence")){
					sequenceInd = i;
					break;
				}
			}
			string[] intensityChoice = ArrayUtils.Concat(mdata.ExpressionColumnNames, mdata.NumericColumnNames);
			int intensityInd = 0;
			for (int i = 0; i < intensityChoice.Length; i++){
				if (intensityChoice[i].ToLower().Equals("intensity")){
					intensityInd = i;
					break;
				}
			}
			string[] proteinChoice = ArrayUtils.Concat(mdata.MultiNumericColumnNames, mdata.StringColumnNames);
			int proteinInd = 0;
			for (int i = 0; i < proteinChoice.Length; i++){
				if (proteinChoice[i].ToLower().Equals("protein group ids")){
					proteinInd = i;
					break;
				}
			}
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Charge", chargeInd){Values = mdata.CategoryColumnNames},
					new SingleChoiceParam("Peptide sequence", sequenceInd){Values = mdata.StringColumnNames},
					new SingleChoiceParam("Experiment", experimentInd){Values = mdata.CategoryColumnNames},
					new SingleChoiceParam("Intensity", intensityInd){Values = intensityChoice},
					new SingleChoiceParam("Protein IDs", proteinInd){Values = proteinChoice}
				});
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int[] charge = GetCharges(mdata, param);
			if (charge == null){
				processInfo.ErrString = "Please specify a 'Charge' column.";
				return;
			}
			int expColInd = param.GetSingleChoiceParam("Experiment").Value;
			if (expColInd < 0){
				processInfo.ErrString = "Please specify an 'Experiment' column.";
				return;
			}
			string[] experiment = GetExperiments(mdata, expColInd);
			int pepSeqInd = param.GetSingleChoiceParam("Peptide sequence").Value;
			if (pepSeqInd < 0){
				processInfo.ErrString = "Please specify a 'Peptide sequence' column.";
				return;
			}
			string[] peptideSequences = mdata.StringColumns[pepSeqInd];
			double[] intensities = GetIntensities(mdata, param);
			if (intensities == null){
				processInfo.ErrString = "Please specify an 'Intensity' column.";
				return;
			}
			string[][] proteinIds = GetProteinIds(mdata, param);
			if (proteinIds == null){
				processInfo.ErrString = "Please specify a 'Protein IDs' column.";
				return;
			}
			List<int> valids = new List<int>();
			for (int i = 0; i < charge.Length; i++){
				if (IsValid(charge[i], experiment[i], peptideSequences[i], intensities[i], proteinIds[i])){
					valids.Add(i);
				}
			}
			charge = ArrayUtils.SubArray(charge, valids);
			experiment = ArrayUtils.SubArray(experiment, valids);
			peptideSequences = ArrayUtils.SubArray(peptideSequences, valids);
			intensities = ArrayUtils.SubArray(intensities, valids);
			proteinIds = ArrayUtils.SubArray(proteinIds, valids);
			string[] allExperiments = ArrayUtils.UniqueValues(experiment);
			int rowCount = 0;
			Dictionary<Tuple<int, string>, int> map = new Dictionary<Tuple<int, string>, int>();
			for (int i = 0; i < charge.Length; i++){
				Tuple<int, string> t = new Tuple<int, string>(charge[i], peptideSequences[i]);
				if (!map.ContainsKey(t)){
					map.Add(t, rowCount);
					rowCount++;
				}
			}
			float[,] intens = new float[rowCount,allExperiments.Length];
			HashSet<string>[] protNames = new HashSet<string>[rowCount];
			for (int i = 0; i < rowCount; i++){
				protNames[i] = new HashSet<string>();
			}
			string[] newPeprides = new string[rowCount];
			string[][] newCharges = new string[rowCount][];
			for (int i = 0; i < charge.Length; i++){
				int rowInd = map[new Tuple<int, string>(charge[i], peptideSequences[i])];
				int colInd = Array.BinarySearch(allExperiments, experiment[i]);
				intens[rowInd, colInd] += (float) intensities[i];
				foreach (string proteinId in proteinIds[i]){
					protNames[rowInd].Add(proteinId);
				}
				newPeprides[rowInd] = peptideSequences[i];
				newCharges[rowInd] = new[] { "" + charge[i] };
			}
			string[] newProteins = new string[rowCount];
			for (int i = 0; i < newProteins.Length; i++){
				newProteins[i] = StringUtils.Concat(";", ArrayUtils.ToArray(protNames[i]));
			}
			bool[,] isImputed = new bool[intens.GetLength(0),intens.GetLength(1)];
			float[,] quality = new float[intens.GetLength(0),intens.GetLength(1)];
			mdata.Clear();
			mdata.SetData("newMatrix", "", new List<string>(allExperiments), new List<string>(allExperiments), intens, isImputed,
				quality, "Q", true, new List<string>(new[]{"Peptides", "Proteins"}), new List<string>(new[]{"", ""}),
				new List<string[]>(new[]{newPeprides, newProteins}), new List<string>(new[]{"Charge"}),
				new List<string>(new[]{""}), new List<string[][]>(new[]{newCharges}), new List<string>(), new List<string>(),
				new List<double[]>(), new List<string>(), new List<string>(), new List<double[][]>(), new List<string>(),
				new List<string>(), new List<string[][]>(), new List<string>(), new List<string>(), new List<double[]>());
		}

		private static bool IsValid(int charge, string experiment, string peptideSequence, double intensity,
			ICollection<string> proteinId){
			if (charge < 1 || charge == int.MaxValue){
				return false;
			}
			if (string.IsNullOrEmpty(experiment)){
				return false;
			}
			if (string.IsNullOrEmpty(peptideSequence)){
				return false;
			}
			if (double.IsNaN(intensity) || double.IsInfinity(intensity) || intensity <= 0){
				return false;
			}
			return proteinId != null && proteinId.Count != 0;
		}

		private static string[][] GetProteinIds(IMatrixData mdata, Parameters param){
			int colInd = param.GetSingleChoiceParam("Protein IDs").Value;
			if (colInd < 0){
				return null;
			}
			if (colInd < mdata.MultiNumericColumnCount){
				double[][] col = mdata.MultiNumericColumns[colInd];
				string[][] result = new string[col.Length][];
				for (int i = 0; i < result.Length; i++){
					result[i] = GetProteins(col[i]);
				}
				return result;
			}
			colInd -= mdata.MultiNumericColumnCount;
			string[] col1 = mdata.StringColumns[colInd];
			string[][] result1 = new string[col1.Length][];
			for (int i = 0; i < result1.Length; i++){
				result1[i] = GetProteins(col1[i]);
			}
			return result1;
		}

		private static string[] GetProteins(string s){
			s = s.Trim();
			return s.Length == 0 ? new string[0] : s.Split(';');
		}

		private static string[] GetProteins(IList<double> doubles){
			string[] result = new string[doubles.Count];
			for (int i = 0; i < result.Length; i++){
				result[i] = "" + (int) Math.Round(doubles[i]);
			}
			return result;
		}

		private static double[] GetIntensities(IMatrixData mdata, Parameters param){
			int colInd = param.GetSingleChoiceParam("Intensity").Value;
			if (colInd < 0){
				return null;
			}
			if (colInd < mdata.ExpressionColumnCount){
				return ArrayUtils.ToDoubles(mdata.GetExpressionColumn(colInd));
			}
			colInd -= mdata.ExpressionColumnCount;
			return mdata.NumericColumns[colInd];
		}

		private static string[] GetExperiments(IMatrixData mdata, int expColInd){
			string[][] col = mdata.GetCategoryColumnAt(expColInd);
			string[] result = new string[col.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = col[i].Length > 0 ? col[i][0] : null;
			}
			return result;
		}

		private static int[] GetCharges(IMatrixData mdata, Parameters param){
			int colInd = param.GetSingleChoiceParam("Charge").Value;
			if (colInd < 0){
				return null;
			}
			string[][] col = mdata.GetCategoryColumnAt(colInd);
			int[] result = new int[col.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = GetCharge(col[i]);
			}
			return result;
		}

		private static int GetCharge(IList<string> s){
			if (s.Count == 0){
				return 0;
			}
			int c;
			return int.TryParse(s[0], out c) ? c : 0;
		}
	}
}