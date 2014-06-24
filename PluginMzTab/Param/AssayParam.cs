using System.Windows;
using System.Windows.Controls;
using BaseLib.Param;
using MzTabLibrary.model;
using PluginMzTab.utils;

namespace PluginMzTab.param{
	public class AssayParam : Parameter {
        private readonly CVLookUp _cv;
        private readonly int _count;
        private readonly bool _reduced;

        private Assay[] Default { get; set; }
        public Assay[] Value { get; set; }

        public AssayParam(int count, Assay[] assays, bool reduced, CVLookUp cv, string name = null) : base(name){
            _reduced = reduced;
            _cv = cv;
            _count = count;
            Value = assays;
            Default = assays;
        }

        public override void ResetValue(){
            Value = Default;
        }

        public override void ResetDefault(){
            Default = Value;
        }

        public override void SetValueFromControl(){
			AssayPanel p = control as AssayPanel;
			if (p == null){
				return;
			}
			Value = p.Value;
        }

        public override void UpdateControlFromValue(){
			AssayPanel p = control as AssayPanel;
			if (p == null){
				return;
			}
			p.Value = Value;
        }

        public override object Clone(){
            return new AssayParam(_count, Value, _reduced, _cv){Default = Default};
        }

        public override void Clear(){
            Value = new Assay[_count];
        }

        public override string StringValue { get; set; }

        public override bool IsModified { get { return Equals(Value, Default); } }

		protected override UIElement Control
		{
			get{  
			    return _reduced ? (AssayPanel) new AssayPanel1(_count, Value, _cv) : new AssayPanel2(_count, Value, _cv);
			}
		}

        public override float Height { get { return _reduced ? AssayPanel1.MinimumHeight() : AssayPanel2.MinimumHeight(AssayPanel.UniqueSamples(Value).Length); } }
    }
}