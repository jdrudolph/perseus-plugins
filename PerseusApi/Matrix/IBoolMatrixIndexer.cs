namespace PerseusApi.Matrix{
	public interface IBoolMatrixIndexer{
		void Init(int nrows, int ncols);
		int RowCount { get; }
		int ColumnCount { get; }
		bool this[int i, int j] { get; set; }
		void Transpose();
		void Set(bool[,] value);
		bool[,] Get();
		bool[] GetRow(int row);
		bool[] GetColumn(int col);
		bool IsInitialized();
		void ExtractRows(int[] rows);
		void ExtractColumns(int[] columns);
	}
}