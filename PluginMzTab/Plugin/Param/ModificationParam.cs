using System.Windows;
using BaseLib.Param;
using PluginMzTab.Lib.Model;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Plugin.Param{
    public class ModificationParam : Parameter{
        private readonly CVLookUp _cv;
        private readonly int _count;
        private Mod[] Default { get; set; }
        public Mod[] Value { get; set; }

        public ModificationParam(Mod[] contacts, CVLookUp cv)
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
            ModificationPanel p = control as ModificationPanel;
            if (p == null){
                return;
            }
            Value = p.Value;
        }

        public override void UpdateControlFromValue(){
            ModificationPanel p = control as ModificationPanel;
            if (p == null){
                return;
            }
            p.Value = Value;
        }

        public override object Clone(){
            return new ModificationParam(Value, _cv){Default = Default};
        }

        public override void Clear(){
            Value = new Mod[_count];
        }

        public override string StringValue { get; set; }

        public override bool IsModified { get { return Equals(Value, Default); } }

        protected override UIElement Control { get { return new ModificationPanel(Value, _cv); } }

        public override float Height { get { return ModificationPanel.MinimumHeight; } }
    }
}