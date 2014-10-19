using System.Collections.Generic;
using System.Windows;
using BaseLib.Param;
using PluginMzTab.Lib.Model;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Plugin.Param{
    public class SoftwareParam : Parameter{
        private readonly int _count;
        private readonly CVLookUp _cv;
        private readonly Dictionary<string, string> _parameters;
        private Software[] Default { get; set; }
        public Software[] Value { get; set; }

        public SoftwareParam(Software[] software, CVLookUp cv, Dictionary<string, string> parameters) : base(null){
            _count = software.Length;
            _cv = cv;
            _parameters = parameters;
            Value = software;
            Default = software;
        }

        public override void ResetValue(){
            Value = Default;
        }

        public override void ResetDefault(){
            Default = Value;
        }

        public override void SetValueFromControl(){
            SoftwarePanel p = control as SoftwarePanel;
            if (p == null){
                return;
            }
            Value = p.Value;
        }

        public override void UpdateControlFromValue(){
            SoftwarePanel p = control as SoftwarePanel;
            if (p == null){
                return;
            }
            p.Value = Value;
        }

        public override object Clone(){
            return new SoftwareParam(Value, _cv, _parameters){Default = Default};
        }

        public override void Clear(){
            Value = new Software[_count];
        }

        public override string StringValue { get; set; }

        public override bool IsModified { get { return Equals(Value, Default); } }

        protected override UIElement Control { get { return new SoftwarePanel(Value, _cv, _parameters); } }

        public override float Height { get { return SoftwarePanel.MiniumHeight; } }
    }
}