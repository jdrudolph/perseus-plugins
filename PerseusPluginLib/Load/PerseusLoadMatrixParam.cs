using System;
using System.Windows;
using BaseLib.Param;
using BaseLib.Util;

namespace PerseusPluginLib.Load{
	[Serializable]
	public class PerseusLoadMatrixParam : Parameter{
		public string Filter { get; set; }
		public string[] Value { get; set; }
		public string[] Default { get; private set; }

		public PerseusLoadMatrixParam(string name) : base(name){
			Value = new string[7];
			for (int i = 0; i < 7; i++){
				Value[i] = "";
			}
			Default = Value;
			Filter = null;
		}

		public override string StringValue { get { return StringUtils.Concat(";", Value); } set { Value = value.Split(';'); } }
		public string[] Value2{
			get{
				SetValueFromControl();
				return Value;
			}
		}
		public override bool IsDropTarget { get { return true; } }

		public override void Drop(string x){
			UpdateFile(x);
		}

		public override void ResetValue(){
			Value = Default;
		}

		public override void ResetDefault(){
			Default = Value;
		}

		public override bool IsModified { get { return !Value.Equals(Default); } }

		public override void SetValueFromControl(){
			PerseusLoadParameterControl tb = (PerseusLoadParameterControl) control;
			Value = tb.Value;
		}

		public override void UpdateControlFromValue(){
			PerseusLoadParameterControl lfp = (PerseusLoadParameterControl) control;
			lfp.Value = Value;
		}

		public override void Clear(){
			Value = new string[7];
			for (int i = 0; i < 7; i++){
				Value[i] = "";
			}
		}

		private void UpdateFile(string filename){
			if (control == null){
				return;
			}
			PerseusLoadParameterControl tb = (PerseusLoadParameterControl) control;
			tb.UpdateFile(filename);
		}

		public override float Height { get { return 770; } }
		protected override FrameworkElement Control{
			get{
				string[] items = Value[1].Length > 0 ? Value[1].Split(';') : new string[0];
				return new PerseusLoadParameterControl(items){Filter = Filter, Value = Value};
			}
		}
		public string Filename { get { return Value[0]; } }
		public string[] Items { get { return Value[1].Length > 0 ? Value[1].Split(';') : new string[0]; } }

		private int[] GetIntValues(int i){
			string x = Value[i + 2];
			string[] q = x.Length > 0 ? x.Split(';') : new string[0];
			int[] result = new int[q.Length];
			for (int i1 = 0; i1 < q.Length; i1++){
				result[i1] = int.Parse(q[i1]);
			}
			return result;
		}

		public int[] ExpressionColumnIndices { get { return GetIntValues(0); } }
		public int[] NumericalColumnIndices { get { return GetIntValues(1); } }
		public int[] CategoryColumnIndices { get { return GetIntValues(2); } }
		public int[] TextColumnIndices { get { return GetIntValues(3); } }
		public int[] MultiNumericalColumnIndices { get { return GetIntValues(4); } }

		public override object Clone(){
			return new PerseusLoadMatrixParam(Name)
			{Help = Help, Visible = Visible, Filter = Filter, Default = Default, Value = Value};
		}
	}
}