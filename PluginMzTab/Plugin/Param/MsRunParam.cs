using System.Windows;
using BaseLib.Param;
using PluginMzTab.Plugin.Extended;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Plugin.Param{
    public class MsRunParam : Parameter{
        private readonly CVLookUp _cv;
        private readonly bool _compact;
        private readonly int _count;
        private MsRunImpl[] Default { get; set; }
        public MsRunImpl[] Value { get; set; }

        public MsRunParam(int groups, MsRunImpl[] msRunsImpl, CVLookUp cv, bool compact, string name = null)
            : base(name){
            _count = groups;
            _cv = cv;
            _compact = compact;
            Value = msRunsImpl;
            Default = msRunsImpl;
        }

        public override void ResetValue(){
            Value = Default;
        }

        public override void ResetDefault(){
            Default = Value;
        }

        public override void SetValueFromControl(){
            MsRunPanel p = control as MsRunPanel;
            if (p == null){
                return;
            }
            Value = p.Value;
        }

        public override void UpdateControlFromValue(){
            MsRunPanel p = control as MsRunPanel;
            if (p == null){
                return;
            }
            p.Value = Value;
        }

        public override object Clone(){
            return new MsRunParam(_count, Value, _cv, _compact);
        }

        public override void Clear(){
            Value = new MsRunImpl[_count];
        }

        public override string StringValue { get; set; }

        public override bool IsModified { get { return Equals(Value, Default); } }

        protected override UIElement Control { get { return _compact ? (MsRunPanel) new MsRunPanel1(_count, Value, _cv) : new MsRunPanel2(_count, Value, _cv); } }

        public override float Height { get { return _compact ? MsRunPanel1.MiniumHeight(_count) : MsRunPanel2.MiniumHeight(); } }
    }
}