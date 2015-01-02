namespace PerseusApi.Matrix{
	public interface IMatrixIndexer{
		void Init(int nrows, int ncols);
		int RowCount { get; }
		int ColumnCount { get; }
		float this[int i, int j] { get; set; }
		void Transpose();
		void Set(float[,] value);
		float[,] Get();
		float[] GetRow(int row);
		float[] GetColumn(int col);
		bool IsInitialized();
		void ExtractRows(int[] rows);
		void ExtractColumns(int[] columns);
	}
}