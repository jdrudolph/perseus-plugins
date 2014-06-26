using System.Windows;
using BaseLib.Param;
using MzTabLibrary.model;
using PluginMzTab.utils;

namespace PluginMzTab.param
{
	public class InstrumentParam : Parameter
    {
        private readonly CVLookUp _cv;
        private readonly int _count;
        private Instrument[] Default { get; set; }
        public Instrument[] Value { get; set; }

        public InstrumentParam(Instrument[] instruments, CVLookUp cv) : base(null){
            _cv = cv;
            _count = instruments.Length;
            Value = instruments;
            Default = instruments;
        }

        public override void ResetValue(){
            Value = Default;
        }

        public override void ResetDefault(){
            Default = Value;
        }

        public override void SetValueFromControl(){
			InstrumentPanel p = control as InstrumentPanel;
			if (p == null){
				return;
			}
			Value = p.Value;
        }

        public override void UpdateControlFromValue(){
			InstrumentPanel p = control as InstrumentPanel;
			if (p == null){
				return;
			}
			p.Value = Value;            
        }

        public override object Clone(){
            return new InstrumentParam(Value, _cv){Default = Default};
        }

        public override void Clear(){
            Value = new Instrument[_count];
        }

        public override string StringValue { get; set; }

        public override bool IsModified { get { return Equals(Value, Default); } }

		protected override UIElement Control { get { return new InstrumentPanel(Value, _cv); } }

        public override float Height{
            get { return InstrumentPanel.MiniumHeight; }
        }
    }
}
