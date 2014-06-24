using System.Windows;
using BaseLib.Param;
using MzTabLibrary.model;
using PluginMzTab.utils;

namespace PluginMzTab.param{
	public class PublicationParam : Parameter {
        private readonly CVLookUp _cv;
        private readonly int _count;        
        private Publication[] Default { get; set; }
        public Publication[] Value { get; set; }

        public PublicationParam(Publication[] publications, CVLookUp cv)
            : base(null){
            _cv = cv;
            _count = publications.Length;
            Value = publications;
            Default = publications;
        }

        public override void ResetValue(){
            Value = Default;
        }

        public override void ResetDefault(){
            Default = Value;
        }

        public override void SetValueFromControl(){
			PublicationPanel p = control as PublicationPanel;
			if (p == null){
				return;
			}
			Value = p.Value;
        }

        public override void UpdateControlFromValue(){
			PublicationPanel p = control as PublicationPanel;
			if (p == null){
				return;
			}
			p.Value =Value;
        }

        public override object Clone(){
            return new PublicationParam(Value, _cv){Default = Default};
        }

        public override void Clear(){
            Value = new Publication[_count];
        }

        public override string StringValue { get; set; }

        public override bool IsModified { get { return Equals(Value, Default); } }

		protected override UIElement Control { get { return new PublicationPanel(Value); } }

        public override float Height { get { return PublicationPanel.MiniumHeight; } }
    }
}