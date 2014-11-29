using System.Collections.Generic;
using PerseusApi.Generic;

namespace PerseusApi.Matrix{
	/// <summary>
	/// The data structure representing an augmented data matrix which is the main data object that is flowing through
	/// the Perseus workflow. Note that plugin programmers are not supposed to write implementations of <code>IMatrixData</code>.
	/// The interface only serves to encapsulate the complexity of the implementation for the purpose of plugin programming.
	/// </summary>
	public interface IMatrixData : IDataWithAnnotationRows{
		int RowCount { get; }
		float[,] Values { get; set; }
		float[,] QualityValues { get; set; }
		bool[,] IsImputed { get; set; }
		string QualityName { get; set; }
		bool QualityBiggerIsBetter { get; set; }
		bool HasQuality { get; }
		float[] GetRow(int row);
		float[] GetColumn(int col);
		float[] GetQualityRow(int row);
		float[] GetQualityColumn(int col);
		bool[] GetIsImputednRow(int row);
		bool[] GetIsImputedColumn(int col);
		float this[int i, int j] { get; set; }
		int CategoryColumnCount { get; }
		List<string> CategoryColumnNames { get; set; }
		List<string> CategoryColumnDescriptions { get; set; }
		List<string[][]> CategoryColumns { set; }

		/// <summary>
		/// For performance reasons, please do not call this inside a loop when iterating over the elements. 
		/// Use <code>GetCategoryColumnEntryAt</code> instead.
		/// </summary>
		string[][] GetCategoryColumnAt(int index);

		string[] GetCategoryColumnEntryAt(int index, int row);
		string[] GetCategoryColumnValuesAt(int index);
		void SetCategoryColumnAt(string[][] vals, int index);
		void RemoveCategoryColumnAt(int index);
		void AddCategoryColumn(string name, string description, string[][] vals);
		void AddCategoryColumns(IList<string> names, IList<string> descriptions, IList<string[][]> vals);
		void ClearCategoryColumns();
		int NumericColumnCount { get; }
		List<string> NumericColumnNames { get; set; }
		List<string> NumericColumnDescriptions { get; set; }
		List<double[]> NumericColumns { get; set; }
		void AddNumericColumn(string name, string description, double[] vals);
		void RemoveNumericColumnAt(int index);
		int StringColumnCount { get; }
		List<string> StringColumnNames { get; set; }
		List<string> StringColumnDescriptions { get; set; }
		List<string[]> StringColumns { get; set; }
		void AddStringColumn(string name, string description, string[] vals);
		void RemoveStringColumnAt(int index);
		int MultiNumericColumnCount { get; }
		List<string> MultiNumericColumnNames { get; set; }
		List<string> MultiNumericColumnDescriptions { get; set; }
		List<double[][]> MultiNumericColumns { get; set; }
		void AddMultiNumericColumn(string name, string description, double[][] vals);
		void RemoveMultiNumericColumnAt(int index);
		void ExtractRows(int[] indices);

		void SetData(string name, List<string> columnNames, float[,] values,
			List<string> stringColumnNames, List<string[]> stringColumns, List<string> categoryColumnNames,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<double[]> numericColumns,
			List<string> multiNumericColumnNames, List<double[][]> multiNumericColumns);

		void SetData(string name, List<string> columnNames, float[,] values, bool[,] isImputed,
			List<string> stringColumnNames, List<string[]> stringColumns, List<string> categoryColumnNames,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<double[]> numericColumns,
			List<string> multiNumericColumnNames, List<double[][]> multiNumericColumns);

		void SetData(string name, List<string> columnNames, float[,] values,
			List<string> stringColumnNames, List<string[]> stringColumns, List<string> categoryColumnNames,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<double[]> numericColumns,
			List<string> multiNumericColumnNames, List<double[][]> multiNumericColumns, List<string> categoryRowNames,
			List<string[][]> categoryRows, List<string> numericRowNames, List<double[]> numericRows);

		void SetData(string name, List<string> columnNames, float[,] values, bool[,] isImputed,
			List<string> stringColumnNames, List<string[]> stringColumns, List<string> categoryColumnNames,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<double[]> numericColumns,
			List<string> multiNumericColumnNames, List<double[][]> multiNumericColumns, List<string> categoryRowNames,
			List<string[][]> categoryRows, List<string> numericRowNames, List<double[]> numericRows);

		void SetData(string name, string description, List<string> columnNames,
			List<string> columnDescriptions, float[,] values, bool[,] isImputed, float[,] qualityValues,
			string qualityName, bool qualityBiggerIsBetter, List<string> stringColumnNames, List<string> stringColumnDescriptions,
			List<string[]> stringColumns, List<string> categoryColumnNames, List<string> categoryColumnDescriptions,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<string> numericColumnDescriptions,
			List<double[]> numericColumns, List<string> multiNumericColumnNames, List<string> multiNumericColumnDescriptions,
			List<double[][]> multiNumericColumns, List<string> categoryRowNames, List<string> categoryRowDescriptions,
			List<string[][]> categoryRows, List<string> numericRowNames, List<string> numericRowDescriptions,
			List<double[]> numericRows);

		void ClearAnnotationColumns();
		void ClearNumericColumns();
		void ClearMultiNumericColumns();
		void ClearStringColumns();
	}
}