using System.Windows;
using BaseLib.Param;
using MzTabLibrary.model;
using PluginMzTab.utils;

namespace PluginMzTab.param{
	public class SampleParam : Parameter {
        private readonly bool _reduced;
        private readonly CVLookUp _cv;
        private readonly int _count;        
        private Sample[] Default { get; set; }
        public Sample[] Value { get; set; }

        public SampleParam(Sample[] samples, bool reduced, CVLookUp cv, string name = null)
            : base(name){
            _reduced = reduced;
            _cv = cv;
            _count = samples == null ? 0 : samples.Length;
            Value = samples;
            Default = samples;
        }

        public override void ResetValue(){
            Value = Default;
        }

        public override void ResetDefault(){
            Default = Value;
        }

        public override void SetValueFromControl(){
			SamplePanel p = control as SamplePanel;
			if (p == null){
				return;
			}
			Value = p.Value;
        }

        public override void UpdateControlFromValue(){
			SamplePanel p = control as SamplePanel;
			if (p == null){
				return;
			}
			p.Value = Value;
        }

        public override object Clone(){
            return new SampleParam(Value, _reduced, _cv){Default = Default};
        }

        public override void Clear(){
            Value = new Sample[_count];
        }

        public override string StringValue { get; set; }

        public override bool IsModified { get { return Equals(Value, Default); } }

		protected override UIElement Control
		{
			get{
				return new SamplePanel(Value, _reduced, _cv);
			}
		}

        public override float Height { get { return SamplePanel.MinimumHeight(_count); } }
    }
}