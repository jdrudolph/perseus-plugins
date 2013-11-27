namespace PerseusPluginLib.Load{
	partial class PerseusLoadMatrixParameterPanel {
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
			this.textBox = new System.Windows.Forms.TextBox();
			this.button = new System.Windows.Forms.Button();
			this.multiListSelector1 = new BaseLib.Forms.Select.MultiListSelector();
			this.SuspendLayout();
			// 
			// textBox
			// 
			this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox.Location = new System.Drawing.Point(3, 1);
			this.textBox.Name = "textBox";
			this.textBox.Size = new System.Drawing.Size(673, 20);
			this.textBox.TabIndex = 0;
			// 
			// button
			// 
			this.button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button.Location = new System.Drawing.Point(682, 0);
			this.button.Name = "button";
			this.button.Size = new System.Drawing.Size(75, 23);
			this.button.TabIndex = 1;
			this.button.Text = "Select";
			this.button.UseVisualStyleBackColor = true;
			this.button.Click += new System.EventHandler(this.ButtonClick);
			// 
			// multiListSelector1
			// 
			this.multiListSelector1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.multiListSelector1.Location = new System.Drawing.Point(3, 27);
			this.multiListSelector1.Name = "multiListSelector1";
			this.multiListSelector1.Size = new System.Drawing.Size(754, 489);
			this.multiListSelector1.TabIndex = 2;
			// 
			// PerseusLoadFileParameterPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.multiListSelector1);
			this.Controls.Add(this.button);
			this.Controls.Add(this.textBox);
			this.Name = "PerseusLoadFileParameterPanel";
			this.Size = new System.Drawing.Size(760, 519);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBox;
		private System.Windows.Forms.Button button;
		private BaseLib.Forms.Select.MultiListSelector multiListSelector1;
	}
}