using PerseusApi.Generic;

namespace PerseusApi.Matrix{
	/// <summary>
	///     The data structure representing an augmented data matrix which is the main data object that is flowing through
	///     the Perseus workflow. Note that plugin programmers are not supposed to write implementations of <code>IMatrixData</code>.
	///     The interface only serves to encapsulate the complexity of the implementation for the purpose of plugin programming.
	/// </summary>
	public interface IMatrixData : IDataWithAnnotationRows, IDataWithAnnotationColumns{
		float[,] Values { get; set; }
		float[,] QualityValues { get; set; }
		bool[,] IsImputed { get; set; }
		string QualityName { get; set; }
		bool QualityBiggerIsBetter { get; set; }
		bool HasQuality { get; }
		float this[int i, int j] { get; set; }
		float[] GetRow(int row);
		float[] GetColumn(int col);
		float[] GetQualityRow(int row);
		float[] GetQualityColumn(int col);
		bool[] GetIsImputednRow(int row);
		bool[] GetIsImputedColumn(int col);
	}
}