using System.Collections.Generic;

namespace PerseusApi.Generic{
	public interface IDataWithAnnotationRows : IData{
		int ColumnCount { get; }
		List<string> ColumnNames { get; set; }
		List<string> ColumnDescriptions { get; set; }
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
		void ExtractColumns(int[] indices);

		void SetAnnotationRows(List<string> categoryRowNames, List<string> categoryRowDescriptions,
			List<string[][]> categoryRows, List<string> numericRowNames, List<string> numericRowDescriptions,
			List<double[]> numericRows);

		void SetAnnotationRows(List<string> categoryRowNames, List<string[][]> categoryRows, List<string> numericRowNames,
			List<double[]> numericRows);

		void ClearAnnotationRows();
	}
}