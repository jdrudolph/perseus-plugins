using System.Collections.Generic;

namespace PerseusApi.Generic{
	public interface IDataWithAnnotationColumns{
		/// <summary>
		///     For performance reasons, please do not call this inside a loop when iterating over the elements.
		///     Use <code>GetCategoryColumnEntryAt</code> instead.
		/// </summary>
		string[][] GetCategoryColumnAt(int index);

		string[] GetCategoryColumnEntryAt(int index, int row);
		string[] GetCategoryColumnValuesAt(int index);
		void SetCategoryColumnAt(string[][] vals, int index);

		void SetAnnotationColumns(List<string> stringColumnNames, List<string[]> stringColumns,
			List<string> categoryColumnNames, List<string[][]> categoryColumns, List<string> numericColumnNames,
			List<double[]> numericColumns, List<string> multiNumericColumnNames, List<double[][]> multiNumericColumns);

		void SetAnnotationColumns(List<string> stringColumnNames, List<string> stringColumnDescriptions,
			List<string[]> stringColumns, List<string> categoryColumnNames, List<string> categoryColumnDescriptions,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<string> numericColumnDescriptions,
			List<double[]> numericColumns, List<string> multiNumericColumnNames, List<string> multiNumericColumnDescriptions,
			List<double[][]> multiNumericColumns);

		void ClearAnnotationColumns();
		void ClearCategoryColumns();
		void ClearStringColumns();
		void ClearNumericColumns();
		void ClearMultiNumericColumns();
		void AddCategoryColumn(string name, string description, string[][] vals);
		void AddStringColumn(string name, string description, string[] vals);
		void AddNumericColumn(string name, string description, double[] vals);
		void AddMultiNumericColumn(string name, string description, double[][] vals);
		void RemoveCategoryColumnAt(int index);
		void RemoveStringColumnAt(int index);
		void RemoveNumericColumnAt(int index);
		void RemoveMultiNumericColumnAt(int index);
		List<string[][]> CategoryColumns { set; }
		List<double[]> NumericColumns { get; set; }
		List<string[]> StringColumns { get; set; }
		List<double[][]> MultiNumericColumns { get; set; }
		int CategoryColumnCount { get; }
		int StringColumnCount { get; }
		int NumericColumnCount { get; }
		int MultiNumericColumnCount { get; }
		List<string> CategoryColumnNames { get; set; }
		List<string> StringColumnNames { get; set; }
		List<string> NumericColumnNames { get; set; }
		List<string> MultiNumericColumnNames { get; set; }
		List<string> CategoryColumnDescriptions { get; set; }
		List<string> StringColumnDescriptions { get; set; }
		List<string> NumericColumnDescriptions { get; set; }
		List<string> MultiNumericColumnDescriptions { get; set; }
		int RowCount { get; }
		void ExtractRows(int[] indices);

	}
}