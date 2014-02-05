namespace PerseusPluginLib.Manual {
	partial class SelectRowsManuallyControl {
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectRowsManuallyControl));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.removeSelectedRowsButton = new System.Windows.Forms.ToolStripButton();
			this.keepSelectedRowsButton = new System.Windows.Forms.ToolStripButton();
			this.matrixDataGridView = new BaseLib.Forms.Table.TableView();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeSelectedRowsButton,
            this.keepSelectedRowsButton});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(339, 25);
			this.toolStrip1.TabIndex = 1;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// removeSelectedRowsButton
			// 
			this.removeSelectedRowsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.removeSelectedRowsButton.Image = ((System.Drawing.Image)(resources.GetObject("removeSelectedRowsButton.Image")));
			this.removeSelectedRowsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.removeSelectedRowsButton.Name = "removeSelectedRowsButton";
			this.removeSelectedRowsButton.Size = new System.Drawing.Size(23, 22);
			this.removeSelectedRowsButton.Text = "Remove selected rows";
			this.removeSelectedRowsButton.Click += new System.EventHandler(this.RemoveSelectedRowsButtonClick);
			// 
			// keepSelectedRowsButton
			// 
			this.keepSelectedRowsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.keepSelectedRowsButton.Image = ((System.Drawing.Image)(resources.GetObject("keepSelectedRowsButton.Image")));
			this.keepSelectedRowsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.keepSelectedRowsButton.Name = "keepSelectedRowsButton";
			this.keepSelectedRowsButton.Size = new System.Drawing.Size(23, 22);
			this.keepSelectedRowsButton.Text = "Keep selected rows";
			this.keepSelectedRowsButton.Click += new System.EventHandler(this.KeepSelectedRowsButtonClick);
			// 
			// matrixDataGridView
			// 
			this.matrixDataGridView.ColumnFooterHeight = 0;
			this.matrixDataGridView.ColumnHeaderHeight = 47;
			this.matrixDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.matrixDataGridView.HasFilter = false;
			this.matrixDataGridView.HasHelp = true;
			this.matrixDataGridView.Location = new System.Drawing.Point(0, 25);
			this.matrixDataGridView.Margin = new System.Windows.Forms.Padding(0);
			this.matrixDataGridView.MultiSelect = true;
			this.matrixDataGridView.Name = "matrixDataGridView";
			this.matrixDataGridView.RowFooterWidth = 0;
			this.matrixDataGridView.RowHeaderWidth = 70;
			this.matrixDataGridView.Size = new System.Drawing.Size(339, 298);
			this.matrixDataGridView.Sortable = true;
			this.matrixDataGridView.TabIndex = 2;
			this.matrixDataGridView.TableModel = null;
			this.matrixDataGridView.VisibleX = 0;
			this.matrixDataGridView.VisibleY = 0;
			// 
			// SelectRowsManuallyControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.matrixDataGridView);
			this.Controls.Add(this.toolStrip1);
			this.Name = "SelectRowsManuallyControl";
			this.Size = new System.Drawing.Size(339, 323);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton removeSelectedRowsButton;
		private System.Windows.Forms.ToolStripButton keepSelectedRowsButton;
		private BaseLib.Forms.Table.TableView matrixDataGridView;
	}
}
