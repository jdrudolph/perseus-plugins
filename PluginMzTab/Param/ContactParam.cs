using System.Windows;
using BaseLib.Param;
using MzTabLibrary.model;
using PluginMzTab.utils;

namespace PluginMzTab.param{
	public class ContactParam : Parameter {
        private readonly CVLookUp _cv;
        private readonly int _count;        
        private Contact[] Default { get; set; }
        public Contact[] Value { get; set; }

        public static string Label = "contact count";

        public ContactParam(Contact[] contacts, CVLookUp cv)
            : base(null){
            _cv = cv;
            _count = contacts.Length;
            Value = contacts;
            Default = contacts;
        }

        public override void ResetValue(){
            Value = Default;
        }

        public override void ResetDefault(){
            Default = Value;
        }

        public override void SetValueFromControl(){
			ContactPanel p = control as ContactPanel;
			if (p == null){
				return;
			}
			Value = p.Value;
        }

        public override void UpdateControlFromValue(){
			ContactPanel p = control as ContactPanel;
			if (p == null){
				return;
			}
			p.Value = Value;
        }

        public override object Clone(){
            return new ContactParam(Value, _cv){Default = Default};
        }

        public override void Clear(){
            Value = new Contact[_count];
        }

        public override string StringValue { get; set; }

        public override bool IsModified { get { return Equals(Value, Default); } }

		protected override UIElement Control { get { return new ContactPanel(Value); } }

        public override float Height { get { return ContactPanel.MinimumHeight; } }
    }
}