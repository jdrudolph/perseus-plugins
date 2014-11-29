using System;
using System.Windows;
using System.Windows.Controls;
using BaseLib.Wpf;
using BaseLibS.Util;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Manual{
	/// <summary>
	/// Interaction logic for SelectRowsManuallyControl1.xaml
	/// </summary>
	public partial class SelectRowsManuallyControl1{
		private readonly IMatrixData mdata;
		private readonly Action<IData> createNewMatrix;

		public SelectRowsManuallyControl1(IMatrixData mdata, Action<IData> createNewMatrix){
			InitializeComponent();
			this.mdata = mdata;
			this.createNewMatrix = createNewMatrix;
			TableView.TableModel = new MatrixDataTable(mdata);
			RemoveSelectedRowsButton.Content = new Image { Source = WpfUtils.LoadBitmap(Properties.Resources.hand) };
			KeepSelectedRowsButton.Content = new Image { Source = WpfUtils.LoadBitmap(Properties.Resources.hand) };
		}

		private void RemoveSelectedRowsButton_OnClick(object sender, RoutedEventArgs e){
			int[] sel = TableView.GetSelectedRows();
			if (sel.Length == 0){
				MessageBox.Show("Please select some rows.");
			}
			IMatrixData mx = (IMatrixData) mdata.Clone();
			mx.ExtractRows(ArrayUtils.Complement(sel, TableView.RowCount));
			createNewMatrix(mx);
		}

		private void KeepSelectedRowsButton_OnClick(object sender, RoutedEventArgs e){
			int[] sel = TableView.GetSelectedRows();
			if (sel.Length == 0){
				MessageBox.Show("Please select some rows.");
			}
			IMatrixData mx = (IMatrixData) mdata.Clone();
			mx.ExtractRows(sel);
			createNewMatrix(mx);
		}
	}
}