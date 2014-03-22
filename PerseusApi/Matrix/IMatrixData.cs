using System.Collections.Generic;
using PerseusApi.Generic;

namespace PerseusApi.Matrix{
	/// <summary>
	/// The data structure representing an augmented data matrix which is the main data object that is flowing through
	/// the Perseus workflow. Note that plugin programmers are not supposed to write implementations of <code>IMatrixData</code>.
	/// The interface only serves to encapsulate the complexity of the implementation for the purpose of plugin programming.
	/// </summary>
	public interface IMatrixData : IData{
		int RowCount { get; }
		float[,] ExpressionValues { get; set; }
		float[,] QualityValues { get; set; }
		string QualityName { get; set; }
		bool QualityBiggerIsBetter { get; set; }
		bool HasQuality { get; }
		bool[,] IsImputed { get; set; }
		int ExpressionColumnCount { get; }
		List<string> ExpressionColumnNames { get; set; }
		List<string> ExpressionColumnDescriptions { get; set; }
		float[] GetExpressionRow(int row);
		float[] GetExpressionColumn(int col);
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
		int CategoryRowCount { get; }
		List<string> CategoryRowNames { get; set; }
		List<string> CategoryRowDescriptions { get; set; }
		List<string[][]> CategoryRows { set; }

		/// <summary>
		/// For performance reasons, please do not call this inside a loop when iterating over the elements. 
		/// Use <code>GetCategoryRowEntryAt</code> instead.
		/// </summary>
		string[][] GetCategoryRowAt(int index);

		string[] GetCategoryRowEntryAt(int index, int column);
		string[] GetCategoryRowValuesAt(int index);
		void SetCategoryRowAt(string[][] vals, int index);
		void RemoveCategoryRowAt(int index);
		void AddCategoryRow(string name, string description, string[][] vals);
		void AddCategoryRows(IList<string> names, IList<string> descriptions, IList<string[][]> vals);
		void ClearCategoryRows();
		int NumericRowCount { get; }
		List<string> NumericRowNames { get; set; }
		List<string> NumericRowDescriptions { get; set; }
		List<double[]> NumericRows { get; set; }
		void AddNumericRow(string name, string description, double[] vals);
		void RemoveNumericRowAt(int index);
		void ClearNumericRows();
		void ExtractExpressionRows(int[] order);
		void ExtractExpressionColumns(int[] order);

		void SetData(string name, List<string> expressionColumnNames, float[,] expressionValues,
			List<string> stringColumnNames, List<string[]> stringColumns, List<string> categoryColumnNames,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<double[]> numericColumns,
			List<string> multiNumericColumnNames, List<double[][]> multiNumericColumns);

		void SetData(string name, List<string> expressionColumnNames, float[,] expressionValues, bool[,] isImputed,
			List<string> stringColumnNames, List<string[]> stringColumns, List<string> categoryColumnNames,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<double[]> numericColumns,
			List<string> multiNumericColumnNames, List<double[][]> multiNumericColumns);

		void SetData(string name, List<string> expressionColumnNames, float[,] expressionValues,
			List<string> stringColumnNames, List<string[]> stringColumns, List<string> categoryColumnNames,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<double[]> numericColumns,
			List<string> multiNumericColumnNames, List<double[][]> multiNumericColumns, List<string> categoryRowNames,
			List<string[][]> categoryRows, List<string> numericRowNames, List<double[]> numericRows);

		void SetData(string name, List<string> expressionColumnNames, float[,] expressionValues, bool[,] isImputed,
			List<string> stringColumnNames, List<string[]> stringColumns, List<string> categoryColumnNames,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<double[]> numericColumns,
			List<string> multiNumericColumnNames, List<double[][]> multiNumericColumns, List<string> categoryRowNames,
			List<string[][]> categoryRows, List<string> numericRowNames, List<double[]> numericRows);

		void SetData(string name, string description, List<string> expressionColumnNames,
			List<string> expressionColumnDescriptions, float[,] expressionValues, bool[,] isImputed, float[,] qualityValues,
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