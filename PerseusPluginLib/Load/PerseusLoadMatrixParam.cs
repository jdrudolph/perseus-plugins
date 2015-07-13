using System;
using BaseLibS.Num;
using BaseLibS.Param;
using BaseLibS.Util;

namespace PerseusPluginLib.Load{
	[Serializable]
	public class PerseusLoadMatrixParam : Parameter<string[]>{
		public string Filter { get; set; }
		[NonSerialized] private PerseusLoadMatrixControl control;

		public PerseusLoadMatrixParam(string name) : base(name){
			Value = new string[8];
			Default = new string[8];
			for (int i = 0; i < 8; i++){
				Value[i] = "";
				Default[i] = "";
			}
			Filter = null;
		}

		public override string StringValue{
			get { return StringUtils.Concat(";", Value); }
			set { Value = value.Split(';'); }
		}

		public override bool IsDropTarget{
			get { return true; }
		}

		public override void Drop(string x){
			UpdateFile(x);
		}

		public override void SetValueFromControl(){
			Value = control.Value;
		}

		public override void UpdateControlFromValue(){
			control.Value = Value;
		}

		public override void Clear(){
			Value = new string[8];
			for (int i = 0; i < 8; i++){
				Value[i] = "";
			}
		}

		private void UpdateFile(string filename){
			if (control == null){
				return;
			}
			control.UpdateFile(filename);
		}

		public override float Height{
			get { return 790; }
		}

		public override object CreateControl(){
			string[] items = Value[1].Length > 0 ? Value[1].Split(';') : new string[0];
			control = new PerseusLoadMatrixControl(items){Filter = Filter, Value = Value};
			return control;
		}

		public string Filename{
			get { return Value[0]; }
		}

		public string[] Items{
			get { return Value[1].Length > 0 ? Value[1].Split(';') : new string[0]; }
		}

		public override bool IsModified{
			get { return !ArrayUtils.EqualArrays(Default, Value); }
		}

		private int[] GetIntValues(int i){
			string x = Value[i + 2];
			string[] q = x.Length > 0 ? x.Split(';') : new string[0];
			int[] result = new int[q.Length];
			for (int i1 = 0; i1 < q.Length; i1++){
				result[i1] = int.Parse(q[i1]);
			}
			return result;
		}

		public int[] ExpressionColumnIndices{
			get { return GetIntValues(0); }
		}

		public int[] NumericalColumnIndices{
			get { return GetIntValues(1); }
		}

		public int[] CategoryColumnIndices{
			get { return GetIntValues(2); }
		}

		public int[] TextColumnIndices{
			get { return GetIntValues(3); }
		}

		public int[] MultiNumericalColumnIndices{
			get { return GetIntValues(4); }
		}

		public bool ShortenExpressionColumnNames{
			get { return bool.Parse(Value[7]); }
		}

		public override object Clone(){
			return new PerseusLoadMatrixParam(Name){
				Help = Help,
				Visible = Visible,
				Filter = Filter,
				Default = Default,
				Value = Value
			};
		}
	}
}