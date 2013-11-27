using BaseLib.Util;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Utils{
	public class ColumnNameInfo : INameInfo{
		public bool CutNames { get; set; }
		public bool CutNames2 { get; set; }
		public int NameColumnIndex { get; set; }
		public int Name2ColumnIndex { get; set; }
		private readonly IMatrixData mdata;

		public ColumnNameInfo(IMatrixData mdata){
			this.mdata = mdata;
		}

		public string[] GetRowNames(){
			string[] result = new string[mdata.ExpressionColumnCount];
			for (int i = 0; i < result.Length; i++){
				result[i] = GetRowName(i);
			}
			return result;
		}

		public string[] GetNameSelection(){
			return ArrayUtils.Concat(new[]{"Name"}, ArrayUtils.Concat(mdata.CategoryRowNames, mdata.NumericRowNames));
		}

		public string GetRowName(int ind, int nameColumnIndex, bool cutNames){
			if (ind < 0){
				return "";
			}
			if (nameColumnIndex < 0){
				return "";
			}
			if (ind >= mdata.ExpressionColumnNames.Count){
				return mdata.NumericColumnNames[ind - mdata.ExpressionColumnNames.Count];
			}
			if (nameColumnIndex == 0){
				return mdata.ExpressionColumnNames[ind];
			}
			int indw = nameColumnIndex - 1;
			if (indw < mdata.CategoryRowCount){
				string[][] x = mdata.GetCategoryRowAt(indw);
				if (ind >= 0 && ind < x.Length){
					string[] w = x[ind];
					if (cutNames){
						return w.Length > 0 ? w[0] : "";
					}
					return StringUtils.Concat(";", w);
				}
			}
			int indi = indw - mdata.CategoryRowCount;
			if (indi < mdata.NumericRowCount && indi >= 0){
				double[] q = mdata.NumericRows[indi];
				if (ind >= 0 && ind < q.Length){
					return "" + q[ind];
				}
			}
			return "";
		}

		public string GetRowName(int ind){
			return GetRowName(ind, NameColumnIndex, CutNames);
		}

		public string GetRowName2(int ind){
			return GetRowName(ind, Name2ColumnIndex, CutNames2);
		}

		public string GetRowDescription(int ind){
			return GetRowName(ind, Name2ColumnIndex, CutNames2);
		}
	}
}