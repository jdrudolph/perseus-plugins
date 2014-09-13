using System;
using System.Windows.Forms;
using BaseLibS.Util;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Manual{
	public partial class SelectRowsManuallyControl : UserControl{
		private readonly IMatrixData mdata;
		private readonly Action<IData> createNewMatrix;

		public SelectRowsManuallyControl(IMatrixData mdata, Action<IData> createNewMatrix){
			InitializeComponent();
			this.mdata = mdata;
			this.createNewMatrix = createNewMatrix;
			matrixDataGridView.TableModel = new MatrixDataTable(mdata);
		}

		private void RemoveSelectedRowsButtonClick(object sender, EventArgs e){
			int[] sel = matrixDataGridView.GetSelectedRows();
			if (sel.Length == 0){
				MessageBox.Show("Please select some rows.");
			}
			IMatrixData mx = (IMatrixData)mdata.Clone();
			mx.ExtractExpressionRows(ArrayUtils.Complement(sel, matrixDataGridView.RowCount));
			createNewMatrix(mx);
		}

		private void KeepSelectedRowsButtonClick(object sender, EventArgs e){
			int[] sel = matrixDataGridView.GetSelectedRows();
			if (sel.Length == 0){
				MessageBox.Show("Please select some rows.");
			}
			IMatrixData mx = (IMatrixData)mdata.Clone();
			mx.ExtractExpressionRows(sel);
			createNewMatrix(mx);
		}
	}
}