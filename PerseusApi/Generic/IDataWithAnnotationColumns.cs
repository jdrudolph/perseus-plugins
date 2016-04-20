using System.Collections.Generic;

namespace PerseusApi.Generic{
	public interface IDataWithAnnotationColumns{
		void CopyAnnotationColumnsFrom(IDataWithAnnotationColumns other);
		void CopyAnnotationColumnsFromRows(IDataWithAnnotationRows other);
		int RowCount { get; }
		/// <summary>
		///     For performance reasons, please do not call this inside a loop when iterating over the elements.
		///     Use <code>GetCategoryColumnEntryAt</code> instead.
		/// </summary>
		string[][] GetCategoryColumnAt(int index);

		string[] GetCategoryColumnEntryAt(int index, int row);
		string[] GetCategoryColumnValuesAt(int index);
		void SetCategoryColumnAt(string[][] vals, int index);
		void RemoveCategoryColumnAt(int index);
		void ClearCategoryColumns();
		void AddCategoryColumn(string name, string description, string[][] vals);
		int CategoryColumnCount { get; }
		List<string> CategoryColumnNames { get; set; }
		List<string> CategoryColumnDescriptions { get; set; }
		List<string[][]> CategoryColumns { set; }

		List<double[]> NumericColumns { get; set; }
		int NumericColumnCount { get; }
		void ClearNumericColumns();
		void AddNumericColumn(string name, string description, double[] vals);
		void RemoveNumericColumnAt(int index);
		List<string> NumericColumnNames { get; set; }
		List<string> NumericColumnDescriptions { get; set; }
	
		void ClearStringColumns();
		void AddStringColumn(string name, string description, string[] vals);
		void RemoveStringColumnAt(int index);
		List<string[]> StringColumns { get; set; }
		int StringColumnCount { get; }
		List<string> StringColumnNames { get; set; }
		List<string> StringColumnDescriptions { get; set; }

		void ClearMultiNumericColumns();
		void AddMultiNumericColumn(string name, string description, double[][] vals);
		void RemoveMultiNumericColumnAt(int index);
		List<double[][]> MultiNumericColumns { get; set; }
		int MultiNumericColumnCount { get; }
		List<string> MultiNumericColumnNames { get; set; }
		List<string> MultiNumericColumnDescriptions { get; set; }

		void ExtractRows(int[] indices);
		void SetAnnotationColumns(List<string> stringColumnNames, List<string[]> stringColumns,
			List<string> categoryColumnNames, List<string[][]> categoryColumns, List<string> numericColumnNames,
			List<double[]> numericColumns, List<string> multiNumericColumnNames, List<double[][]> multiNumericColumns);
		void SetAnnotationColumns(List<string> stringColumnNames, List<string> stringColumnDescriptions,
			List<string[]> stringColumns, List<string> categoryColumnNames, List<string> categoryColumnDescriptions,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<string> numericColumnDescriptions,
			List<double[]> numericColumns, List<string> multiNumericColumnNames, List<string> multiNumericColumnDescriptions,
			List<double[][]> multiNumericColumns);
		void ClearAnnotationColumns();

        void Clone(IDataWithAnnotationColumns clone);
	}
}