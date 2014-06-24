using System.Windows;
using BaseLib.Param;
using MzTabLibrary.model;
using PluginMzTab.utils;

namespace PluginMzTab.param{
	public class StudyVariableParam : Parameter {
        private readonly CVLookUp _cv;
        private readonly int _count;
        private readonly bool _reduced;

        private StudyVariable[] Default { get; set; }
        public StudyVariable[] Value { get; set; }

        public StudyVariableParam(StudyVariable[] studyVariables, bool reduced, CVLookUp cv, string name = null)
            : base(name){
            _reduced = reduced;
            _cv = cv;
            _count = studyVariables.Length;
            Value = studyVariables;
            Default = studyVariables;
        }

        public override void ResetValue(){
            Value = Default;
        }

        public override void ResetDefault(){
            Default = Value;
        }

        public override void SetValueFromControl(){
			StudyVariablePanel p = control as StudyVariablePanel;
			if (p == null){
				return;
			}
			Value = p.Value;
        }

        public override void UpdateControlFromValue(){
			StudyVariablePanel p = control as StudyVariablePanel;
			if (p == null){
				return;
			}
			p.Value = Value;
        }

        public override object Clone(){
            return new StudyVariableParam(Value, _reduced, _cv, Name){Default = Default};
        }

        public override void Clear(){
            Value = new StudyVariable[_count];
        }

        public override string StringValue { get; set; }

        public override bool IsModified { get { return Equals(Value, Default); } }

		protected override UIElement Control
		{
			get{
				return _reduced ? (StudyVariablePanel) new StudyVariablePanel1(Value) : new StudyVariablePanel2(Value);
			}
        }

        public override float Height { get { return _reduced ? StudyVariablePanel1.MinimumHeight(_count) : StudyVariablePanel2.MinimumHeight(_count); } }
    }
}